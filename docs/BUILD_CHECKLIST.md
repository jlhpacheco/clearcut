# ClearCut Build Checklist

## Build Preferences

- **Build mode:** Autonomous coordination; submitted code and assets are created only with competition-permitted Google tooling. Codex and its subagents may plan, review, test, and coordinate but must not author submitted implementation or creative assets.
- **Comprehension checks:** N/A; pause only for credentials, irreversible external actions, or material product decisions.
- **Git:** Commit tested restore points after items 3, 7, 10, 11, and 12.
- **Verification:** Continuous automated tests plus truthful manual checks.
- **Browser verification:** After the application build is stable, use a Google-generated TypeScript Playwright suite with exactly one worker. Cover the golden path, truthful failure states, keyboard use, mobile layout, accessibility basics, and deployed smoke checks.
- **Check-in cadence:** Speed-run with visual pauses after items 3, 8, and 11.
- **Execution grain:** Twelve outcome blocks totaling 20 hours; execute each block through 15–30 minute subcycles.
- **Canonical checklist:** `docs/hackathon-build/checklist.md`; the repository mirror must match it by SHA-256 before each checklist commit.
- **Cloud boundary:** Every cloud command must pass literal project `clearcut-agentic-20260901`, validate its project number, and fail closed on any mismatch. Never list, read, or mutate COR.
- **Runtime AI boundary:** Gemini/Google Cloud AI plus Parallel Search only.
- **Cut line:** Preserve the real Gemini → ADK → Parallel → human-decision chain. Cut print polish, extra animations, or optional proof panels first.
- **Wow moment:** Timestamped Gemini finding → visible agent-generated task/query → one live Parallel invocation → cited evidence → uncertainty → human disposition.

## Checklist

- [ ] **1. Pass provenance, credentials, infrastructure, and retrieval preflight (1.0 h)**
  Spec ref: `spec.md > AI Usage`, `spec.md > Architecture > Google Cloud Boundary`, and `spec.md > Environment Contract`
  What to build: Start `docs/PROVENANCE.md` recording the creator/tool/model/date/ownership of every submitted code and asset; explicitly bar Codex/subagent-authored implementation and creative assets. Verify Docker, .NET 10, a Python 3.12 container, gcloud authentication, billing, required APIs, Vertex region/model access, Parallel account/key/credit, bucket access, and Secret Manager. Capture the ClearCut project number and require every future cloud command to use the literal project. Run an early live Parallel spike against an entrant-owned fictional page or a vetted public-domain/official source; if the owned page is not retrievable, lock the safe-source fallback and text-only demo treatment before UI work.
  Acceptance: The build can proceed without hidden credential or infrastructure blockers; the test search returns a real URL; provenance rules protect submission eligibility; no command touches COR; and a safe golden-path source strategy exists.
  Verify: Record tool versions and project ID/number, prove billing and API access in ClearCut, confirm the bucket is non-public, confirm the Parallel secret exists without printing it, run exactly one sanitized search, and inspect cloud command templates for explicit `--project clearcut-agentic-20260901`.

- [ ] **2. Generate the guarded two-service skeleton and shared contracts (1.5 h)**
  Spec ref: `spec.md > File Structure`, `spec.md > Architecture > Gemini Video Analyzer`, and `spec.md > Architecture > Temporary Review Session`
  What to build: Using permitted Gemini-powered Google coding tooling, create the .NET 10 Blazor solution/tests, Python 3.12 agent service/tests, Dockerfiles, non-secret configuration examples, typed findings/evidence/events/dispositions, and temporary review-session invariants. Add a cross-language golden JSON contract consumed by both Pydantic and C#.
  Acceptance: One operation may run at a time; findings sort chronologically; malformed or incomplete sets are rejected; Dismiss requires successful evidence and confirmation; export requires three human decisions; reset is explicit; and C#/Python fields and enums cannot drift.
  Verify: Run `dotnet build`, `dotnet test`, Python schema tests in the 3.12 container, the cross-language fixture check, both container builds, a fake-project deployment-guard test, provenance review, and a secret scan before the first implementation commit.

- [ ] **3. Generate the cinematic fixture-driven review experience (1.5 h) — visual pause 1**
  Spec ref: `spec.md > Architecture > Public Web Service — ClearCut.Web`
  What to build: With permitted Google coding tooling, implement the opening, video region, finding cards, evidence area, four-stage timeline, disposition controls, and checklist in a responsive midnight-navy/teal Blazor page. Use explicit fixtures only for local UI development and label them so they cannot silently enter a live run. Integrate the already Google-generated ClearCut logo and its provenance.
  Acceptance: PRD Epic 1 and Story 3.1 are visible; fictional-media labeling, workflow explanation, disclaimer, and **Analyze with Gemini** appear without scrolling at 1280×720; the layout works near 390 px; and the fixture path is visibly distinguishable from runtime mode.
  Verify: Run the local app, complete a keyboard-only pass, test 1280×720 and ~390 px, run axe/Lighthouse or an equivalent reproducible accessibility check with no critical violations, verify the logo provenance, show Jose Luis the first visual, then commit.

