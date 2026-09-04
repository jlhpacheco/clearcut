# ClearCut Provenance Register

## Submission gate

ClearCut's submitted implementation and creative assets must be newly created during the Agentic Cinema contest period using only competition-permitted Google tooling. Parallel Search may supply runtime retrieval results; it is not used to author implementation or creative assets.

Codex and Codex subagents may plan, coordinate, audit, inspect, run tests, and report defects. They must not create, modify, patch, or regenerate any implementation code, runtime configuration, test fixture, logo, image, video, voice, music, or other creative asset included in the submission. Any required implementation change must be made manually by the entrant or with a permitted Google coding tool and recorded below. Codex-assisted planning documents may remain only when clearly labeled as non-runtime planning material.

A file cannot enter the final repository, hosted build, demo video, or Devpost materials until its provenance entry is complete and approved. Any file with unknown origin, prohibited tooling, unresolved third-party rights, or an incomplete evidence field must be excluded or replaced. No `TODO` may remain for a submitted file at the final gate.

## Approved assets

| Field | Evidence |
|---|---|
| File | `assets/brand/clearcut-logo.png` |
| Category | Original brand image; submitted asset |
| Creator and owner | Jose Luis Pacheco; entrant-owned |
| Created | 2026-09-01, America/Los_Angeles |
| Provider/tool | Google Vertex AI image generation |
| Model | `gemini-2.5-flash-image` |
| Google Cloud project | `clearcut-agentic-20260901` |
| Inputs | Original text-only prompt; no reference image or third-party asset |
| Third-party content | None declared |
| Rights/license | Entrant-owned; distributed with the repository under Apache-2.0 |
| Generation evidence | Authenticated Vertex AI REST generation in the ClearCut project; generation completed 2026-09-01 PT. No raw request token or credential retained. |
| SHA-256 | `DDC3A17AC0E4AB95A9D096D661BD419D8328C99DB70AA518EDAF9EB922999793` |
| Review | Jose Luis Pacheco, 2026-09-01, approved in conversation ("awesome") |

## Entry template

| Field | Evidence |
|---|---|
| File | Path in repository or submission |
| Category | Implementation, runtime configuration, test fixture, image, video, audio, or document |
| Submission use | Repository, hosted build, demo video, or Devpost |
| Creator and owner | Entrant and ownership status |
| Created | Date and timezone |
| Provider/tool | Exact permitted Google tool or manual entrant work |
| Model/version | Exact model or version, when applicable |
| Google Cloud project | Project identifier, when applicable |
| Inputs and sources | Prompts, entrant-owned inputs, and authorized sources |
| Third-party content | Names, licenses, permission, and usage, or `None` |
| Generation evidence | Sanitized request/run/build evidence; never include credentials |
| SHA-256 | Final file hash |
| Review | Reviewer, date, and approval status |

## Repair run record

### Repair Pass 1 (Foundation Repair Pass)
| Field | Evidence |
|---|---|
| Tool | Gemini CLI 0.58.0 |
| Model | `gemini-3.5-flash` |
| Date | 2026-09-02 America/Los_Angeles |
| Prompt file | `docs/GEMINI_FOUNDATION_REPAIR_PROMPT.md` |
| Input | Repository input only |
| Files created/changed | `.gitignore`, `.dockerignore`, `agent/.env.example`, `agent/app/__init__.py`, `agent/app/settings.py`, `agent/app/contracts.py`, `agent/app/video_analysis.py`, `agent/app/parallel_search.py`, `agent/app/agent.py`, `agent/app/api.py`, `agent/tests/test_parallel_normalization.py`, `agent/tests/test_research_events.py` |
| Verification status | Backend unit and contract tests passed. Hashes pending verification. |

### Repair Pass 2 (Visual Corrective Pass)
| Field | Evidence |
|---|---|
| Tool | Gemini CLI 0.58.0 |
| Model | `gemini-3.5-flash` |
| Date | 2026-09-02 America/Los_Angeles |
| Prompt file | `docs/GEMINI_FOUNDATION_REPAIR_2_PROMPT.md` |
| Input | Repository input only |
| Files created/changed | `src/ClearCut.Web/Components/Pages/Review.razor`, `src/ClearCut.Web/Components/Review/FindingCard.razor`, `src/ClearCut.Web/Components/Review/ResearchTimeline.razor`, `src/ClearCut.Web/wwwroot/app.css`, `THIRD_PARTY_NOTICES.md`, `docs/PROVENANCE.md` |
| Verification status | All Blazor / C# and Python tests passed. Hashes pending verification. |
| Logo Provenance | Preserved logo provenance (`assets/brand/clearcut-logo.png`). Identical copy deployed at `src/ClearCut.Web/wwwroot/assets/brand/clearcut-logo.png`. |

