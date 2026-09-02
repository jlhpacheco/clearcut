# ClearCut Technical Specification

## Overview

ClearCut is a two-service, anonymous demonstration that turns one original short film clip into a timestamped, cited clearance-preparation checklist. The public C# application owns the complete user experience. A private Python service owns the Google ADK workflow, Gemini video understanding, and the live Parallel Search tool call.

This design is intentionally narrow enough for a 20-hour build. It has no accounts, uploads, database, background queue, or permanent workspace. It must prove one honest end-to-end path twice from a clean browser session.

Product behavior is defined in `docs/PRD.md`. This specification implements PRD Epics 1–6 and does not expand the MVP.

## Stack

| Layer | Choice | Reason |
| --- | --- | --- |
| Web experience | .NET 10, ASP.NET Core Blazor Web App with Interactive Server rendering | Matches the participant's C# experience and keeps UI and server coordination in one deployable service. |
| Agent service | Python 3.12, Google Agent Development Kit, FastAPI-compatible HTTP entry point | ADK has first-class Python support; Python 3.12 avoids relying on the workstation's newer Python 3.14 runtime. |
| AI model | Gemini on Vertex AI, configured through `GEMINI_MODEL` | Provides real video-and-audio analysis and structured findings without embedding a model ID throughout the code. |
| Partner retrieval | Parallel Search API `POST /v1/search` | Produces live ranked URLs and excerpts for the evidence chain. |
| Media | One original MP4 in Cloud Storage, with a matching web-playable copy or app-served route | Gemini can analyze a `gs://` URI while the judge can play the same controlled asset. |
| Hosting | Two Cloud Run services in one region | Supports .NET and Python containers, private service-to-service access, and scale-to-zero cost control. |
| Secrets | Google Secret Manager | Keeps the Parallel key out of source, images, logs, and browser traffic. |
| Session state | Scoped in-memory Blazor state | Satisfies the temporary anonymous-demo requirement without a database. |
| Report | Print-specific HTML/CSS and browser print dialog | Produces a useful export without a PDF dependency. |
| Tests | xUnit for C#; pytest for Python | Covers contracts, state rules, tool normalization, and golden-path fixtures. |

Current reference documentation:

- Google ADK deployment targets: https://google.github.io/agents-cli/guide/deployment/
- Google ADK documentation: https://google.github.io/adk-docs/
- Gemini video understanding sample: https://docs.cloud.google.com/vertex-ai/generative-ai/docs/samples/googlegenaisdk-textgen-with-video
- Cloud Run .NET deployment: https://docs.cloud.google.com/run/docs/quickstarts/build-and-deploy/deploy-dotnet-service
- Parallel Search quickstart: https://docs.parallel.ai/search/search-quickstart
- Parallel Search best practices: https://docs.parallel.ai/search/best-practices

Package versions must be pinned in project lock/configuration files after the first successful local integration test. Secrets and model names remain environment configuration.

## Architecture

### Public Web Service — `ClearCut.Web`

The only public service. It renders the Blazor interface, serves the original demo clip, maintains one temporary review session, consumes the private agent API, and renders the print report.

Responsibilities:

- Implements PRD Epics 1, 3, 5, and 6.
- Shows the opening explanation and disclaimer.
- Prevents duplicate analysis and simultaneous research runs.
- Seeks the HTML video player to selected timestamps.
- Maps streamed research events to the visible four-stage timeline.
- Keeps dispositions and optional reviewer notes in memory.
- Enforces the Dismiss and report-unlock rules in both UI state and domain methods.

### Private Agent Service — `ClearCut.Agent`

A Python ADK application deployed as an authenticated Cloud Run service. It is not directly reachable by an anonymous browser. The web service calls it using its Cloud Run service identity.

Responsibilities:

