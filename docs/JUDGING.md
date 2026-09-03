# Judging Evidence Map

ClearCut is optimized for the official Agentic Cinema judging process. This file is a living proof map; **planned evidence is not marked complete until verified**.

Official source: [Devpost rules and judging criteria](https://agentic-cinema.devpost.com/rules).

## Stage One: pass/fail viability gate

| Requirement | ClearCut proof | Status |
| --- | --- | --- |
| Functional agent addressing a media and entertainment workflow | End-to-end clearance-preparation flow | Planned |
| Gemini and Google Cloud Agent Builder used at runtime | Runtime imports, calls, tool trace, and demo footage | Not yet verified |
| Partner service used at runtime | Parallel Search call visible in code, UI, and video | Not yet verified |
| Hosted project URL | Public Cloud Run URL | Pending deployment |
| Public open-source repository | This public repository | Ready |
| Complete OSI license | Apache License 2.0 at repository root | Ready |
| All source, assets, and run instructions | Repository completeness audit | Pending implementation |
| Public demo video, no longer than 3 minutes | YouTube or Vimeo URL; English narration or subtitles | Pending |
| New project created during contest period | Repository and project history | Planned verification |
| Original, non-infringing submission content | Synthetic fictional near-final film scene, mark, presenter, music, and set | Pending asset creation |

## Stage Two: equal-weight criteria

### 1. Technological Implementation

**Judge question:** How well is the project built, and how effectively does it use Google Cloud and the partner service?

Target evidence:

- Gemini performs real multimodal analysis rather than displaying canned findings.
- Google ADK/Agent Builder controls a visible, deterministic multi-step workflow.
- Parallel Search is imported or configured and actually called at runtime.
- Source URLs travel from Parallel through the agent contract into the C# interface.
- Cloud Run deployment, secret handling, structured validation, timeouts, and failure states work.
- README contains verified local and cloud run instructions.

### 2. Design

**Judge question:** Is this a complete, coherent product experience rather than only a technical proof of concept?

Target evidence:

- One calm, cinematic workflow from video to checklist.
- Timestamp markers connect each finding directly to the relevant shot.
- The user always knows whether Gemini, the agent, or Parallel is working.
- Evidence cards prioritize source, relevance, and uncertainty.
- Human decisions are prominent and irreversible claims are avoided.
- The demo can be completed without hidden setup or verbal explanation.

### 3. Potential Impact

**Judge question:** Does the project make a credible, specific case for a real audience and demonstrate that it addresses the problem?

Target evidence:

- Named beachhead: independent filmmakers and small production teams.
- Named user: producer or post-production supervisor near final cut.
- Concrete pain: late, fragmented clearance preparation before distributor or E&O review.
- Demonstrated outcome: a timestamped, cited, human-reviewed preparation package.
- Honest boundary: the tool reduces preparation effort but does not replace counsel.

Research anchors:

- [WIPO — Rights Clearance: A Guide for Independent Filmmakers](https://www.wipo.int/web-publications/rights-clearance-a-guide-for-independent-filmmakers/assets/90943/rights-clearance-a-guide-for-independent-filmmakers-en-WEB.pdf)
- [International Documentary Association — Errors & Omissions & Rights, Oh My!](https://www.documentary.org/feature/errors-omissions-rights-oh-my-guide-protecting-your-film)

### 4. Quality of the Idea

**Judge question:** Is this a creative, non-obvious application of Google Cloud and the partner service that reflects real problem understanding?

Target evidence:

- The product connects multimodal film understanding to live evidence retrieval, not generic chat.
- Parallel is essential: it turns a visual or audio finding into current, cited external evidence.
- The agent exposes its evidence chain rather than hiding it behind a score.
- The workflow ends with accountable human judgment.
- The narrow three-candidate demo shows depth across visual, factual, and music concerns.

## Internal 9.5/10 bar

- The complete evidence chain succeeds twice consecutively from a clean browser session.
- A judge can explain the user, problem, novelty, and partner value after one viewing.
- Every technical claim in Devpost has a corresponding code path and visible proof.
- The first 90 seconds of the video show the problem, product, and live Parallel moment.
- No unverified claims, real third-party marks, secrets, broken links, or placeholder content remain.
