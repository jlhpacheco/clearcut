import pytest
import httpx
import json
from app.parallel_search import get_fixture_evidence, execute_parallel_search
from app.contracts import EvidenceSource
from app.settings import settings

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

def test_fixture_summaries_semantics():
    for fid in ["find-01-brand", "find-02-claim", "find-03-music"]:
        sources = get_fixture_evidence(fid)
        for src in sources:
            summary = src.relevance_summary.lower()
            # Avoid executed-result claims
            assert "executed" in summary
            # Contain demo/no-executed-search/no-conclusion semantics
            assert "demo" in summary or "demonstration" in summary
            assert "does not represent an executed search" in summary or "no search was executed" in summary
            assert "no clearance conclusion" in summary or "no ownership or licensing conclusion" in summary

@pytest.mark.asyncio
async def test_offline_live_success(monkeypatch):
    monkeypatch.setattr(settings, "USE_FIXTURES", False)
    monkeypatch.setattr(settings, "PARALLEL_API_KEY", "test-api-key")
    monkeypatch.setattr(settings, "GEMINI_MODEL", "gemini-test-model")

    original_async_client = httpx.AsyncClient
    request_captured = []
    request_count = 0

    async def mock_handler(request: httpx.Request) -> httpx.Response:
        nonlocal request_count
        request_count += 1
        request_captured.append(request)

        response_data = {
            "search_id": "srch_123",
            "session_id": "session-123",
            "results": [
                {
                    "url": "https://valid1.com/path/",
                    "title": "Valid 1",
                    "excerpts": ["Excerpt 1a", "Excerpt 1b"]
                },
                {
                    "url": "https://valid1.com/path",  # trailing-slash dedup
                    "title": "Valid 1 duplicate",
                    "excerpts": ["Duplicate"]
                },
                {
                    "url": "ftp://invalid-scheme.com",  # invalid URL filtering
                    "title": "Invalid Scheme",
                    "excerpts": ["Invalid"]
                },
                {
                    "url": "https://valid2.com",
                    "title": "Valid 2",
                    "excerpts": ["Excerpt 2"]
                },
                {
                    "url": "https://valid3.com",
                    "title": "Valid 3",
                    "excerpts": ["Excerpt 3"]
                },
                {
                    "url": "https://valid4.com",  # frugal caps (max 3)
                    "title": "Valid 4",
                    "excerpts": ["Excerpt 4"]
                }
            ]
        }
        return httpx.Response(200, json=response_data, request=request)

    def async_client_factory(*args, **kwargs):
        timeout = kwargs.get("timeout", 10.0)
        transport = httpx.MockTransport(mock_handler)
        return original_async_client(transport=transport, timeout=timeout)

    monkeypatch.setattr(httpx, "AsyncClient", async_client_factory)

    # Act
    result = await execute_parallel_search(
        queries=["query1", "query2"],
        objective="test objective",
        session_id="session-123",
        max_results=5
    )

    # Assert exactly one POST
    assert request_count == 1
    req = request_captured[0]
    assert req.method == "POST"
    assert str(req.url) == "https://api.parallel.ai/v1/search"
    assert req.headers.get("x-api-key") == "test-api-key"

    # Exact payload
    payload = json.loads(req.content)
    assert payload == {
        "search_queries": ["query1", "query2"],
        "objective": "test objective",
        "session_id": "session-123",
        "mode": "basic",
        "max_chars_total": 3600,
        "client_model": "gemini-test-model",
        "advanced_settings": {
            "max_results": 3,  # min(max_results, 3)
            "excerpt_settings": {
                "max_chars_per_result": 1200
            }
        }
    }

    # Frugal caps (max 3 sources returned)
    evidence = result["evidence"]
    assert len(evidence) == 3

    # Actual metadata
    assert result["search_id"] == "srch_123"
    assert result["session_id"] == "session-123"
    assert "retrieval_time" in result
    assert result["objective"] == "test objective"
    assert result["queries"] == ["query1", "query2"]

    # Excerpt join
    assert evidence[0].title == "Valid 1"
    assert evidence[0].relevance_summary == "Excerpt 1a Excerpt 1b"
    assert evidence[0].url == "https://valid1.com/path/"

    # Invalid URL filtering and trailing-slash dedup
    urls = [ev.url for ev in evidence]
    assert "ftp://invalid-scheme.com" not in urls
    assert "https://valid1.com/path" not in urls
    assert "https://valid2.com" in urls
    assert "https://valid3.com" in urls
    assert "https://valid4.com" not in urls

