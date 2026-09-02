# Product Requirements Document

## Product Summary

ClearCut is a clearance-preparation assistant for independent filmmakers and small production teams approaching final cut or distribution. It helps a producer review one short rough cut, locate material that may require follow-up, research selected findings with current sources, record human decisions, and produce a timestamped checklist for downstream professional review.

ClearCut is not a legal service. It does not determine whether material is legally cleared, replace counsel, certify errors-and-omissions readiness, or make a final rights decision. Its job is to make the preparation work visible, organized, and evidence-backed.

The hackathon MVP is a focused, anonymous demonstration built around one newly created 30–45 second fictional commercial. The clip contains three planted review candidates: a fictional brand mark, a factual claim, and an original music cue.

## Target User

### Primary user

An independent producer or post-production supervisor preparing a project for attorney, distributor, broadcaster, platform, or errors-and-omissions review.

### User context

- They are near final cut and under time pressure.
- They know the film but may not know how to organize clearance research.
- They need exact timestamps, current sources, and a defensible record of what still needs attention.
- They do not have a studio-scale clearance department.
- They remain responsible for every decision and may escalate findings to qualified professionals.

### Primary user need

“Show me where I should look, help me gather useful evidence, and give me a clear checklist—without pretending the software can make the legal decision for me.”

## Experience Principles

1. **Calm, not alarming.** Use neutral review language and a cool, cinematic visual system. Avoid red-alert styling and legal-threat language.
2. **Evidence before confidence.** Show observations, sources, and uncertainty instead of an unexplained risk score.
3. **Human accountability.** ClearCut may recommend research, but only the user records a disposition.
4. **Visible agent work.** The transition from video finding to research task to Parallel evidence must be understandable on screen.
5. **One excellent path.** The included demo clip and its three findings matter more than broad upload, account, or collaboration features.
6. **Truthful status.** Never fabricate a missing finding, source, successful search, or completed clearance decision.

## Core User Journey

1. The user opens ClearCut and sees the included fictional commercial ready for review.
2. The page briefly explains the outcome and states that ClearCut provides research assistance, not legal advice.
3. The user selects **Analyze with Gemini**.
4. ClearCut visibly analyzes the clip and returns three valid findings in chronological order.
5. The user selects a finding; the video seeks to its timestamp and the related card becomes active.
6. The user selects **Research with Parallel** for one finding.
7. ClearCut visibly progresses through task preparation, live search, source review, and evidence readiness.
8. The user reviews 3–5 cited sources and any uncertainty or disagreement.
9. The user assigns a human disposition to each finding: dismiss, investigate, replace, or license.
10. ClearCut updates a final checklist and enables a printable report when every finding has a disposition.
11. The user may change a decision, open a source, retry incomplete research, or start over.

## Epics And User Stories

### Epic 1: Begin a focused review

#### Story 1.1 — Arrive ready to work

As an independent producer, I want the demonstration cut already available so that I can understand ClearCut immediately without preparing a file or account.

Acceptance criteria:

- The first screen identifies the product as **ClearCut** and describes the outcome in one short sentence.
- The original demonstration clip is visible and ready to play.
- The clip is clearly labeled as fictional demonstration media.
- A single primary action, **Analyze with Gemini**, is visible without scrolling on a typical laptop display.
- A short “research assistance, not legal advice” statement is visible before analysis begins.
- No sign-in, account creation, payment, project setup, or upload is required.
- Before analysis, the findings area shows a calm empty state explaining that timestamped review candidates will appear there.

#### Story 1.2 — Understand the review sequence

As a first-time user, I want a compact explanation of the workflow so that I know what will happen after I start.

Acceptance criteria:

- The opening screen communicates the sequence: analyze the cut, investigate findings, review sources, make decisions, and prepare the checklist.
- The explanation fits in one compact section and does not compete visually with the primary action.
- The language uses “finding,” “review,” and “evidence,” not “violation,” “illegal,” or “automatically cleared.”

### Epic 2: Analyze the demonstration cut

#### Story 2.1 — Start analysis with confidence

As a producer, I want clear feedback after starting analysis so that I know the application is working and do not trigger duplicate runs.

