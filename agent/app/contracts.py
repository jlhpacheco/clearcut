from typing import List, Optional, Literal
from pydantic import BaseModel, Field, field_validator
import re

class ReviewFinding(BaseModel):
    model_config = {"extra": "forbid"}

    finding_id: str = Field(..., description="Stable, unique identifier for the finding")
    category: Literal["brand_mark", "factual_claim", "music_cue"] = Field(..., description="Category of finding")
    start_seconds: float = Field(..., description="Start timestamp of the finding in seconds")
    end_seconds: Optional[float] = Field(None, description="Optional end timestamp of the finding in seconds")
    label: str = Field(..., description="Human-readable summary label of the finding")
    observation: str = Field(..., description="Neutral clearance-preparation observation")
    review_priority: Literal["routine", "attention", "priority"] = Field(..., description="Priority level")
    research_objective: str = Field(..., description="Agent research objective for the finding")

    @field_validator("finding_id")
    @classmethod
    def validate_non_empty_id(cls, v: str) -> str:
        if not v or not v.strip():
            raise ValueError("finding_id must be a non-empty string.")
        return v

    @field_validator("start_seconds")
    @classmethod
    def validate_start_seconds(cls, v: float) -> float:
        if v < 0:
            raise ValueError("start_seconds must be non-negative.")
        if v > 45.0:
            raise ValueError("start_seconds is outside the permitted clip duration (max 45 seconds).")
        return v

    @field_validator("end_seconds")
    @classmethod
    def validate_end_seconds(cls, v: Optional[float], info) -> Optional[float]:
        if v is not None:
            if v < 0:
                raise ValueError("end_seconds must be non-negative.")
            if v > 45.0:
                raise ValueError("end_seconds is outside the permitted clip duration (max 45 seconds).")
            # Get start_seconds from other fields if possible
            start_seconds = info.data.get("start_seconds")
            if start_seconds is not None and v < start_seconds:
                raise ValueError("end_seconds cannot be earlier than start_seconds.")
        return v

    @field_validator("label", "observation", "research_objective")
    @classmethod
    def validate_non_empty_strings(cls, v: str) -> str:
        if not v or not v.strip():
            raise ValueError("String fields must be non-empty and non-blank.")
        return v

class AnalysisResponse(BaseModel):
    model_config = {"extra": "forbid"}
    findings: List[ReviewFinding]

class EvidenceSource(BaseModel):
    model_config = {"extra": "forbid"}

    title: str = Field(..., description="Title of the cited source page")
    publisher: str = Field(..., description="Publisher name or domain of the source")
    retrieval_date: str = Field(..., description="Standard UTC or local date when source was fetched")
    relevance_summary: str = Field(..., description="Brief summary of the source's relevance to the finding")
    url: str = Field(..., description="Direct verified source URL")

    @field_validator("url")
    @classmethod
    def validate_http_url(cls, v: str) -> str:
        if not (v.startswith("http://") or v.startswith("https://")):
            raise ValueError("url must be a valid HTTP or HTTPS URL.")
        return v

class ResearchEvent(BaseModel):
    model_config = {"extra": "forbid"}

    status: Literal["preparing", "searching", "reviewing", "ready", "incomplete"] = Field(..., description="Current status of the research")
    task: Optional[str] = Field(None, description="Descriptive task details prepared by the agent")
    evidence: Optional[List[EvidenceSource]] = Field(None, description="List of evidence sources returned when ready")
    error: Optional[str] = Field(None, description="Detailed error message if status is incomplete")
    session_id: Optional[str] = Field(None, description="Stable session ID to prove the call")
    search_id: Optional[str] = Field(None, description="The unique search ID returned by the search service")
    retrieval_time: Optional[str] = Field(None, description="ISO timestamp of the search retrieval")