@pytest.mark.asyncio
async def test_missing_id_failure(monkeypatch):
    monkeypatch.setattr(settings, "USE_FIXTURES", False)

    original_async_client = httpx.AsyncClient
    request_captured = []
    request_count = 0

    async def mock_handler(request: httpx.Request) -> httpx.Response:
        nonlocal request_count
        request_count += 1
        request_captured.append(request)
        response_data = {
            "session_id": "session-123",
            "results": []
        }
        return httpx.Response(200, json=response_data, request=request)

    def async_client_factory(*args, **kwargs):
        timeout = kwargs.get("timeout", 10.0)
        transport = httpx.MockTransport(mock_handler)
        return original_async_client(transport=transport, timeout=timeout)

    monkeypatch.setattr(httpx, "AsyncClient", async_client_factory)

    with pytest.raises(RuntimeError) as exc_info:
        await execute_parallel_search(
            queries=["query1"],
            objective="test objective",
            session_id="session-123"
        )
    assert str(exc_info.value) == "Evidence search failed. Service unavailable."
    assert request_count == 1

@pytest.mark.asyncio
async def test_conflicting_session_failure(monkeypatch):
    monkeypatch.setattr(settings, "USE_FIXTURES", False)

    original_async_client = httpx.AsyncClient
    request_captured = []
    request_count = 0

    async def mock_handler(request: httpx.Request) -> httpx.Response:
        nonlocal request_count
        request_count += 1
        request_captured.append(request)
        response_data = {
            "search_id": "srch_123",
            "session_id": "different-session",
            "results": []
        }
        return httpx.Response(200, json=response_data, request=request)

    def async_client_factory(*args, **kwargs):
        timeout = kwargs.get("timeout", 10.0)
        transport = httpx.MockTransport(mock_handler)
        return original_async_client(transport=transport, timeout=timeout)

    monkeypatch.setattr(httpx, "AsyncClient", async_client_factory)

    with pytest.raises(RuntimeError) as exc_info:
        await execute_parallel_search(
            queries=["query1"],
            objective="test objective",
            session_id="session-123"
        )
    assert str(exc_info.value) == "Evidence search failed. Service unavailable."
    assert request_count == 1

@pytest.mark.asyncio
async def test_invalid_arguments_fail_before_http(monkeypatch):
    monkeypatch.setattr(settings, "USE_FIXTURES", False)

    original_async_client = httpx.AsyncClient
    request_count = 0

    async def mock_handler(request: httpx.Request) -> httpx.Response:
        nonlocal request_count
        request_count += 1
        return httpx.Response(200, json={}, request=request)

    def async_client_factory(*args, **kwargs):
        timeout = kwargs.get("timeout", 10.0)
        transport = httpx.MockTransport(mock_handler)
        return original_async_client(transport=transport, timeout=timeout)

    monkeypatch.setattr(httpx, "AsyncClient", async_client_factory)

    # Invalid queries (empty)
    with pytest.raises(ValueError):
        await execute_parallel_search(queries=[], objective="obj", session_id="sess")

    # Invalid queries (>3)
    with pytest.raises(ValueError):
        await execute_parallel_search(queries=["q1", "q2", "q3", "q4"], objective="obj", session_id="sess")

    # Blank objective
    with pytest.raises(ValueError):
        await execute_parallel_search(queries=["q1"], objective=" ", session_id="sess")

    # Blank session
    with pytest.raises(ValueError):
        await execute_parallel_search(queries=["q1"], objective="obj", session_id=" ")

    assert request_count == 0

@pytest.mark.asyncio
async def test_zero_usable_sources_fails_closed(monkeypatch):
    monkeypatch.setattr(settings, "USE_FIXTURES", False)

    original_async_client = httpx.AsyncClient
    request_captured = []
    request_count = 0

    async def mock_handler(request: httpx.Request) -> httpx.Response:
        nonlocal request_count
        request_count += 1
        request_captured.append(request)
        response_data = {
            "search_id": "srch_123",
            "session_id": "session-123",
            "results": [
                {
                    "url": "ftp://invalid-scheme.com",
                    "title": "Invalid Scheme"
                },
                {
                    "url": "   ",
                    "title": "Blank URL"
                },
                {
                    "url": None,
                    "title": "None URL"
                }
            ]
        }
        return httpx.Response(200, json=response_data, request=request)

    def async_client_factory(*args, **kwargs):
        return original_async_client(transport=httpx.MockTransport(mock_handler), timeout=kwargs.get("timeout", 10.0))

    monkeypatch.setattr(httpx, "AsyncClient", async_client_factory)

    with pytest.raises(RuntimeError) as exc_info:
        await execute_parallel_search(queries=["query1"], objective="test objective", session_id="session-123")

    assert str(exc_info.value) == "Evidence search failed. Service unavailable."
    assert request_count == 1