- [ ] **4. Produce the original media and safe evidence corpus (2.0 h)**
  Spec ref: `spec.md > Stack` and `spec.md > Demo And Submission Flow`
  What to build: Using competition-permitted Google tooling, produce and encode one original 30–45 second synthetic near-final scene from a fictional independent science-fiction film. Its ad-saturated environment must contain exactly three organic review questions: a fictional brand mark, a precise product claim, and an original music cue. Create or select the safe text evidence corpus established in item 1. Store a web-playable copy and a private Cloud Storage copy and document prompts, models, dates, ownership, and sources.
  Acceptance: No real marks, ads, slogans, posters, copyrighted music, COR/HeyGen material, or private footage appear; the three cue timestamps are clear; browser and Gemini copies are byte-identical; and the recorded demo never needs to open a third-party webpage.
  Verify: Review the complete clip with audio, confirm H.264/AAC browser playback and 30–45 second duration, record exact cue timestamps, compare SHA-256 hashes of local and downloaded GCS copies, verify bucket privacy/project ownership, and update `docs/PROVENANCE.md`.

- [ ] **5. Generate the fixture-backed private ADK service (1.5 h)**
  Spec ref: `spec.md > Architecture > Private Agent Service — ClearCut.Agent` and `spec.md > External APIs And Dependencies > Google ADK`
  What to build: With permitted Google coding tooling, implement validated settings, health/analyze/research-stream routes, fixture adapters, the ADK root agent, and automated proof that exactly the intended Parallel tool is registered. Keep the service private by design and make fixture mode impossible when production configuration is active.
  Acceptance: The Python 3.12 service starts, reports health, rejects malformed requests, streams schema-valid events, visibly uses Google ADK runtime code, registers one intended Parallel tool, and cannot silently substitute fixtures in production.
  Verify: Run pytest in the container, call health, send invalid requests, test fixture lockout, assert the registered tool list automatically, inspect the runtime dependency tree, and review provenance.

- [ ] **6. Connect and stabilize real Gemini video analysis (2.0 h)**
  Spec ref: `spec.md > Data Flow > Analysis Flow — PRD Epic 2` and `spec.md > Risks And Verification`
  What to build: Use Gemini on Vertex AI to analyze the configured `gs://` clip with a neutral structured prompt; validate timestamps, IDs, categories, observations, and exactly one finding per planted category; sort chronologically; and return honest incomplete/unavailable states.
  Acceptance: A real call satisfies PRD Epic 2; failures never become “No risks found”; malformed JSON, duplicate IDs, unsupported categories, out-of-range times, and fewer than three findings are rejected; only one automatic retry is allowed for transient Gemini transport failure.
  Verify: Run contract/error tests, analyze the real clip three times during development, require two consecutive schema-valid three-category passes, manually compare timestamps to the video, capture sanitized Gemini request/result evidence, and confirm all charges belong to ClearCut.

- [ ] **7. Connect Parallel through the ADK agent and pin dependencies (2.0 h)**
  Spec ref: `spec.md > Architecture > ADK Research Agent`, `spec.md > Architecture > Parallel Search Tool`, and `spec.md > External APIs And Dependencies > Parallel Search`
  What to build: Implement the raw Parallel adapter, URL/excerpt normalization, explicit user-triggered timeout/retry behavior, and ADK tool invocation. The agent must generate the visible objective and 1–3 queries from the selected finding, call Parallel exactly once, return at most five deduplicated HTTP(S) sources, and label conflicts/weak results. After the first live success, pin dependencies and lock files.
  Acceptance: Displayed URLs/excerpts map exactly to the raw live response; no model-memory URL is shown; empty/error results become **Evidence incomplete**; automatic Parallel retries are prohibited; the API key stays server-side; and the agent trace proves meaningful tool use rather than a fixed status animation.
  Verify: Run adapter/duplicate/timeout tests, invoke one live search through ADK rather than directly, capture a sanitized finding→objective→queries→single tool call→sources trace, assert no second call, open links off-camera, rebuild both containers cleanly from locks, scan the full Git history/current tree for secrets, then commit.

- [ ] **8. Join C# and Python into the live evidence chain (2.0 h) — visual pause 2**
  Spec ref: `spec.md > Data Flow > Live Research Flow — PRD Epic 4`
  What to build: With permitted Google coding tooling, implement the C# agent client, local development auth handler, arbitrary-chunk NDJSON parser, cancellation, operation-lock release, timestamp seeking, source-card mapping, and truthful UI stages driven only by backend events. Defer Cloud Run identity tokens to item 11.
  Acceptance: PRD Epics 3 and 4 work locally end to end; prior successful evidence survives a failed retry; malformed/out-of-order events fail honestly; disconnect/cancel releases the lock; and the on-screen trace connects the real finding, generated objective/query, exact Parallel call, returned sources, uncertainty, and later human decision.
  Verify: Test arbitrary NDJSON chunk boundaries, malformed/order errors, cancel/disconnect, retry, prior-evidence preservation, and fixture/runtime separation; complete one real chain; verify sources match the sanitized raw trace; show Jose Luis the wow-moment visual.

