import pytest
from app.parallel_search import get_fixture_evidence
from app.contracts import EvidenceSource

def test_get_fixture_evidence_returns_correct_sources():
    # Act: Retrieve fixture evidence for brand mark finding
    sources = get_fixture_evidence("find-01-brand")
    
    # Assert
    assert len(sources) == 2
    assert all(isinstance(src, EvidenceSource) for src in sources)
    assert sources[0].publisher == "uspto.gov"
    assert sources[1].publisher == "wipo.int"
    
def test_get_fixture_evidence_returns_lumaleaf_evidence_with_token():
    # Act: Retrieve fixture evidence for factual claim
    sources = get_fixture_evidence("find-02-claim")
    
    # Assert
    assert len(sources) == 1
    assert "CC-EVID-9F4D" in sources[0].relevance_summary
    assert sources[0].publisher == "clearcut.web"
    assert sources[0].url == "http://localhost:5000/evidence/lumaleaf-energy-study"

def test_get_fixture_evidence_for_invalid_finding_id_returns_empty():
    sources = get_fixture_evidence("find-non-existent")
    assert len(sources) == 0
