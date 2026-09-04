import asyncio
import json
import pytest
from pydantic import ValidationError
from app.api import generate_research_events, ResearchRequest
from app.settings import settings

def test_research_request_validation():
    # Pydantic rejects blank required field
    with pytest.raises(ValidationError):
        ResearchRequest(
            finding_id=" ",
            label="Test Label",
            observation="Test Observation",
            research_objective="Test Objective",
            session_id="session-123"
        )
    # Pydantic rejects extra field
    with pytest.raises(ValidationError):
        ResearchRequest(
            finding_id="find-02-claim",
            label="Test Label",
            observation="Test Observation",
            research_objective="Test Objective",
            session_id="session-123",
            extra_field="not allowed"
        )

@pytest.mark.asyncio
async def test_research_stream_events_sequence(monkeypatch):
    monkeypatch.setattr(settings, "USE_FIXTURES", True)
    req = ResearchRequest(
        finding_id="find-02-claim",
        label="LumaLeaf Claim",
        observation="LumaLeaf claims 76% energy savings.",
        research_objective="Verify the 76% energy savings claim.",
        session_id="session-123"
    )
    events = []
    async for ev_str in generate_research_events(req):
        if ev_str.strip():
            events.append(json.loads(ev_str.strip()))

    assert len(events) == 4
    assert events[0]["status"] == "preparing"
    assert "Preparing research task" in events[0]["task"]
    assert "complete" not in events[0]["task"].lower()
    assert "retrieved" not in events[0]["task"].lower()

    assert events[1]["status"] == "searching"
    assert "complete" not in events[1]["task"].lower()
    assert "retrieved" not in events[1]["task"].lower()

    assert events[2]["status"] == "reviewing"

    assert events[3]["status"] == "ready"
    assert "objective" in events[3]
    assert "queries" in events[3]
    assert "session_id" in events[3]
    assert "search_id" in events[3]
    assert "retrieval_time" in events[3]
    assert "evidence" in events[3]

    evidence = events[3]["evidence"]
    assert len(evidence) == 1
    assert "CC-EVID-9F4D" in evidence[0]["relevance_summary"]
