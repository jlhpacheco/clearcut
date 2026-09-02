# Devpost Draft

This is a working draft. Final answers must describe only verified functionality.

## Project

- **Name:** ClearCut
- **Tagline:** Turn a rough cut into timestamped, cited clearance evidence before legal review.
- **Submitter type:** Individual
- **Organization:** N/A
- **Government employee:** No
- **Country of residence:** United States
- **Canada province:** N/A
- **Project status before July 27, 2026:** New
- **Partner track:** Parallel
- **Team size:** 1
- **Repository:** https://github.com/jlhpacheco/clearcut
- **Hosted project:** Pending Cloud Run deployment
- **Demo video:** Pending public YouTube upload
- **First time using Parallel:** Yes
- **IBM / Grafana / Clickhouse / Replit first-time questions:** Use each provided N/A option

## Built with — planned, verify before submission

C#, ASP.NET Core, Blazor, Gemini, Google Cloud Agent Builder, Google ADK, Parallel Search, Google Cloud Run, Cloud Storage, and Secret Manager.

## Draft description

ClearCut is a clearance-preparation agent for independent filmmakers and small production teams approaching final cut or distribution. It turns a short rough cut into a timestamped, cited review package so a producer can resolve obvious concerns before attorney, distributor, or errors-and-omissions review.

Independent filmmakers routinely need to identify visible marks, factual claims, music, and other material that may require follow-up. The work is detailed, fragmented, and expensive to begin late. ClearCut helps organize the evidence; it does not make legal conclusions or replace qualified counsel.

The demonstration uses one original 30–45 second fictional commercial. Gemini identifies three planted review candidates at exact timestamps: a fictional brand mark, a factual claim, and an original music cue. The user selects a finding, and a Google Cloud Agent Builder/ADK agent turns it into a focused research task. Parallel Search runs live and returns source-backed evidence. The user then records a human decision: dismiss, investigate, replace, or license.

The key moment exposes the complete evidence chain: timestamped finding → Gemini research task → visible Parallel Search → cited evidence card → human decision.

The submission uses newly created fictional media and competition-permitted tooling. ClearCut presents research assistance and evidence organization—not legal advice.
