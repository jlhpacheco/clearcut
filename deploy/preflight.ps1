<#
.SYNOPSIS
  Preflight check script for Cloud Run deployment prerequisites.
  Performs no mutations and no billable builds.
#>
[CmdletBinding()]
param (
    [string]$ProjectId = "clearcut-agentic-20260901",
    [string]$ProjectNumber = "328400425249"
)

$ErrorActionPreference = "Stop"

# 1. Immediate parameter validation
if ($ProjectId -ne "clearcut-agentic-20260901" -or $ProjectNumber -ne "328400425249") {
    Write-Error "Parameter mismatch! ProjectId must be 'clearcut-agentic-20260901' and ProjectNumber must be '328400425249'."
    exit 1
}

$failures = [System.Collections.Generic.List[string]]::new()
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

# 2. Helper to invoke gcloud safely
function Invoke-Gcloud {
    param (
        [string[]]$Arguments,
        [bool]$IsSecret = $false
    )
    $allArgs = $Arguments + "--project=clearcut-agentic-20260901"
    if ($IsSecret) {
        # Never print secret values
        $output = & gcloud @allArgs 2>&1
    } else {
        $output = & gcloud @allArgs 2>&1
    }
    if ($LASTEXITCODE -ne 0) {
        $errStr = ($output -join " ")
        throw "gcloud command failed with exit code ${LASTEXITCODE}: $errStr"
    }
    return $output
}

# 3. Check local files exist
$requiredFiles = @(
    "deploy/agent.Dockerfile",
    "deploy/web.Dockerfile",
    "agent/app/api.py",
    "src/ClearCut.Web/Program.cs"
)

foreach ($relPath in $requiredFiles) {
    $fullPath = Join-Path $repoRoot $relPath
    if (-not (Test-Path $fullPath -PathType Leaf)) {
        $failures.Add("Local file missing: $relPath (resolved: $fullPath)")
    }
}

# 4. Check gcloud CLI and active account
try {
    $null = Get-Command gcloud -ErrorAction Stop
    $authList = Invoke-Gcloud -Arguments @("auth", "list", "--format=json") | ConvertFrom-Json
    $activeAccount = $authList | Where-Object { $_.status -eq "ACTIVE" }
    if (-not $activeAccount) {
        $failures.Add("No active gcloud account found. Run 'gcloud auth login'.")
    }
} catch {
    $failures.Add("gcloud CLI check failed: $_")
}

# 5. Check exact project describe and project number
if ($failures.Count -eq 0) {
    try {
        $projectDesc = Invoke-Gcloud -Arguments @("projects", "describe", "clearcut-agentic-20260901", "--format=json") | ConvertFrom-Json
        if ($projectDesc.projectNumber -ne "328400425249") {
            $failures.Add("Project number mismatch. Expected '328400425249', got '$($projectDesc.projectNumber)'")
        }
    } catch {
        $failures.Add("Failed to describe project: $_")
    }
}

# 6. Check Cloud Storage bucket, object, and Secret Manager metadata
if ($failures.Count -eq 0) {
    try {
        $null = Invoke-Gcloud -Arguments @("storage", "buckets", "describe", "gs://clearcut-agentic-20260901-media-328400425249", "--format=json")
        $null = Invoke-Gcloud -Arguments @("storage", "objects", "describe", "gs://clearcut-agentic-20260901-media-328400425249/clearcut-demo.mp4", "--format=json")
    } catch {
        $failures.Add("GCS metadata check failed: $_")
    }

    try {
        $null = Invoke-Gcloud -Arguments @("secrets", "describe", "parallel-api-key", "--format=json") -IsSecret $true
    } catch {
        $failures.Add("Secret Manager metadata check failed: $_")
    }
}

# 7. Report results
if ($failures.Count -gt 0) {
    Write-Host "PREFLIGHT FAILED:" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
} else {
    Write-Host "PASS: All preflight checks passed. No mutations or billable builds were performed." -ForegroundColor Green
}
