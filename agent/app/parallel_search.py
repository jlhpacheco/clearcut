import httpx
import datetime
import uuid
import logging
from typing import List, Optional, Dict, Any
from app.contracts import EvidenceSource
from app.settings import settings

logger = logging.getLogger(__name__)

def get_fixture_evidence(finding_id: str) -> List[EvidenceSource]:
    # Use dynamic retrieval date matching current UTC date
    current_date = datetime.datetime.now(datetime.timezone.utc).date().isoformat()
    
    evidence_map = {
        "find-01-brand": [
            EvidenceSource(
                title="United States Patent and Trademark Office TESS Database",
                publisher="uspto.gov",
                retrieval_date=current_date,
                relevance_summary="No active trademark registrations found for 'LumaLeaf' or 'LumaLeaf Energy' under class 042 (Energy).",
                url="https://www.uspto.gov/trademarks"
            ),
            EvidenceSource(
                title="Global Brand Database",
                publisher="wipo.int",
                retrieval_date=current_date,
                relevance_summary="No active international trademark filings found matching 'LumaLeaf' with a stylized leaf emblem.",
                url="https://www.wipo.int/reference/en/branddb/"
            )
        ],
        "find-02-claim": [
            EvidenceSource(
                title="LumaLeaf Fictional Energy Study Page",
                publisher="clearcut.web",
                retrieval_date=current_date,
                relevance_summary="Contains the unique verification token CC-EVID-9F4D. Explicitly states that LumaLeaf Energy and its 76% comparison claims are entirely fictional demonstration data.",
                url="http://localhost:5000/evidence/lumaleaf-energy-study"
            )
        ],
        "find-03-music": [
            EvidenceSource(
                title="APM Music Search and Licensing",
                publisher="apmmusic.com",
                retrieval_date=current_date,
                relevance_summary="No audio matches found in the APM production music catalogs for this background track. Music cue is likely an original custom-composed track.",
                url="https://www.apmmusic.com"
            ),
            EvidenceSource(
                title="Shazam Audio Fingerprinting Service",
                publisher="shazam.com",
                retrieval_date=current_date,
                relevance_summary="No matches found in the commercial music catalog. Supports the finding that this is an original or unreleased composition.",
                url="https://www.shazam.com"
            )
        ]
    }
    return evidence_map.get(finding_id, [])

async def execute_parallel_search(
    queries: List[str],
    objective: str,
    session_id: str,
    max_results: int = 5,
    source_policy: Optional[Dict[str, Any]] = None
) -> Dict[str, Any]:
    current_date = datetime.datetime.now(datetime.timezone.utc).date().isoformat()
    retrieval_time = datetime.datetime.now(datetime.timezone.utc).isoformat()
    
    if settings.USE_FIXTURES:
        # If in fixture mode, this helper won't run, but we return empty for safety
        return {
            "evidence": [],
            "search_id": f"srch_fix_{str(uuid.uuid4())[:8]}",
            "retrieval_time": retrieval_time
        }

    headers = {
        "x-api-key": settings.PARALLEL_API_KEY or "",
        "Content-Type": "application/json"
    }
    
    # Correct Parallel Search API v1 request/response handling schema exactly
    payload = {
        "search_queries": queries,
        "objective": objective,
        "mode": "advanced",
        "max_chars_total": 5000,
        "client_model": settings.GEMINI_MODEL,
        "session_id": session_id,
        "advanced_settings": {
            "max_results": max_results,
            "excerpt_settings": {}
        }
    }
    
    if source_policy is not None:
        payload["advanced_settings"]["source_policy"] = source_policy
        
    try:
        # EXACTLY ONE POST, NO RETRIES! Timeout set to 10.0 seconds.
        async with httpx.AsyncClient(timeout=10.0) as client:
            response = await client.post(
                "https://api.parallel.ai/v1/search",
                headers=headers,
                json=payload
            )
            response.raise_for_status()
            data = response.json()
            
            search_id = data.get("search_id") or f"srch-{uuid.uuid4().hex[:12]}"
            
            # Map external Parallel Search results to our EvidenceSource contract
            sources: List[EvidenceSource] = []
            seen_urls = set()
            
            results = data.get("results", [])
            for res in results:
                url = res.get("url", "")
                if not url:
                    continue
                    
                # Normalize and deduplicate only returned HTTP(S) URLs
                if not (url.startswith("http://") or url.startswith("https://")):
                    continue
                    
                if url in seen_urls:
                    continue
                seen_urls.add(url)
                
                # Fetch publisher from host domain
                publisher = url.split("//")[-1].split("/")[0]
                
                # Parse excerpts array
                excerpts = res.get("excerpts", [])
                relevance_summary = " ".join(excerpts) if excerpts else res.get("snippet", res.get("excerpt", ""))
                
                sources.append(EvidenceSource(
                    title=res.get("title", "Untitled Source"),
                    publisher=publisher,
                    retrieval_date=current_date,
                    relevance_summary=relevance_summary or "No summary available.",
                    url=url
                ))
                
                if len(sources) >= max_results:
                    break
                    
            return {
                "evidence": sources,
                "search_id": search_id,
                "retrieval_time": retrieval_time
            }
            
    except Exception as e:
        # Sanitized structured logging and stable client-safe errors
        logger.error(
            "Parallel Search API request failed.",
            extra={
                "exception_class": type(e).__name__,
                "session_id": session_id
            }
        )
        raise RuntimeError("Evidence search failed. Service unavailable.")