- Implements PRD Epics 2 and 4.
- Sends the configured `gs://` MP4 to Gemini for video-and-audio analysis.
- Requests a strict JSON result and validates every field before returning it.
- Uses an ADK agent with a narrowly defined Parallel Search tool.
- Emits honest newline-delimited status events during research.
- Deduplicates sources by normalized URL and returns at most five useful sources.
- Never returns a legal conclusion or a synthetic source URL.

### Gemini Video Analyzer

`video_analysis.py` calls Gemini through Vertex AI using Application Default Credentials. The prompt identifies the clip as fictional demonstration media and requests candidate observations—not legal judgments.

The response schema contains:

- `finding_id`
- `category`: `brand_mark`, `factual_claim`, or `music_cue`
- `start_seconds` and optional `end_seconds`
- `label`
- `observation`
- `review_priority`: `routine`, `attention`, or `priority`
- `research_objective`

The validator rejects malformed timestamps, unsupported categories, missing observations, duplicate IDs, and findings outside the clip duration. It sorts valid findings chronologically. The golden path requires one valid finding in each supported category. It never fabricates a missing item.

### ADK Research Agent

The ADK root agent receives only the selected validated finding and safe clip context. Its instruction is to form a focused research objective, choose 1–3 concise search queries, call the Parallel tool once, and summarize relevance without deciding clearance.

The tool result, not model memory, is the source of displayed URLs. The agent may label uncertainty or disagreement, but it may not select a disposition.

### Parallel Search Tool

`parallel_search.py` performs one server-side request to `https://api.parallel.ai/v1/search` using the `x-api-key` secret. It sends:

- A human-readable objective derived from the active finding
- One to three concise `search_queries`
- A unique per-run `session_id`
- A five-result ceiling where the current API supports it
- A bounded character budget

The tool validates HTTP status, timeout, response shape, URLs, and excerpts. It removes duplicate URLs and maps the response to the internal evidence contract. No API key or raw authorization header may be logged.

### Temporary Review Session

`ReviewSessionStore` is scoped to the interactive Blazor session. It contains the analysis state, ordered findings, evidence, notes, dispositions, and the active operation flag. Refreshing or closing the session may clear it, matching PRD Story 6.3.

No production database, browser local storage, cookie identity, analytics profile, or cross-session history is part of the MVP.

### Google Cloud Boundary

All resources must live only in project `clearcut-agentic-20260901`. The COR project must not be read or modified.

Recommended resources:

- Public Cloud Run service: `clearcut-web`
- Private Cloud Run service: `clearcut-agent`
- Dedicated web service account with only Cloud Run Invoker permission on `clearcut-agent`
- Dedicated agent service account with minimum Vertex AI and demo-object read permissions
- Secret Manager secret: `parallel-api-key`
- Regional Cloud Storage bucket containing only the original demo MP4
- Artifact Registry images for the two services

Both Cloud Run services use minimum instances `0` and maximum instances `1` during the hackathon. Set request timeouts deliberately and keep the services in the same region as the storage and Vertex AI calls where supported.

## File Structure

