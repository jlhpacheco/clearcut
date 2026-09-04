import json
import logging
import uuid
from typing import Optional
from fastapi import FastAPI, HTTPException
from fastapi.responses import StreamingResponse
from pydantic import BaseModel, Field, field_validator
from app.contracts import AnalysisResponse, ResearchEvent
from app.settings import settings
from app.video_analysis import analyze_video
from app.agent import ResearchAgent

logger = logging.getLogger(__name__)

app = FastAPI(title="ClearCut Agent Service", version="1.0.0")
agent = ResearchAgent()

# Enforce fail-closed production fixture lockout at startup
if settings.ENVIRONMENT.lower() == "production" and settings.USE_FIXTURES:
    raise RuntimeError(
        "CRITICAL SECURITY VIOLATION: Fixture mode is enabled in a production environment. "
        "The system must fail closed."
    )

class ResearchRequest(BaseModel):
    model_config = {"extra": "forbid"}
    finding_id: str = Field(..., max_length=100, description="The ID of the finding to research")
    label: str = Field(..., max_length=250, description="Human-readable summary label of the finding")
    observation: str = Field(..., max_length=2000, description="Neutral clearance-preparation observation")
    research_objective: str = Field(..., max_length=2000, description="Agent research objective for the finding")
    session_id: Optional[str] = Field(None, max_length=100, description="Optional stable session ID")

    @field_validator("finding_id", "label", "observation", "research_objective")
    @classmethod
    def strip_and_validate_strings(cls, v: str) -> str:
        if v is not None:
            v = v.strip()
        if not v:
            raise ValueError("String fields must be non-empty and non-blank.")
        return v

    @field_validator("session_id")
    @classmethod
    def strip_session_id(cls, v: Optional[str]) -> Optional[str]:
        if v is not None:
            v = v.strip()
            if not v:
                return None
        return v

@app.get("/health")
def health_check():
    # Enforce strict fail-closed state
    settings.validate()
    return {
        "status": "healthy",
        "environment": settings.ENVIRONMENT,
        "use_fixtures": settings.USE_FIXTURES
    }

@app.post("/v1/analyze", response_model=AnalysisResponse)
async def analyze():
    # Enforce strict project setting validations and fail-closed checks
    settings.validate()
    try:
        # Analyze the configured GCS video URI
        return await analyze_video(settings.DEMO_VIDEO_GCS_URI)
    except Exception as exc:
        logger.error("Video analysis failed", extra={"error_type": type(exc).__name__})
        raise HTTPException(status_code=500, detail="Video analysis failed. Service unavailable.")

async def generate_research_events(req: ResearchRequest):
    session_id = req.session_id or str(uuid.uuid4())
    objective = req.research_objective
    finding_id = req.finding_id

    # 1. Stage: Preparing research task (no invented query claims)
    yield json.dumps({
        "status": "preparing",
        "task": f"Preparing research task for objective: '{objective}'."
    }) + "\n"

    # 2. Stage: Searching (says ADK will formulate queries and invoke one Parallel search, without claiming completion)
    yield json.dumps({
        "status": "searching",
        "task": "Google ADK will formulate 1 to 3 search queries and invoke the parallel search tool exactly once."
    }) + "\n"

    try:
        # Call the real backend search operation
        result = await agent.run_research(finding_id, objective, session_id)
        
        # 3. Stage: Reviewing sources (only after success)
        yield json.dumps({
            "status": "reviewing",
            "task": f"Retrieved {len(result['evidence'])} evidence sources. Normalizing and deduplicating HTTP(S) URLs."
        }) + "\n"

        # Serialize list of evidence sources
        evidence_dicts = [ev.model_dump() for ev in result["evidence"]]
        
        # 4. Stage: Evidence ready (Returns results)
        yield json.dumps({
            "status": "ready",
            "task": f"Research complete. {len(evidence_dicts)} sources ready.",
            "evidence": evidence_dicts,
            "session_id": result.get("session_id"),
            "search_id": result.get("search_id"),
            "retrieval_time": result.get("retrieval_time"),
            "queries": result.get("queries"),
            "objective": result.get("objective")
        }) + "\n"
        
    except Exception as exc:
        logger.error("Research execution failed", extra={"error_type": type(exc).__name__, "session_id": session_id})
        yield json.dumps({
            "status": "incomplete",
            "task": "Research execution failed.",
            "error": "An error occurred during the research process. Please try again later."
        }) + "\n"

@app.post("/v1/research/stream")
async def research_stream(req: ResearchRequest):
    settings.validate()
    return StreamingResponse(
        generate_research_events(req),
        media_type="application/x-ndjson"
    )
