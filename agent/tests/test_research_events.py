import asyncio
import json
from app.api import generate_research_events

def test_research_stream_events_sequence():
    # Arrange
    events = []
    
    # Act: Run the asynchronous stream generator using standard asyncio event loop
    async def collect():
        async for ev_str in generate_research_events("find-02-claim", "session-123"):
            if ev_str.strip():
                events.append(json.loads(ev_str.strip()))
                
    asyncio.run(collect())
    
    # Assert standard 4-stage progression
    assert len(events) == 4
    
    # Stage 1
    assert events[0]["status"] == "preparing"
    assert "Preparing research task" in events[0]["task"]
    
    # Stage 2
    assert events[1]["status"] == "searching"
    
    # Stage 3
    assert events[2]["status"] == "reviewing"
    
    # Stage 4
    assert events[3]["status"] == "ready"
    assert "evidence" in events[3]
    assert len(events[3]["evidence"]) == 1
    assert "CC-EVID-9F4D" in events[3]["evidence"][0]["relevance_summary"]
    assert events[3]["evidence"][0]["publisher"] == "clearcut.web"
    assert events[3]["evidence"][0]["url"] == "http://localhost:5000/evidence/lumaleaf-energy-study"

def test_research_stream_invalid_finding_id():
    events = []
    async def collect():
        async for ev_str in generate_research_events("find-invalid", "session-123"):
            if ev_str.strip():
                events.append(json.loads(ev_str.strip()))
                
    asyncio.run(collect())
    
    # Should emit an incomplete event and stop
    assert len(events) == 1
    assert events[0]["status"] == "incomplete"
    assert "not found in clip" in events[0]["error"]