- [ ] **9. Complete decisions, report, and measurable impact (1.5 h)**
  Spec ref: `spec.md > Data Flow > Human Decision And Report — PRD Epic 5` and `prd.md > Submission Proof Points > Potential Impact`
  What to build: Wire the four human dispositions, Dismiss confirmation, notes, live checklist, readiness state, and print view. Add an honest demo measurement panel/report line: elapsed time from Analyze to ready-to-print package, three timestamped findings, number of sources gathered, and clearly labeled workflow steps consolidated. Add a short source-backed independent-filmmaker problem statement without unsupported ROI claims.
  Acceptance: Pending findings block export; all three human choices unlock it; decisions update immediately; no overall “cleared/approved” claim appears; and impact is demonstrated with observable demo counts/time rather than invented savings.
  Verify: Run disposition/report tests, exercise all choices and Dismiss guard, print preview, compare displayed metrics to captured timestamps/source counts, validate problem citations, and search rendered copy for prohibited or unsupported claims.

- [ ] **10. Harden failures, security, accessibility, and first-time usability (2.0 h)**
  Spec ref: `spec.md > Risks And Verification` and `prd.md > Edge Cases`
  What to build: Implement precise retry semantics, truthful failures, start-over confirmation, responsive/focus states, disagreement treatment, privacy copy, safe correlation IDs, and judge-facing clarity. With permitted Google coding tooling, add a robust TypeScript Playwright suite configured with exactly one worker. Conduct one unaided test with a person who has not built the app and fix all severity-1 blockers.
  Acceptance: Every PRD edge case has explicit behavior; keyboard/mobile use is practical; failures never imply safety; the user completes the golden path in under two minutes without coaching; and Gemini, ADK, Parallel, uncertainty, and human responsibility are evident without developer tools.
  Verify: Run all test suites and the full edge-case table; run the TypeScript Playwright suite with exactly one worker across the golden path, truthful failures, keyboard navigation, ~390 px mobile layout, and accessibility basics; record fresh-user completion time/confusion/keyboard/mobile failures and fixes; scan full Git history, container layers, Cloud Build output, and local logs for secrets; run `git diff --check`; then commit.

- [ ] **11. Provision, deploy, and smoke-test the frugal public MVP (1.5 h) — visual pause 3**
  Spec ref: `spec.md > Architecture > Google Cloud Boundary` and `spec.md > External APIs And Dependencies > Cloud Run Identity`
  What to build: Provision dedicated least-privilege service accounts/IAM, private bucket, Secret Manager injection, request timeouts, Artifact Registry, private agent Cloud Run service, and public web Cloud Run service. Every command must include the literal ClearCut project and verified project number. Implement `deploy/smoke-test.ps1`, identity-token web→agent calls, and deployed stream flushing; set minimum instances 0 and maximum 1.
  Acceptance: The hosted app is anonymous; direct agent access returns 401/403; web→agent succeeds; storage is non-public; the browser sees no agent URL/token/key; MP4 range seeking works; deployed streaming is incremental; two clean-session golden paths complete under two minutes; and COR is untouched.
  Verify: Inspect IAM/scaling/timeouts/secrets/bucket policy, run signed-out public checks and `deploy/smoke-test.ps1`, run the TypeScript Playwright deployed smoke suite with exactly one worker, test MP4 range requests, agent denial, authenticated service call, stream flushing, browser traffic, Cloud Run/Cloud Build logs and image layers, and two clean golden paths; show Jose Luis the deployed MVP, update README only with verified facts, then commit.

- [ ] **12. Prepare the submission package and Devpost handoff (1.5 h)**
  Spec ref: `spec.md > Demo And Submission Flow` and `prd.md > Submission Proof Points`
  What to build: Select 3–5 screenshots, finalize the story and measured impact, record/caption a ≤3-minute YouTube demo, and assemble runtime/provenance/testing proof. The recording must show text-only evidence inside ClearCut and must not open external source pages that may expose third-party branding. After an action-time approval, update YouTube and the Devpost draft; never submit without the separate required final confirmation.
  Acceptance: Stage One deliverables and all four equal-weight Stage Two criteria have visible proof; the recorded golden path uses live Gemini and live Parallel rather than fixtures; repository/app/video/Devpost tell the same story; public repo displays Apache-2.0 in GitHub About; and no prohibited third-party creative element is shown.
  Verify: Confirm canonical/mirror checklist SHA-256 match, run the final submission checklist, inspect repo/license/app/video/captions/source links signed out, verify live-call evidence and asset provenance, confirm video duration and no external-page footage, verify Devpost remains unsubmitted, confirm next command is `$prepare-submission`, and create the final pre-submission commit.