Acceptance criteria:

- Selecting **Analyze with Gemini** starts one analysis run.
- The primary action becomes unavailable while analysis is active.
- The screen displays a visible in-progress state associated with Gemini.
- The video remains visible while analysis runs.
- The user is not shown fabricated percentage progress.
- Repeated clicks cannot create simultaneous analysis runs.
- A successful run transitions directly to the findings view without requiring a page refresh.

#### Story 2.2 — Receive valid timestamped findings

As a producer, I want the strongest review candidates tied to exact moments so that I can inspect the relevant material quickly.

Acceptance criteria:

- The successful golden-path analysis displays exactly three valid findings from the included clip.
- The three findings correspond to the planted fictional brand mark, factual claim, and original music cue.
- Findings are ordered by their first timestamp.
- Each finding includes a stable label, category, start timestamp, neutral observation, review priority, and suggested research objective.
- “Review priority” is descriptive and does not claim to be a legal risk score.
- Every finding shown must be grounded in the analyzed clip; ClearCut never invents a missing third result.
- If fewer than three valid findings are returned, ClearCut displays **Analysis incomplete** and offers Retry.
- If more than three valid observations are returned, the MVP displays the three strongest review candidates and clearly labels the view as the focused demo review.

#### Story 2.3 — Recover from analysis failure

As a user, I want a useful error state when analysis fails so that I know whether to retry rather than assuming the clip is clear.

Acceptance criteria:

- A failed or timed-out analysis shows **Analysis unavailable** or **Analysis incomplete**, never “No risks found.”
- The error state distinguishes failure from a successful zero-finding result.
- A Retry action is visible.
- Retrying starts a new analysis run and clears the prior error message.
- Starting over remains available after failure.

### Epic 3: Inspect and navigate findings

#### Story 3.1 — Scan the findings beside the cut

As a producer, I want findings presented as concise cards so that I can understand the review at a glance.

Acceptance criteria:

- Three finding cards appear in chronological order beside or directly below the video, depending on screen width.
- Every card shows category, timestamp, short observation, review priority, and current research status.
- Categories are visually distinguishable without relying on color alone.
- One finding can be active at a time.
- The active finding has a clear selected state.
- Cards do not display a legal conclusion or a percentage chance of infringement.
- The layout remains usable on a narrow screen without hiding the timestamp or primary action.

#### Story 3.2 — Jump to the relevant moment

As a producer, I want selecting a finding to move the video to the right moment so that I can verify the observation myself.

Acceptance criteria:

- Selecting a finding seeks the video to that finding’s start timestamp.
- The selected card becomes active at the same time.
- The timestamp shown on the card matches the video position within normal player precision.
- Selecting findings in any order works.
- Pausing, replaying, or manually seeking the video does not erase findings or decisions.

### Epic 4: Build a live evidence chain

#### Story 4.1 — Start focused research deliberately

As a producer, I want to choose which finding is researched so that the search is relevant, visible, and under my control.

Acceptance criteria:

- The active finding includes a **Research with Parallel** action.
- Research does not start automatically when analysis completes or when a card is selected.
- The user may research findings in any order.
- Only one live research run may be active at a time.
- While one search is active, other research actions are temporarily unavailable and explain why.
- Starting research preserves the selected finding and video position.

#### Story 4.2 — See what the agent is doing

As a user and hackathon judge, I want the research workflow exposed on screen so that I can understand and verify the agentic behavior.

Acceptance criteria:

- The application displays these stages in order: **Preparing research task**, **Searching with Parallel**, **Reviewing sources**, and **Evidence ready**.
- The prepared research task is visible in concise human-readable language.
- The interface identifies Gemini/agent preparation separately from the Parallel search step.
- A stage is marked complete only after that stage actually succeeds.
- The display does not expose secrets, raw credentials, internal prompts, or private infrastructure details.
- The successful demo makes the Parallel runtime step visible without requiring developer tools or narration.

#### Story 4.3 — Review useful cited evidence

As a producer, I want concise evidence cards with working sources so that I can decide what requires follow-up.

Acceptance criteria:

