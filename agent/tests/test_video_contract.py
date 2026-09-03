import os
import json
import pytest
from app.contracts import ReviewFinding, EvidenceSource

def get_contracts_path():
    curr = os.path.abspath(os.path.dirname(__file__))
    while curr:
        path = os.path.join(curr, "contracts", "golden-review.json")
        if os.path.exists(path):
            return path
        parent = os.path.dirname(curr)
        if parent == curr:
            break
        curr = parent
    raise FileNotFoundError("Could not locate contracts/golden-review.json in any parent folder.")

def test_pydantic_schema_deserializes_golden_contract():
    # Arrange
    contract_path = get_contracts_path()
    with open(contract_path, "r", encoding="utf-8") as f:
        data = json.load(f)
        
    findings_list = data["findings"]
    evidence_list = data["evidence_sources"]
    
    # Act: Deserialize
    findings = [ReviewFinding.model_validate(f) for f in findings_list]
    evidence = [EvidenceSource.model_validate(e) for e in evidence_list]
    
    # Assert Findings
    assert len(findings) == 3
    assert findings[0].finding_id == "find-01-brand"
    assert findings[0].category == "brand_mark"
    assert findings[0].start_seconds == 4.5
    assert findings[0].end_seconds == 12.0
    assert findings[0].label == "LumaLeaf Logo"
    assert "LumaLeaf Energy" in findings[0].observation
    assert findings[0].review_priority == "attention"
    assert "Determine if" in findings[0].research_objective

    # Assert Evidence
    assert len(evidence) == 1
    assert evidence[0].title == "LumaLeaf Fictional Energy Study"
    assert evidence[0].publisher == "clearcut.web"
    assert "CC-EVID-9F4D" in evidence[0].relevance_summary
    assert evidence[0].url == "http://localhost:5000/evidence/lumaleaf-energy-study"