```text
ClearCut.sln
README.md
LICENSE
.env.example                         # Names only; never real credentials
.gitignore

src/
  ClearCut.Web/
    ClearCut.Web.csproj
    Program.cs                       # DI, auth client, endpoints, Cloud Run port
    appsettings.json                 # Non-secret defaults
    Components/
      App.razor
      Layout/MainLayout.razor
      Pages/Review.razor             # Complete PRD journey composition
      Review/VideoPlayer.razor       # Playback and timestamp seeking
      Review/FindingCard.razor       # Finding summary and selection
      Review/ResearchTimeline.razor  # Four truthful runtime stages
      Review/EvidenceCard.razor      # URL, publisher, date, relevance
      Review/DispositionPanel.razor  # Four human decisions and guardrails
      Review/ClearanceChecklist.razor# Completion state and print action
    Models/
      AnalysisContracts.cs
      ResearchContracts.cs
      ReviewSession.cs
      Disposition.cs
    Services/
      AgentClient.cs                 # Authenticated private HTTP client
      ReviewSessionStore.cs          # Scoped temporary state and invariants
      ReportService.cs               # Print-ready view model
    wwwroot/
      css/app.css                    # Slate/teal cinematic design
      js/videoInterop.js             # Precise seek and print helpers
      media/clearcut-demo.mp4        # Original judge-playback copy

agent/
  pyproject.toml                     # Pinned Python 3.12 dependencies
  agents-cli-manifest.yaml           # ADK Cloud Run target metadata
  app/
    __init__.py
    api.py                            # Health, analyze, research-stream routes
    agent.py                          # ADK root agent and instruction
    video_analysis.py                 # Gemini multimodal request
    parallel_search.py                # Partner tool and normalization
    contracts.py                      # Pydantic schemas
    settings.py                       # Validated environment configuration
  tests/
    test_video_contract.py
    test_parallel_normalization.py
    test_research_events.py
    fixtures/
      gemini_three_findings.json
      parallel_results.json

tests/
  ClearCut.Web.Tests/
    ClearCut.Web.Tests.csproj
    ReviewSessionStoreTests.cs
    AgentClientContractTests.cs
    ReportServiceTests.cs

deploy/
  web.Dockerfile
  agent.Dockerfile
  deploy.ps1                         # Explicit project/region/resource guards
  smoke-test.ps1                     # Public health and golden-path checks

docs/
  SCOPE.md
  PRD.md
  SPEC.md
  JUDGING.md
  SUBMISSION_CHECKLIST.md
```

## Data Flow

### Analysis Flow — PRD Epic 2

1. `Review.razor` calls `ReviewSessionStore.BeginAnalysis()`. The store refuses if an operation is already active.
2. `AgentClient` sends `POST /v1/analyze` with the configured demo asset ID; it never accepts an arbitrary user URI.
3. The private service maps that ID to `DEMO_VIDEO_GCS_URI` and calls Gemini with the MP4 plus the structured-analysis instruction.
4. `video_analysis.py` parses and validates the structured response.
5. Three valid category-specific findings return in timestamp order.
6. The web service stores and renders them. Fewer than three, a timeout, or a validation failure becomes `Analysis incomplete` or `Analysis unavailable`, never `No risks found`.

### Timestamp Navigation — PRD Epic 3

1. The user selects a finding card.
2. The store updates `ActiveFindingId` without changing evidence or decisions.
3. `videoInterop.js` sets `HTMLMediaElement.currentTime` to `start_seconds` and focuses the selected card.
4. The UI preserves state if the user later pauses or seeks manually.

### Live Research Flow — PRD Epic 4

1. The selected card calls `BeginResearch(findingId)`. A single-operation guard disables other research buttons.
2. `AgentClient` opens `POST /v1/research/stream` and consumes newline-delimited JSON events.
3. The service emits `preparing` only after the agent begins forming the objective and returns the visible research task.
4. It emits `searching` immediately before awaiting the real Parallel request.
5. It emits `reviewing` only after a valid Parallel response exists.
6. It emits `ready` only after URL validation, deduplication, relevance summaries, and evidence-contract validation succeed.
7. The web store attaches evidence to the same finding and releases the operation guard.
8. A timeout or invalid result emits `incomplete`, preserves prior successful evidence, and permits Retry.

### Human Decision And Report — PRD Epic 5

1. Investigate, Replace, and License are enabled after successful analysis.
2. Dismiss is rejected unless that finding has a successful evidence result and the user confirms the decision is their judgment.
3. The user may change a decision or note; the store rebuilds the checklist view model immediately.
4. The report remains `Incomplete` while any finding is pending.
5. When all three dispositions exist, `Ready to export` and the print action become available.
6. Print CSS produces the report with clip name, review date, findings, sources, decisions, notes, and the research-assistance disclaimer.

