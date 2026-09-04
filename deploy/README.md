# ClearCut Deployment Preflight

This directory contains the preflight validation script to verify deployment prerequisites before any infrastructure is provisioned or code is built.

## Preflight Command
Run the following command to execute the read-only preflight checks:
```powershell
pwsh -NoProfile -File ./deploy/preflight.ps1
```

## Preflight Characteristics
- **Read-Only**: Performs no mutations, creates no resources, and triggers no billable builds.
- **Target Scope**: Guards exact project `clearcut-agentic-20260901` (Project Number: `328400425249`).
- **Current Blocker**: Missing `gs://clearcut-agentic-20260901-media-328400425249/clearcut-demo.mp4`. No Cloud Run or Artifact Registry resources should be created until this preflight check passes.

## Infrastructure & Security Design
- **Media Bucket**: The dedicated media bucket `gs://clearcut-agentic-20260901-media-328400425249` enforces uniform bucket-level access and public-access-prevention.
- **Planned Production Design**: Public web service, private agent service, OIDC service identity, and autoscaling limits set to min 0 / max 1.
- **Cost Controls**: Google Cloud budgets can alert but cannot enforce a hard spending cap.

*Note: No deployment currently exists.*