- A successful search displays between 3 and 5 source cards when that many useful results are available.
- Each source card includes title, publisher or domain, retrieval date, short relevance summary, and **Open source** action.
- Source URLs come from the completed research result and are not fabricated by the interface.
- Opening a source uses a new browser tab and does not reset the ClearCut session.
- The evidence section distinguishes source facts from ClearCut’s relevance summary.
- Duplicate source URLs are shown only once.
- If credible sources disagree, the evidence area explicitly labels the disagreement or uncertainty.
- Source count and research status are visible on the related finding card.

#### Story 4.4 — Handle weak or failed research honestly

As a producer, I want incomplete research labeled clearly so that I do not mistake missing evidence for a safe result.

Acceptance criteria:

- A timeout, tool error, or result with no useful sources displays **Evidence incomplete**.
- The finding remains visible and selected after the failure.
- A Retry action is available.
- The application never converts an empty result into “no concern.”
- If prior successful evidence exists and a retry fails, the prior evidence remains visible and is labeled as the previous successful result.
- A finding with incomplete evidence can still be marked **Investigate**, but not **Dismiss**.

### Epic 5: Record accountable human decisions

#### Story 5.1 — Assign a clear disposition

As a producer, I want a small set of understandable decisions so that every finding has an explicit next action.

Acceptance criteria:

- Each finding supports exactly four dispositions: **Dismiss**, **Investigate**, **Replace**, and **License**.
- The interface briefly defines each disposition:
  - Dismiss: no further action after human review.
  - Investigate: more research or professional review is required.
  - Replace: remove or substitute the material.
  - License: seek permission or licensing.
- Investigate, Replace, and License are available after analysis.
- Dismiss becomes available only after that finding has successful evidence and the user confirms it is a human judgment.
- The system never selects a disposition automatically.
- The user may change a disposition at any time.
- Changing a disposition updates the related card and final checklist immediately.
- A finding without a selected disposition remains visibly marked **Pending review**.

#### Story 5.2 — Complete the review checklist

As a producer, I want one final summary so that I can see what is resolved, what remains open, and what evidence supports each decision.

Acceptance criteria:

- The checklist contains all three findings in timestamp order.
- Each row includes category, timestamp, observation, research status, source count, human disposition, and optional reviewer note.
- The report is labeled **Incomplete** while any finding remains Pending review.
- The report is labeled **Ready to export** only after all three findings have a human disposition.
- The printable report action is unavailable until all three dispositions are selected.
- The final report includes the clip name, review date, and a prominent research-assistance disclaimer.
- The report never uses “cleared,” “approved,” or equivalent language as its overall status.
- Printing or saving the report does not alter the current session.

### Epic 6: Preserve trust and recover safely

#### Story 6.1 — Understand the product boundary

As a user, I want ClearCut’s limitations communicated at the right moments so that I do not mistake research support for legal advice.

Acceptance criteria:

- A concise disclaimer appears on the opening screen and final report.
- Evidence and decision screens state that the user remains responsible for the disposition.
- Failure and empty states do not imply legal safety.
- The language encourages professional review where appropriate without making alarmist claims.

#### Story 6.2 — Start over intentionally

As a user, I want to reset the demonstration safely so that I can repeat the workflow without stale findings.

Acceptance criteria:

- A **Start over** action is available after analysis begins.
- Selecting Start over displays a confirmation explaining that findings, evidence, notes, and decisions will be cleared.
- Cancel leaves the session unchanged.
- Confirm returns to the opening state with the original demo clip ready.
- A reset does not create or retain an account, project, or history.

#### Story 6.3 — Know what happens to session data

As a privacy-conscious filmmaker, I want the anonymous demo’s temporary nature stated clearly so that I understand its limitations.

Acceptance criteria:

- The interface states that the MVP is an anonymous demonstration and does not preserve a workspace.
- Reloading or closing the app may reset the session.
- The included clip is the only supported MVP media.
- No promise of permanent storage, collaboration, or audit history is made.

## Edge Cases

