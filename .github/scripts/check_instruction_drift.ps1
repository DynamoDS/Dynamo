<#
Verifies that the build/test commands documented in AGENTS.md and
.github/copilot-instructions.md stay in sync. Both files are hand-maintained
mirrors for different tools; without this check a command change in one
silently goes stale in the other.

What is checked:
- Every dotnet/msbuild command line in AGENTS.md must appear verbatim in
  .github/copilot-instructions.md (and vice versa).
- The set of PublicAPI project names listed in both files must match.

Exits non-zero on drift. Run locally with pwsh or in CI (see
validate_agent_skills.yml).
#>

param(
    [switch]$Report
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$agentsPath = Join-Path $repoRoot "AGENTS.md"
$copilotPath = Join-Path $repoRoot ".github\copilot-instructions.md"

if (-not (Test-Path $agentsPath)) { throw "Missing AGENTS.md at repo root" }
if (-not (Test-Path $copilotPath)) { throw "Missing .github/copilot-instructions.md" }

$agentsContent = Get-Content -Raw -Encoding UTF8 $agentsPath
$copilotContent = Get-Content -Raw -Encoding UTF8 $copilotPath

# Command lines we care about: dotnet/msbuild invocations, normalized for
# trailing whitespace and CRLF so line-ending drift doesn't false-positive.
function Get-CommandLines {
    param([string]$content)

    $lines = $content -split "`r?`n"
    $commands = foreach ($line in $lines) {
        $trimmed = $line.TrimEnd()
        if ($trimmed -match '^\s*(dotnet|msbuild)\s+\S') {
            $trimmed.TrimStart()
        }
    }
    return $commands
}

$agentsCommands = @(Get-CommandLines -content $agentsContent | Sort-Object -Unique)
$copilotCommands = @(Get-CommandLines -content $copilotContent | Sort-Object -Unique)

$driftCount = 0

$missingInCopilot = @($agentsCommands | Where-Object { $copilotCommands -notcontains $_ })
$missingInAgents = @($copilotCommands | Where-Object { $agentsCommands -notcontains $_ })

foreach ($cmd in $missingInCopilot) {
    Write-Host "Command in AGENTS.md but missing from copilot-instructions.md: $cmd"
    $driftCount++
}

foreach ($cmd in $missingInAgents) {
    Write-Host "Command in copilot-instructions.md but missing from AGENTS.md: $cmd"
    $driftCount++
}

# PublicAPI project list parity: both files enumerate which projects carry
# PublicAPI.*.txt files. A new project added to one list but not the other is
# exactly the kind of silent divergence this script exists to catch.
function Get-PublicApiProjects {
    param([string]$content)

    $projects = foreach ($line in ($content -split "`r?`n")) {
        if ($line -match 'PublicAPI\.(Shipped|Unshipped)\.txt') {
            # Collect bare project names mentioned on the line.
            if ($line -match '(DynamoCore|DynamoCoreWpf|DynamoUtilities|NodeServices)') {
                $Matches[1]
            }
        }
    }
    return $projects | Sort-Object -Unique
}

$agentsApiProjects = @(Get-PublicApiProjects -content $agentsContent)
$copilotApiProjects = @(Get-PublicApiProjects -content $copilotContent)

$apiMissingInCopilot = @($agentsApiProjects | Where-Object { $copilotApiProjects -notcontains $_ })
$apiMissingInAgents = @($copilotApiProjects | Where-Object { $agentsApiProjects -notcontains $_ })

foreach ($p in $apiMissingInCopilot) {
    Write-Host "PublicAPI project '$p' listed in AGENTS.md but missing from copilot-instructions.md"
    $driftCount++
}

foreach ($p in $apiMissingInAgents) {
    Write-Host "PublicAPI project '$p' listed in copilot-instructions.md but missing from AGENTS.md"
    $driftCount++
}

if ($Report) {
    Write-Host ""
    Write-Host "Instruction drift report"
    Write-Host "- Commands in AGENTS.md: $($agentsCommands.Count)"
    Write-Host "- Commands in copilot-instructions.md: $($copilotCommands.Count)"
    Write-Host "- Commands missing from copilot-instructions.md: $($missingInCopilot.Count)"
    Write-Host "- Commands missing from AGENTS.md: $($missingInAgents.Count)"
    Write-Host "- PublicAPI projects in AGENTS.md: $($agentsApiProjects.Count)"
    Write-Host "- PublicAPI projects in copilot-instructions.md: $($copilotApiProjects.Count)"
    Write-Host "- Total drift count: $driftCount"

    if ($env:GITHUB_REPOSITORY -and $env:GITHUB_RUN_ID) {
        $summaryContent = @"
## Instruction Drift Report

| Metric | Count |
|--------|-------|
| Commands in AGENTS.md | $($agentsCommands.Count) |
| Commands in copilot-instructions.md | $($copilotCommands.Count) |
| Commands missing from copilot-instructions.md | $($missingInCopilot.Count) |
| Commands missing from AGENTS.md | $($missingInAgents.Count) |
| **Total drift count** | **$driftCount** |

$( if ($driftCount -eq 0) { "✅ AGENTS.md and copilot-instructions.md are in sync" } else { "⚠️ Drift detected - align both files" } )
"@
        $summaryPath = $env:GITHUB_STEP_SUMMARY
        if ($summaryPath) {
            Add-Content -Path $summaryPath -Value $summaryContent -Encoding UTF8
        }
    }
}

if ($driftCount -gt 0) {
    Write-Host ""
    Write-Host "Instruction drift detected: $driftCount difference(s) between AGENTS.md and .github/copilot-instructions.md."
    Write-Host "Align both files so every tool's agent guidance stays identical."
    exit 1
}

Write-Host "✅ AGENTS.md and copilot-instructions.md command sets are in sync."
exit 0