### Repair Pass 3 (Live ADK and Parallel Integration)
| Field | Evidence |
|---|---|
| Tool | Authenticated Gemini Generative Language API patch workflow in Google Cloud project `clearcut-agentic-20260901` |
| Model | `gemini-3.5-flash` (Codex only inspected, tested, reviewed drafts, and mechanically applied accepted Gemini-produced diffs) |
| Date | 2026-09-04 America/Los_Angeles |
| Scope | Python FastAPI agent, Google ADK orchestration, Gemini video-analysis path, Parallel Search adapter, C# AgentClient/session/UI trace proof, configs/manifests, dependency lock files, Docker agent build, and offline unit/contract tests. |
| Live sanitized proof | Objective compared official DOE information on LED vs incandescent energy use for the fictional `LumaLeaf 76 percent efficiency` statement; 3 queries; exactly one Parallel call; search_id `search_125e49bc4c3b6f3ea020c62e2cad7ade`; session_id `cc-live-smoke-8d7bed48b523`; retrieval time `2026-09-04T20:02:50.636162+00:00`; 3 cited sources from energy.gov and energystar.gov. No credentials or raw provider bodies retained. |
| Verification status | Locked Python 3.12 Docker tester 19 passed with one upstream ADK deprecation warning; .NET 10 suite 24 passed; clean agent runtime Docker build; clean web Docker build; dependency base digest recorded in deploy/agent.Dockerfile. |
| Security/correctness gates | Exactly one call/no retry, 1-3 queries, max 3 evidence items and 3600 chars, request/result session binding, untrusted-data delimiters, zero evidence fails closed, malformed/null/unknown/premature streams fail closed, fixture wording states no search executed, live trace metadata shown in UI. |
| Files created/changed | Key directories/files: `agent/app/` (agent, api, parallel_search, video_analysis, settings, contracts), `agent/tests/` (unit and contract tests), `src/ClearCut.Web/` (AgentClient, session state, UI trace views), `deploy/` (agent.Dockerfile, web.Dockerfile, manifests), `agent/requirements.lock`, and `agent/requirements-test.lock`. |
| Hashes | Pending final release artifact freeze. |

### Repair Pass 4 (Playwright and UI Checkpoint)
| Field | Evidence |
|---|---|
| Tool | Authenticated Gemini Generative Language API patch workflow in Google Cloud project `clearcut-agentic-20260901` |
| Model | `gemini-3.5-flash` (Codex only inspected, tested, reviewed drafts, and mechanically applied accepted Gemini-produced diffs) |
| Date | 2026-09-04 America/Los_Angeles |
| Scope | Playwright/TypeScript setup, stable data-testid hooks, responsive CSS, relative fixture evidence URL, Dismiss confirmation behavior, accessible checklist table region, and the retrying assertion that removed an asynchronous print test race. |
| Verification status | Verification results on 2026-09-04: npm dependency audit: 0 vulnerabilities; Playwright Chromium: 10/10 passed, one worker, fullyParallel false, 38.1 seconds on final full rerun; Targeted repaired export/print test: 1/1 passed; .NET Release suite: 24/24 passed; Locked Python agent Docker suite: 19/19 passed (one upstream ADK deprecation warning); Web Docker image clearcut-web-playwright-check built successfully. Generated test artifacts and node_modules are excluded by .gitignore. |
| Files created/changed | Playwright/TypeScript configuration, UI components, CSS files, and test suites. |
| Hashes | Pending final release artifact freeze. |

## Final review

- [ ] Every submitted implementation and creative asset has a complete provenance entry.
- [ ] Submitted code, runtime configuration, and fixtures were authored manually by the entrant or with competition-permitted Google tooling.
- [ ] Submitted creative assets were produced with permitted Google tooling and contain no unauthorized third-party content.
- [ ] Codex and subagents were limited to planning, coordination, inspection, testing, audit, and defect reporting.
- [ ] Third-party packages and templates are authorized and their licenses are documented.
- [ ] Final hashes match the files in the repository, hosted build, demo video, and Devpost submission.
- [ ] No credentials, private identifiers, secrets, or raw authentication artifacts are present.