| Situation | Required behavior |
| --- | --- |
| User clicks Analyze repeatedly | Only one analysis runs; the action remains unavailable until completion or failure. |
| Gemini returns fewer than three valid findings | Show Analysis incomplete and Retry; do not invent findings. |
| Gemini returns more than three observations | Show the three strongest demo-relevant findings and label the view as focused. |
| User selects findings out of order | Seek correctly and preserve all prior evidence and decisions. |
| User starts another search while one is active | Keep the current search active; disable other research actions with a short explanation. |
| Parallel returns no useful sources | Show Evidence incomplete; never interpret this as no concern. |
| Parallel times out | Preserve the finding, show Retry, and allow Investigate. |
| Sources conflict | Label uncertainty and show the relevant sources without choosing a winner. |
| Duplicate source URLs are returned | Show the source once. |
| User opens an external source | Open a new tab and preserve the ClearCut session. |
| User tries to dismiss without successful evidence | Keep Dismiss unavailable and explain that evidence review is required. |
| User tries to export with a pending finding | Keep export unavailable and identify the unfinished finding. |
| User changes a decision after viewing the report | Update the report immediately. |
| User selects Start over accidentally | Require confirmation before clearing the session. |
| Page is reloaded | The session may reset; the opening screen explains the anonymous-demo behavior. |

## What We Are Building

- One polished first-run experience with the original fictional clip already available
- One Gemini-triggered analysis flow with honest loading, success, incomplete, and failure states
- Exactly three golden-path findings with timestamp navigation
- Finding cards with neutral review priority and research status
- User-triggered, one-at-a-time Parallel research
- A visible four-stage evidence chain
- 3–5 cited evidence cards per successful search when available
- Explicit handling of weak, missing, duplicate, and conflicting sources
- Four human dispositions with a safeguard around Dismiss
- A live-updating three-item final checklist
- A printable report after every finding receives a decision
- Anonymous-session reset and clear safety language
- Responsive, cool, calm, Hollywood-inspired presentation

## Non-Goals For The MVP

- No user profiles, because the judgeable workflow works anonymously.
- No general upload system, because one controlled original clip creates a more reliable 20-hour demo.
- No multiple projects or batch processing, because they do not strengthen the core evidence chain.
- No collaborative review or permissions, because a single producer persona is sufficient for validation.
- No permanent workspace or audit history, because persistence adds complexity without improving the primary demo.
- No automated legal conclusion, because the product supports—not replaces—human and professional review.
- No automatic licensing outreach, payment, or rights acquisition, because these require external workflows and legal safeguards.
- No native mobile application, because a responsive web experience satisfies the submission and demo needs.

## What We Would Add With More Time

- Secure uploads for real production clips
- Multiple projects and reusable clearance workspaces
- Team comments, assignments, and approval roles
- Saved evidence history and audit trails
- Custom risk taxonomies and production policies
- Rights-holder and licensing workflow integrations
- Attorney or E&O reviewer handoff packages
- Batch analysis for long-form content
- Source freshness monitoring and re-research reminders
- Organization-level privacy, retention, and governance controls

## Submission Proof Points

### Technological Implementation

- A real clip analysis produces the visible findings.
- The on-screen agent stages distinguish task preparation from the live Parallel step.
- Successful evidence cards preserve real source URLs.
- Failure states prove the app is not relying on a pre-rendered happy path.

### Design

- A first-time judge can move from opening screen to findings without instruction.
- Selecting a finding seeks the video to the exact moment.
- The evidence chain is understandable without developer tools.
- The experience ends in a complete, printable human-reviewed artifact.

### Potential Impact

- The product names a specific underserved user: the independent producer or post-production supervisor.
- The demonstration addresses late, fragmented clearance preparation.
- The output is directly useful for downstream attorney, distributor, or E&O conversations while preserving honest boundaries.

### Quality Of The Idea

- ClearCut connects multimodal film understanding to current web evidence rather than offering generic chat.
- Parallel is essential to the user-visible workflow, not an incidental integration.
- The creative value is the transparent evidence chain and accountable human conclusion.

### Internal Release Bar

- The entire golden path succeeds twice from a clean browser session.
- The first research run visibly reaches Evidence ready and displays working citations.
- All three findings can receive dispositions and unlock the printable report.
- No page contains real third-party marks, copyrighted posters, unlicensed music, secrets, placeholder claims, or misleading legal language.
