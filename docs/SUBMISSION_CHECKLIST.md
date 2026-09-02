# Submission Checklist

Deadline: **September 9, 2026 at 2:00 PM Pacific Time**.

## Build verification

- [ ] The included demonstration clip is newly created, fictional, and 30–45 seconds long.
- [ ] Gemini analyzes the real clip at runtime.
- [ ] Three findings have accurate, clickable timestamps.
- [ ] Google ADK/Agent Builder performs a real multi-step workflow.
- [ ] Parallel Search is imported or configured and actually called at runtime.
- [ ] The UI visibly distinguishes Gemini analysis, agent orchestration, and Parallel retrieval.
- [ ] Evidence cards preserve working source URLs.
- [ ] The user can choose dismiss, investigate, replace, or license.
- [ ] The final checklist accurately reflects the human selections.
- [ ] Output is labeled research assistance, not legal advice.
- [ ] A clean-session end-to-end test passes twice.

## Repository verification

- [x] Repository is public.
- [x] Apache License 2.0 exists at the repository root.
- [ ] All implementation source code is present.
- [ ] All required original assets are present or reproducibly generated.
- [ ] README contains exact local run instructions.
- [ ] README contains exact deployment instructions.
- [ ] Example environment file names every required variable without secrets.
- [ ] Runtime Google and Parallel packages/configuration are easy to locate.
- [ ] No secret, token, billing identifier, private URL, or personal data is committed.
- [ ] A fresh clone succeeds using only documented steps.

## Hosted application

- [ ] C# application is deployed to Cloud Run in the isolated ClearCut project.
- [ ] Agent service is deployed to Google Cloud where required.
- [ ] Cloud Run minimum instances are zero.
- [ ] Billing alerts and frugal request limits are enabled.
- [ ] Public judge URL works in a signed-out browser.
- [ ] Error and timeout states are understandable.

## Demo video

- [ ] Video is no longer than 3:00.
- [ ] Video shows the functioning product, not only a cinematic trailer.
- [ ] English narration or English subtitles are present.
- [ ] First 20 seconds state the independent-filmmaker problem and ClearCut outcome.
- [ ] Video shows the original clip and timestamped findings.
- [ ] Video clearly captures Gemini, the agent step, and live Parallel Search.
- [ ] Video opens a cited source and records a human disposition.
- [ ] Video ends on the completed checklist and impact statement.
- [ ] No third-party advertising, slogans, logos, trademarks, copyrighted posters, or unlicensed music appear.
- [ ] Video is publicly visible on YouTube or Vimeo.

## Devpost fields

- [ ] Project name: ClearCut
- [ ] Tagline is final and under 200 characters.
- [ ] Description reflects only verified features.
- [x] Submitter type: Individual.
- [x] Government-employee answer: No.
- [x] Country of residence: United States.
- [x] Project marked New.
- [x] Partner track: Parallel.
- [x] Team size: 1.
- [x] Repository URL: https://github.com/jlhpacheco/clearcut
- [ ] Hosted project URL is entered.
- [ ] Google Cloud products list matches actual runtime use.
- [ ] Other tools list matches actual runtime use.
- [x] First-time Parallel user.
- [ ] Non-applicable IBM, Grafana, Clickhouse, and Replit questions use the provided N/A options.
- [ ] Public demo video URL is entered.

## Final compliance gate

- [ ] Only competition-permitted Google Cloud AI and chosen partner AI/tooling appear in submitted implementation and assets.
- [ ] No OpenAI, Anthropic, Microsoft, AWS, or other prohibited AI model/API/framework is present in submitted code or assets.
- [ ] Every technology named in README and Devpost is actually used.
- [ ] Hosted app, repository, Devpost description, and video tell the same truthful story.
- [ ] Final submission is completed before the deadline.

Do not check an item because it is planned. Check it only after direct verification.
