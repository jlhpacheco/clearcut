import os
import json
import logging
from typing import List
from app.contracts import ReviewFinding, AnalysisResponse
from app.settings import settings

logger = logging.getLogger(__name__)

def get_fixture_findings() -> List[ReviewFinding]:
    # Load from shared contract file contracts/golden-review.json
    path_to_fixture = os.path.abspath(
        os.path.join(os.path.dirname(__file__), "..", "..", "contracts", "golden-review.json")
    )
    if not os.path.exists(path_to_fixture):
        # Fallback if path is different (e.g. during running in Docker)
        path_to_fixture = os.path.abspath(
            os.path.join(os.path.dirname(__file__), "golden-review.json")
        )
        if not os.path.exists(path_to_fixture):
            path_to_fixture = "/app/contracts/golden-review.json"
            
    with open(path_to_fixture, "r", encoding="utf-8") as f:
        data = json.load(f)
        
    findings = [ReviewFinding(**item) for item in data["findings"]]
    return validate_and_sort_findings(findings)

def validate_and_sort_findings(findings: List[ReviewFinding]) -> List[ReviewFinding]:
    # 1. Reject duplicate finding IDs
    seen_ids = set()
    for f in findings:
        if f.finding_id in seen_ids:
            raise ValueError(f"Duplicate finding_id found: {f.finding_id}")
        seen_ids.add(f.finding_id)

    # 2. Sort chronologically by start_seconds
    sorted_findings = sorted(findings, key=lambda x: x.start_seconds)

    # 3. Golden path verification: must contain exactly one fictional brand-artwork,
    # one precise factual product-claim, and one original-music cue
    categories_present = [f.category for f in sorted_findings]
    if len(sorted_findings) != 3:
        raise ValueError(f"Golden review requires exactly three findings, but got {len(sorted_findings)}.")

    required_categories = {"brand_mark", "factual_claim", "music_cue"}
    if set(categories_present) != required_categories:
        raise ValueError(
            f"Golden review findings must have exactly one of each category {required_categories}. "
            f"Got: {categories_present}"
        )

    return sorted_findings

async def analyze_video(video_uri: str) -> AnalysisResponse:
    if settings.USE_FIXTURES:
        return AnalysisResponse(findings=get_fixture_findings())
        
    # Real-mode Vertex AI / Gemini structured call
    try:
        from google import genai
        from google.genai import types
        
        # Enforce location us-central1 and project clearcut-agentic-20260901
        client = genai.Client(
            vertexai=True,
            project=settings.GOOGLE_CLOUD_PROJECT,
            location=settings.GOOGLE_CLOUD_LOCATION
        )
        
        prompt = (
            "You are an expert film clearance preparation assistant. "
            "Analyze the provided commercial video and audio track for trademark brand marks, factual claims, and music cues. "
            "Omit speculative findings. For each candidate finding, output exact start_seconds, end_seconds, "
            "neutral clearance observations (without making legal clearance claims), priority level, "
            "and a suggested research objective. Return the data structured according to the schema."
        )
        
        # Multimodal request to Gemini on Vertex AI
        response = client.models.generate_content(
            model=settings.GEMINI_MODEL,
            contents=[
                types.Part.from_uri(file_uri=video_uri, mime_type="video/mp4"),
                prompt
            ],
            config=types.GenerateContentConfig(
                response_mime_type="application/json",
                response_schema=AnalysisResponse,
            )
        )
        
        # Parse and validate response
        parsed = AnalysisResponse.model_validate_json(response.text)
        validated_findings = validate_and_sort_findings(parsed.findings)
        return AnalysisResponse(findings=validated_findings)
        
    except ImportError:
        raise RuntimeError("Google GenAI SDK is not installed in the environment. Please run in fixture mode or install google-genai.")
    except Exception as e:
        logger.error(
            "Gemini video analysis failed.",
            extra={
                "exception_class": type(e).__name__
            }
        )
        raise RuntimeError("Gemini video analysis failed. Service unavailable.")
