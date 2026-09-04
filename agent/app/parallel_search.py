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
                relevance_summary="This is demonstration evidence and does not represent an executed search. This card points a reviewer to an official search starting point; a human must perform and document the search. No clearance conclusion is implied.",
                url="https://www.uspto.gov/trademarks"
            ),
            EvidenceSource(
                title="Global Brand Database",
                publisher="wipo.int",
                retrieval_date=current_date,
                relevance_summary="This is demonstration evidence and does not represent an executed search. This card points a reviewer to an official search starting point; a human must perform and document the search. No clearance conclusion is implied.",
                url="https://www.wipo.int/reference/en/branddb/"
            )
        ],
        "find-02-claim": [
            EvidenceSource(
                title="LumaLeaf Fictional Energy Study Page",
                publisher="clearcut.web",
                retrieval_date=current_date,
                relevance_summary="This is demonstration evidence containing the verification token CC-EVID-9F4D. LumaLeaf and its 76% claim are fictional demonstration content. No search was executed and no clearance conclusion is implied.",
                url="http://localhost:5000/evidence/lumaleaf-energy-study"
            )
        ],
        "find-03-music": [
            EvidenceSource(
                title="APM Music Search and Licensing",
                publisher="apmmusic.com",
                retrieval_date=current_date,
                relevance_summary="This is demonstration evidence illustrating possible catalog research. A human must perform and document the search; this demo makes no ownership or licensing conclusion, and no search was executed.",
                url="https://www.apmmusic.com"
            ),
            EvidenceSource(
                title="Shazam Audio Fingerprinting Service",
                publisher="shazam.com",
                retrieval_date=current_date,
                relevance_summary="This is demonstration evidence illustrating possible fingerprinting research. A human must perform and document the search; this demo makes no ownership or licensing conclusion, and no search was executed.",
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

    if not isinstance(queries, list):
        raise ValueError("Queries must be a list.")
    trimmed_queries = [q.strip() for q in queries if isinstance(q, str) and q.strip()]
    if not (1 <= len(trimmed_queries) <= 3):
        raise ValueError("Must provide between 1 and 3 nonblank queries.")

    if not objective or not isinstance(objective, str) or not objective.strip():
        raise ValueError("Objective must be a nonblank string.")
    trimmed_objective = objective.strip()

    if not session_id or not isinstance(session_id, str) or not session_id.strip():
        raise ValueError("Session ID must be a nonblank string.")
    trimmed_session_id = session_id.strip()

    if settings.USE_FIXTURES:
        return {
            "evidence": [],
            "search_id": f"srch_fix_{str(uuid.uuid4())[:8]}",
            "session_id": trimmed_session_id,
            "retrieval_time": retrieval_time,
            "objective": trimmed_objective,
            "queries": trimmed_queries
        }

    headers = {
        "x-api-key": settings.PARALLEL_API_KEY or "",
        "Content-Type": "application/json"
    }
    
    payload = {
        "search_queries": trimmed_queries,
        "objective": trimmed_objective,
        "session_id": trimmed_session_id,
        "mode": "basic",
        "max_chars_total": 3600,
        "client_model": settings.GEMINI_MODEL,
        "advanced_settings": {
            "max_results": min(max_results, 3),
            "excerpt_settings": {
                "max_chars_per_result": 1200
            }
        }
    }
    
    if source_policy is not None:
        payload["advanced_settings"]["source_policy"] = source_policy
        
    try:
        async with httpx.AsyncClient(timeout=10.0) as client:
            response = await client.post(
                "https://api.parallel.ai/v1/search",
                headers=headers,
                json=payload
            )
            response.raise_for_status()
            data = response.json()
            
            search_id = data.get("search_id")
            returned_session_id = data.get("session_id")
            if not search_id or not returned_session_id:
                raise RuntimeError("Missing search_id or session_id in response.")
            if returned_session_id != trimmed_session_id:
                raise RuntimeError("Returned session_id conflicts with requested session_id.")

            sources: List[EvidenceSource] = []
            seen_urls = set()
            
            results = data.get("results", [])
            for res in results:
                url = res.get("url")
                if not url or not isinstance(url, str):
                    continue
                url = url.strip()

                if not (url.startswith("http://") or url.startswith("https://")):
                    continue

                normalized_url = url.rstrip('/')
                if normalized_url in seen_urls:
                    continue
                seen_urls.add(normalized_url)

                try:
                    publisher = url.split("//")[-1].split("/")[0]
                except Exception:
                    publisher = "unknown"

                excerpts = res.get("excerpts")
                if isinstance(excerpts, list):
                    relevance_summary = " ".join([str(e) for e in excerpts if e])
                else:
                    relevance_summary = res.get("snippet") or res.get("excerpt") or ""

                if not relevance_summary:
                    relevance_summary = "No summary available."

                sources.append(EvidenceSource(
                    title=res.get("title") or "Untitled Source",
                    publisher=publisher,
                    retrieval_date=current_date,
                    relevance_summary=relevance_summary,
                    url=url
                ))

                if len(sources) >= 3:
                    break

            if not sources:
                raise RuntimeError("No usable evidence sources found.")

            return {
                "evidence": sources,
                "search_id": search_id,
                "session_id": returned_session_id,
                "retrieval_time": retrieval_time,
                "objective": trimmed_objective,
                "queries": trimmed_queries
            }

    except Exception as e:
        logger.error(
            "Parallel Search API request failed.",
            extra={
                "exception_class": type(e).__name__,
                "session_id": trimmed_session_id
            }
        )
        raise RuntimeError("Evidence search failed. Service unavailable.")