### Reset — PRD Epic 6

1. Start over opens a confirmation dialog.
2. Cancel changes nothing.
3. Confirm replaces the scoped session with a clean instance and seeks the video to zero.
4. No server-side history remains after the Blazor session ends.

## Components And Responsibilities

| Component | PRD coverage | Verification checkpoint |
| --- | --- | --- |
| Opening and workflow shell | Epics 1 and 6 | Primary action and disclaimer visible at laptop width without scrolling. |
| Gemini analyzer | Epic 2 | Fixture and live clip both produce schema-valid ordered findings; incomplete output is rejected honestly. |
| Video/finding interaction | Epic 3 | Each card seeks within normal player precision and retains state. |
| ADK research agent | Epic 4 | Trace or logs prove the agent prepared the task and invoked the registered Parallel tool. |
| Parallel tool | Epic 4 | Live response yields working, non-fabricated, deduplicated URLs. |
| Research timeline | Epic 4 | Stages appear only after their matching backend events. |
| Disposition domain rules | Epic 5 | Unit tests prove no automatic decision and guarded Dismiss. |
| Checklist/report | Epics 5 and 6 | Pending items block printing; three human decisions unlock it. |
| Reset/session store | Epic 6 | Confirm clears all state; cancel preserves it; reload may reset. |

## External APIs And Dependencies

### Google Vertex AI / Gemini

- Authentication: Application Default Credentials from the private agent service account.
- Input: configured Cloud Storage MP4 URI and bounded analysis prompt.
- Output: JSON matching `AnalysisResponse`.
- Limits: one analysis at a time per browser session; one automatic retry only for transient transport failure, not invalid semantic output.
- Configuration: `GOOGLE_CLOUD_PROJECT`, `GOOGLE_CLOUD_LOCATION`, `GEMINI_MODEL`, `DEMO_VIDEO_GCS_URI`.

### Google ADK

- The root agent and Parallel tool live in `agent/app`.
- ADK must be visible in dependencies and runtime code, not only documentation.
- A local ADK test must show the tool registration before deployment.
- Deployment uses the official Cloud Run target and preserves the ADK application entry point.

### Parallel Search

- Authentication: `PARALLEL_API_KEY` loaded server-side from Secret Manager.
- Endpoint: `POST https://api.parallel.ai/v1/search`.
- Required request content: objective and at least one search query.
- Response handling: accept only absolute HTTP(S) URLs; deduplicate; retain title, URL, excerpts, and available publication metadata.
- Cost control: exactly one call per user research action, at most three queries, at most five displayed results, and no automatic research of all findings.

### Cloud Run Identity

- `clearcut-web` permits unauthenticated judge access.
- `clearcut-agent` rejects unauthenticated access.
- The web service obtains an identity token for the private agent audience.
- The browser never receives the agent URL, identity token, Gemini credentials, or Parallel key.

## AI Usage

### Permitted Runtime AI

- Gemini on Google Cloud performs clip understanding and concise relevance synthesis.
- Google ADK orchestrates the constrained research workflow and invokes the partner tool.
- Parallel Search supplies current external evidence at runtime.

No submitted runtime code or asset pipeline may call OpenAI, Anthropic, HeyGen, or another non-permitted generative AI provider. Codex-created planning material is not runtime content. Any final logo, presenter, video, voice, music, or set imagery must be original and created with competition-permitted tooling before being added to the submitted project.

### Prompt And Output Rules

- Use neutral clearance-preparation language.
- State that observations are not legal determinations.
- Ask Gemini to omit uncertain claims rather than inventing details.
- Validate structured outputs before display.
- Display URLs only from the Parallel response.
- Do not expose chain-of-thought; show short operational stage labels and the user-facing research objective.
- Do not let the model choose Dismiss, Investigate, Replace, or License.

### Golden Path Versus Fixtures

