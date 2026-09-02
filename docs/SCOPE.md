# ClearCut MVP Scope

## Goal

Build a coherent, judgeable agent experience that helps an independent filmmaker turn one short rough cut into a timestamped, cited clearance-preparation checklist.

## Primary user

An independent producer or post-production supervisor preparing a project for attorney, distributor, or errors-and-omissions review.

## Problem statement

Small film teams often discover clearance questions late, when finding the relevant shot, forming the right research question, collecting credible evidence, and documenting a decision are slow and fragmented. ClearCut compresses that preparation workflow without making legal conclusions.

## 20-hour constraint

The product is time-boxed to 20 focused build hours. Every feature must strengthen either the end-to-end evidence chain or submission readiness.

## Core user flow

1. Open the hosted web application.
2. Load the included original 30–45 second fictional commercial.
3. Run Gemini multimodal analysis.
4. Review exactly three timestamped candidates: fictional brand mark, factual claim, and original music cue.
5. Select one candidate and inspect the Gemini-generated research task.
6. Start a visible live Parallel Search.
7. Review cited evidence and source links.
8. Choose dismiss, investigate, replace, or license.
9. View and export the final clearance-preparation checklist.

## In scope

- One included, original demonstration video
- Exactly three seeded review candidates
- Timestamped Gemini multimodal analysis
- Deterministic multi-step ADK/Agent Builder workflow
- Live Parallel Search invoked at runtime
- Visible tool status and cited evidence
- Human disposition for each finding
- Final checklist view and lightweight export
- Polished responsive web interface
- Cloud Run deployment
- Clear setup, architecture, and verification documentation

## Out of scope

- Legal opinions, clearance approval, or E&O certification
- Real client footage or third-party intellectual property
- User accounts, organizations, permissions, or collaboration
- Multiple videos or production-scale batch processing
- Payments, subscriptions, or billing
- Studio asset-management integrations
- Long-term evidence storage or complex databases
- Automatic outreach, licensing negotiation, or rights acquisition
- Native mobile applications

## Success criteria

- A judge can complete the core flow without instruction in under two minutes.
- The UI displays exact timestamps and lets the judge jump to each finding.
- The demo visibly proves a real Gemini call, agent tool decision, and live Parallel Search call.
- Every displayed evidence item includes a source URL and retrieval status.
- The final disposition is explicitly human-selected.
- The hosted app, repository, and first three minutes of the demo video tell the same story.
- The repository contains detectable Apache-2.0 licensing and reproducible run instructions.

## Safety constraints

- Always label output as research assistance, not legal advice.
- Never state that a clip is legally cleared.
- Use only newly created fictional demo content.
- Keep secrets in environment configuration or Secret Manager; never commit them.
- Permit only competition-approved AI and partner tooling in submitted code and assets.

## Planned time allocation

| Workstream | Hours |
| --- | ---: |
| Final design and demo asset preparation | 3 |
| Gemini analysis and structured findings | 4 |
| ADK orchestration and Parallel integration | 5 |
| C# / Blazor evidence-review experience | 4 |
| Cloud Run deployment and verification | 2 |
| Demo video, README, and submission QA | 2 |
| **Total** | **20** |

## Deferred opportunities

After the hackathon: real production uploads, multi-project workspaces, collaborative approvals, rights-holder directories, E&O workflows, audit history, and enterprise integrations.
