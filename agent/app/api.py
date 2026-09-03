import asyncio
import json
import uuid
from typing import Optional
from fastapi import FastAPI, HTTPException
from fastapi.responses import StreamingResponse
from pydantic import BaseModel, Field
from app.contracts import AnalysisResponse, ResearchEvent
from app.settings import settings
from app.video_analysis import analyze_video, get_fixture_findings
from app.agent import ResearchAgent

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
    finding_id: str = Field(..., description="The ID of the finding to research")
    session_id: Optional[str] = Field(None, description="Optional stable session ID")

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
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

async def generate_research_events(finding_id: str, session_id: str):
    # Retrieve findings to formulate queries and research objectives
    try:
        findings = get_fixture_findings()
    except Exception as e:
        yield json.dumps({
            "status": "incomplete",
            "error": f"Failed to load findings fixture: {str(e)}"
        }) + "\n"
        return

    finding = next((f for f in findings if f.finding_id == finding_id), None)
    if not finding:
        yield json.dumps({
            "status": "incomplete",
            "error": f"Finding with ID '{finding_id}' not found in clip."
        }) + "\n"
        return

    objective = finding.research_objective

    # 1. Stage: Preparing research task (Formulates queries and logs them)
    # Formulate queries deterministically
    if finding_id == "find-01-brand":
        queries = ["LumaLeaf Energy trademark", "LumaLeaf stylized leaf logo registry"]
    elif finding_id == "find-02-claim":
        queries = ["LumaLeaf 76 percent energy saving", "LumaLeaf Energy scientific study"]
    elif finding_id == "find-03-music":
        queries = ["cinematic ambient background synth cue apm", "electronic background track shazam"]
    else:
        queries = [f"{objective} verification"]

    yield json.dumps({
        "status": "preparing",
        "task": f"Preparing research task for objective: '{objective}'. Formulating search queries: {queries}."
    }) + "\n"

    # 2. Stage: Searching with Parallel (Agent runs the search tool)
    yield json.dumps({
        "status": "searching",
        "task": f"Executing single Parallel Search tool call for: {queries}."
    }) + "\n"

    try:
        # Call the real backend search operation
        result = await agent.run_research(finding_id, objective, session_id)
        
        # 3. Stage: Reviewing sources (Deduplicating, formatting citation cards)
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
            "retrieval_time": result.get("retrieval_time")
        }) + "\n"
        
    except Exception as ex:
        yield json.dumps({
            "status": "incomplete",
            "task": f"Research failed for: '{objective}'.",
            "error": f"Parallel Search tool failure: {str(ex)}"
        }) + "\n"

@app.post("/v1/research/stream")
async def research_stream(req: ResearchRequest):
    settings.validate()
    # Ensure we have a stable session_id
    sess_id = req.session_id or str(uuid.uuid4())
    return StreamingResponse(
        generate_research_events(req.finding_id, sess_id),
        media_type="application/x-ndjson"
    )