Fixtures support tests and offline UI development only. The submitted hosted golden path must make real Gemini and Parallel calls. If a live call fails, the UI shows an honest retryable failure; it must not silently substitute fixture results.

## Risks And Verification

| Risk | Mitigation | Verification |
| --- | --- | --- |
| Gemini misses a planted item | Make the original cues visually/audibly unambiguous; refine prompt; reject incomplete output rather than fabricate. | Run the same clip from a clean session three times during development; require two consecutive final passes. |
| Gemini returns invalid JSON | Use structured response configuration plus Pydantic validation. | Contract tests for missing categories, bad times, duplicates, and extra fields. |
| Parallel returns weak, duplicate, or conflicting sources | Focus queries, bound results, normalize URLs, label uncertainty, allow Retry. | Fixture tests plus live checks that every displayed source opens. |
| Status animation overstates backend progress | Drive the timeline from backend events, not timers or fake percentages. | Review event timestamps and record one real run. |
| Private service is exposed | Require Cloud Run authentication and grant Invoker only to the web service account. | Anonymous request to the agent returns 401/403; web call succeeds. |
| Secret reaches repository or browser | Secret Manager, `.env.example` names only, logging redaction. | Secret scan and browser network inspection before push. |
| Blazor session resets during judging | State is small and temporary by design; keep the golden path under two minutes. | Complete twice in a clean browser and once after an idle interval. |
| Cloud cost exceeds the shared credit | Scale to zero, max one instance, one short clip, manual searches, billing alert. | Inspect Cloud Run settings and billing dashboard before recording. |
| Python 3.14 compatibility problems | Build the agent in a Python 3.12 container. | Docker build and pytest run are mandatory. |
| Submission appears canned | Show live statuses, working source URLs, error behavior, and runtime proof in the demo. | Capture a continuous ≤3-minute video without hidden substitutions. |
| Legal interpretation is implied | Repeated disclaimer, neutral labels, mandatory human decision, no overall “cleared” state. | Search final UI and repo for prohibited claim language. |

## Demo And Submission Flow

The public demonstration should take approximately 100–120 seconds:

1. Open the hosted app and establish the independent-filmmaker problem and disclaimer.
2. Play several seconds of the original fictional commercial.
3. Select **Analyze with Gemini** and show the three timestamped findings.
4. Select the factual-claim finding and jump to its exact moment.
5. Select **Research with Parallel** and keep the visible agent stages on screen.
6. Open one real source card in a new tab, then return without losing state.
7. Apply human dispositions to all three findings.
8. Show the completed checklist and printable report.
9. End on a compact architecture/proof screen naming Gemini, ADK, Parallel, and Cloud Run.

Before recording or submitting:

- Complete the golden path twice from clean browser sessions.
- Confirm every URL opens and no secrets appear in logs or network payloads.
- Confirm both Docker images build and all C# and Python tests pass.
- Confirm the agent service is private and the public URL works without sign-in.
- Confirm the video, logo, presenter, music, and all other assets are original and permitted.
- Replace every planning-status statement in the README with verified behavior only after it exists.
- Keep the public demo video at or below three minutes with English narration or subtitles.
- Ensure the hosted app, repository, Devpost story, and demo video tell the same evidence-chain story.

## Environment Contract

The repository may document these names but must never contain their real values:

```text
GOOGLE_CLOUD_PROJECT=clearcut-agentic-20260901
GOOGLE_CLOUD_LOCATION=<selected-region>
GEMINI_MODEL=<verified-current-model>
DEMO_VIDEO_GCS_URI=gs://<clearcut-bucket>/<original-demo>.mp4
CLEARCUT_AGENT_BASE_URL=https://<private-agent-service>
PARALLEL_API_KEY=<secret-manager-injected>
```

`deploy.ps1` must refuse to run unless `GOOGLE_CLOUD_PROJECT` exactly equals `clearcut-agentic-20260901`. This guard protects the unrelated COR project.

