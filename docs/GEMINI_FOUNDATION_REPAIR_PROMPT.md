# Gemini Foundation Repair Brief

You are the competition-permitted Google coding tool responsible for authoring and repairing ClearCut's submitted implementation. Work only inside this repository. Read `docs/SCOPE.md`, `docs/PRD.md`, `docs/SPEC.md`, `docs/BUILD_CHECKLIST.md`, `docs/PROVENANCE.md`, `docs/GOOGLE_BUILD_PROMPT.md`, and the existing source/tests before editing.

## Non-negotiable boundaries

- Project: `clearcut-agentic-20260901`; project number: `328400425249`.
- Never list, read, reference, or mutate any other Google Cloud project.
- Do not use or mention OpenAI, Anthropic, HeyGen, or any non-Google AI in submitted implementation or assets. Parallel is the selected partner retrieval service.
- Do not create the final creative video/audio in this pass. Use an honest accessible placeholder state until the permitted Google media-generation pass.
- Do not commit, push, deploy, or make billable external API calls in this pass.
- Never print, read, write, or invent API keys. Use environment-variable/Secret Manager names only.
- Fixture mode must be conspicuous in development and impossible in production.
- Do not claim legal clearance, trademark availability, music ownership, or safety.

## Repair checklist

1. Repository hygiene and provenance
   - Add a robust root `.gitignore`, root `.dockerignore`, and non-secret `agent/.env.example`.
   - Ignore `.env`/local secrets while allowing `.env.example`; ignore `bin`, `obj`, Python caches, pytest/coverage output, Playwright output, logs, and editor/build artifacts.
   - Delete only generated build/cache artifacts already present under `bin`, `obj`, `__pycache__`, `.pytest_cache`, and equivalent test-result folders. Do not delete authored source or user documents.
   - Record this repair run truthfully in `docs/PROVENANCE.md`: Gemini CLI 0.58.0, model `gemini-3.5-flash`, date 2026-09-02 America/Los_Angeles, prompt file `docs/GEMINI_FOUNDATION_REPAIR_PROMPT.md`, repository input only, and the files created/changed. Mark final hashes as pending until verification rather than inventing them.
   - Preserve the approved logo provenance and describe the deployed logo copy as an identical derived copy.
   - Remove unused template/vendor assets where practical. If any third-party code/assets remain, add complete license/notice attribution in `THIRD_PARTY_NOTICES.md`.

2. Guarded two-service foundation
   - Keep .NET 10 ASP.NET Core Blazor Interactive Server for the public web app and Python 3.12 FastAPI for the private agent service.
   - Add missing package/module files and deterministic dependency declarations/locks suitable for repeatable Docker builds. Use actual Google packages, including native Google ADK and Google Gen AI/Vertex libraries; do not describe a custom class as ADK-compatible.
   - Implement an actual native Google ADK root Agent with exactly one typed Parallel search function tool. Ensure tests can inspect the registered tool list without a live external call.
   - Force Vertex mode to the literal ClearCut project and configured `us-central1` location; validate settings and fail closed on project-number/project-ID mismatch.
   - Correct Parallel Search API v1 request/response handling: required `search_queries`, visible `objective`, `mode`, `max_chars_total`, `client_model`, unique `session_id`, `advanced_settings.max_results`, optional allowlisted `source_policy`, and `excerpts` array parsing. One user action means exactly one POST and zero automatic retries. Normalize and deduplicate only returned HTTP(S) URLs. Use a stable public error code/message; log sanitized internal details server-side.
   - Drive NDJSON research stages from real backend operations, never fixed sleeps or fabricated progress.
   - Add `agent/app/__init__.py`, health/analyze/research-stream validation, and production fixture lockout.

3. Cross-language contracts and session invariants
   - Strengthen C# and Pydantic contracts so enums cannot drift and extra/empty/invalid fields are rejected.
   - Enforce exactly three findings for the golden review: one fictional brand-artwork question, one precise factual product-claim question, and one original-music question; unique nonempty IDs; chronological valid timestamps; valid categories/priorities/statuses; and valid HTTP(S) evidence URLs.
   - Enforce one operation at a time, explicit reset confirmation, unknown-finding rejection, and exactly three human dispositions before export.
   - `Dismiss` requires successful evidence plus explicit human confirmation. Never infer originality or availability from a missing search match.
   - Make `contracts/golden-review.json` a real shared fixture consumed by both test suites, including dispositions/events where needed to prove parity.

4. Web production safety and judge-facing fixture UI
   - Default fixtures off. Permit fixtures only in Development and make the app fail at startup if fixtures are enabled outside Development.
   - In Development fixture mode show the exact prominent banner: `FIXTURE MODE — NO LIVE SERVICES`.
   - Keep `FICTIONAL DEMO — NOT LEGAL ADVICE` as a separate media/legal label.
   - Remove all default Blazor template pages, nav items, sample branding, and unused Bootstrap assets.
   - Repair the unhandled UI error and invalid placeholder-video/range behavior. Until real media exists, show a truthful cinematic placeholder panel that does not request an invalid MP4.
   - At 1280x720 without scrolling show ClearCut/logo, fictional label, one-sentence outcome, compact five-step workflow, legal boundary, scene placeholder/video region, and one unique `Analyze with Gemini` primary action.
   - Add a calm pre-analysis findings empty state.
   - After fixture analysis show exactly three chronological cards. Use semantic buttons/selected state, keyboard activation, visible focus, status not conveyed by color alone, live-region announcements, reduced-motion support, 44px controls, wrapping long text, and no horizontal scrolling around 390px.
   - Error mode must not show competing Analyze/Retry primary actions; Start Over remains available after failure with confirmation.
   - Use neutral text such as `research evidence for professional review`, not `defensible`, `verified`, `cleared`, `approved`, `safe`, or a legal-risk percentage.

5. Docker and tests
   - Provide root `docker-compose.yml` for local web+agent development without secrets.
   - Make both Dockerfiles reproducible and add a Python test stage that includes tests/contracts and runs under Python 3.12.
   - Add comprehensive C# and Python unit/contract tests for all invariants above: malformed/incomplete/extra findings, category completeness, duplicate IDs, timestamp validity, enum drift, URL validation, operation locking, cancellation, reset confirmation, unknown finding, Dismiss confirmation, changed disposition, print readiness, fixture production guard, Parallel single-call/no-retry behavior, response parsing/deduplication, sanitized errors, ADK tool registration, and shared golden contract parity.
   - Add explicit project-guard scripts/tests that reject a wrong project ID or number before any gcloud call. Every real cloud command must include literal `--project clearcut-agentic-20260901`.
   - Do not create the Playwright suite in this pass; it will be generated after the UI stabilizes.

## Required self-verification

Run formatting where available, `dotnet build`, `dotnet test`, Python tests in a Python 3.12 container/test stage, and both Docker builds. Do not weaken tests to make them pass. If a dependency or environment prevents a check, report the exact blocker and leave the implementation in the safest fail-closed state.

Finish with a concise list of changed files, test results, and unresolved blockers. Do not commit or push.
