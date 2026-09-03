# Google Build Prompt — Phase 1

This is a non-runtime planning document authored with Codex coordination. All implementation files produced from it must be generated or repaired by competition-permitted Google tooling and recorded in `docs/PROVENANCE.md`.

Build an exportable, production-shaped repository named **ClearCut** for the Agentic Cinema hackathon. The target user is an independent filmmaker preparing a rough cut for attorney, distributor, or E&O review.

## Non-negotiable technology

The submitted application must use C# .NET 10 ASP.NET Core Blazor Web App with Interactive Server rendering for the public web experience, plus a separate Python 3.12 FastAPI-compatible Google ADK agent service. Do not substitute React, JavaScript, TypeScript, Node, Firebase, or another frontend stack.

Create this structure: `ClearCut.sln`; `src/ClearCut.Web`; `tests/ClearCut.Web.Tests`; `agent/clearcut_agent`; `agent/tests`; `contracts/golden-review.json`; Dockerfiles for both services; `docker-compose.yml`; `.env.example` with names only; and verified README run instructions. Never embed credentials. Runtime AI is Gemini on Vertex AI and the only retrieval partner is Parallel Search. Cloud project is exactly `clearcut-agentic-20260901`, project number `328400425249`; do not reference or inspect any other project.

## Contracts and rules

Implement typed contracts for ReviewFinding, EvidenceSource, ResearchEvent, Disposition, and ReviewSession. Enforce:

- one operation at a time;
- chronological findings with stable IDs;
- exactly four dispositions: Dismiss, Investigate, Replace, License;
- Dismiss only after successful evidence plus explicit human confirmation;
- printable export only when all three findings have a human decision;
- cross-language enums and JSON fields verified by C# and Pydantic tests consuming `contracts/golden-review.json`;
- honest rejection of malformed or incomplete sets.

## Experience

Create a cool, calm Hollywood screening room—not a generic dashboard. Use midnight navy, warm ivory, restrained teal, crisp typography, subtle cinematic light, generous whitespace, excellent contrast, keyboard accessibility, and responsive layouts at 1280×720 and about 390px.

Above the fold show the ClearCut brand area, a fictional-demo label, short workflow explanation, disclaimer that this is research assistance and not legal advice, included-film region, and primary button **Analyze with Gemini**. The approved logo is provided at `assets/brand/clearcut-logo.png`; copy it into the web app and use it without altering or replacing it.

After analysis, show three chronological finding cards for a fictional brand mark, factual claim, and original music cue. Each card has timestamp, neutral observation, review priority, research status, and **Research with Parallel**. Selecting a card seeks the video. Show the visible four-stage trace: **Preparing research task**, **Searching with Parallel**, **Reviewing sources**, **Evidence ready**. Show cited source cards, uncertainty, human disposition controls, live checklist, and print readiness.

Never claim cleared, approved, infringement probability, or legal safety. Failed analysis says **Analysis unavailable** or **Analysis incomplete**. Weak search says **Evidence incomplete**.

Fixtures are allowed only for local UI development and must be visibly labeled **FIXTURE DEMO**. Production configuration must fail closed if fixture mode is enabled.

Add an entrant-owned plain-text fictional evidence page at `/evidence/lumaleaf-energy-study` containing unique token `CC-EVID-9F4D` and a clear statement that LumaLeaf and its 76% energy comparison are fictional demonstration data. Use no external images, fonts, logos, embeds, analytics, or quotations.

## Verification

Add meaningful unit tests, contract tests, accessible markup, health endpoints, strict settings validation, structured correlation IDs, timeout/cancellation paths, and container health checks. Pin sensible stable dependency versions. Make `dotnet build`, `dotnet test`, Python 3.12 `pytest`, and both container builds pass without real cloud credentials in fixture mode.

This is build phase 1: prioritize a clean compilable skeleton, shared contracts, tests, and the full cinematic fixture UI. Do not make live Gemini or Parallel calls yet. Read `docs/PRD.md`, `docs/SPEC.md`, `docs/ARCHITECTURE.md`, `docs/JUDGING.md`, `docs/PROVENANCE.md`, and `docs/BUILD_CHECKLIST.md` before editing. Work autonomously until the phase-one tests pass. At the end, update the provenance register with exact generated implementation paths, tool name/version, model, date, and project.
