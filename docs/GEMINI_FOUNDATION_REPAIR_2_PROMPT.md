# Gemini Foundation Repair Pass 2

Continue the permitted Google-authored ClearCut implementation repair. Read the repository and the prior brief `docs/GEMINI_FOUNDATION_REPAIR_PROMPT.md`. Do not commit, push, deploy, access secrets, call external paid APIs, or create final creative media.

The prior pass timed out. Independent verification found:

- `dotnet test ClearCut.sln --configuration Release` passes 13/13.
- `docker build -f deploy/agent.Dockerfile -t clearcut-agent:foundation .` succeeds.
- Runtime import fails exactly: `ModuleNotFoundError: No module named 'google'` at `from google.adk.agents import Agent`.
- `agent/pyproject.toml` still omits Google ADK/Gen AI and points at missing `README.md`.
- `deploy/agent.Dockerfile` still installs floating packages directly, omits tests/contracts, and has no test target.
- `parallel_search.py` incorrectly places `source_policy` at the payload root and types it as a string; it must be under `advanced_settings.source_policy` as an object. It still prints raw exception text and uses a hard-coded retrieval date.
- `ResearchAgent.run_research` still bypasses ADK execution and uses fixed queries, so the runtime cannot prove Gemini/ADK generated the objective/tool arguments.
- The Docker Compose, explicit deployment project guard, robust cross-language contracts/tests, provenance update, template cleanup, invalid MP4 cleanup, and judge-facing fixture UI fixes may be incomplete.

## Required repairs

1. Make Python dependencies reproducible and runtime-valid on Python 3.12. Use compatible pinned versions or defensible bounded versions for actual `google-adk`, `google-genai`/Vertex support, FastAPI, Uvicorn, Pydantic, HTTPX, dotenv, pytest, and pytest-asyncio. Add the missing agent README or remove the broken readme declaration.
2. Rewrite `deploy/agent.Dockerfile` as a reproducible multi-stage Dockerfile with a `test` target that installs the declared project dependencies, copies `agent/tests` and `contracts`, and can run all Python tests. The runtime target must import and start `app.api` successfully without secrets in Development fixture mode.
3. Correct the Parallel v1 payload/response contract exactly: `search_queries`, `objective`, `mode`, `max_chars_total`, `client_model`, unique `session_id`, and `advanced_settings` containing `max_results`, `excerpt_settings`, and optional object `source_policy`. Parse `excerpts` arrays; validate/deduplicate only returned HTTP(S) URLs; exactly one POST and no automatic retry. Return search/session IDs and retrieval time in the typed result/stream so the UI can prove the call. Never print raw exceptions; use sanitized structured logging and stable client-safe errors.
4. Use actual Google ADK execution for non-fixture research. Gemini/ADK must derive the objective and 1–3 queries from the selected finding and invoke the one registered Parallel tool exactly once. Do not use fixed finding-ID queries in runtime mode. Fixture mode may be deterministic but must be visibly and structurally separate. Add tests that prove exactly one tool registration and enforce single-call/no-retry behavior without network.
5. Complete strict Pydantic/C# contracts and tests from the first brief. Preserve the currently passing C# behavior while adding negative cases and shared-golden-contract parity.
6. Complete root `docker-compose.yml`, `.dockerignore`, `.gitignore`, non-secret configuration example, and `deploy/verify-project.ps1` or equivalent fail-closed guard. It must reject any ID except literal `clearcut-agentic-20260901` and any number except `328400425249` before a gcloud command. Do not implement deployment yet.
7. Complete the web fixture-safety/UI foundation: default fixtures off, production startup lockout, exact `FIXTURE MODE — NO LIVE SERVICES` development banner, separate `FICTIONAL DEMO — NOT LEGAL ADVICE` label, calm empty findings state, compact five-step workflow, semantic finding buttons/selected state, honest error/retry/start-over behavior, responsive 390px layout, focus/live-region/reduced-motion support, and no invalid MP4 network request. Remove default Blazor sample pages/nav/branding and unused Bootstrap vendor files.
8. Update `docs/PROVENANCE.md` truthfully for both repair passes. Identify Gemini CLI 0.58.0, model `gemini-3.5-flash`, date 2026-09-02 America/Los_Angeles, prompt paths, repository-only input, and files changed. Keep hashes pending for independent verification. Preserve logo provenance and identify its deployed identical copy. Add `THIRD_PARTY_NOTICES.md` for retained packages/assets.
9. Delete only generated build/cache outputs (`bin`, `obj`, `__pycache__`, `.pytest_cache`, coverage/test-result output) after verification. Do not delete source or user-authored documents.

## Required checks

Run `dotnet test ClearCut.sln --configuration Release`. Build the agent Docker `test` target and run its pytest suite. Build the agent runtime image and prove `import app.api`. Build the web image. Run the wrong-project guard tests. Do not weaken tests.

Finish with exact pass counts and unresolved blockers. Do not commit or push.
