<#
Verifies that the build/test commands documented in AGENTS.md and
.github/copilot-instructions.md stay in sync. Both files are hand-maintained
mirrors for different tools; without this check a command change in one
silently goes stale in the other.

What is checked:
- Every dotnet/msbuild command line in AGENTS.md must appear verbatim in
  .github/copilot-instructions.md (and vice versa).
- The set of PublicAPI project names listed in both files must match.
- Neither file may exceed 250 lines ($maxInstructionLines), counted like
  `wc -l`. Both are loaded into agent context at session start, so the
  budget keeps explicit headroom; move detail into AGENTS.md sections or
  area READMEs instead of growing either file.

Exits non-zero on drift or when a file is over the line budget. Run locally
with pwsh or in CI (see validate_agent_skills.yml).
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

# Line budget for each instruction file. Single source for the limit used in
# the check, the -Report output and the step summary.
$maxInstructionLines = 250

# The authoritative set of projects carrying PublicAPI.*.txt files, derived
# from the tree so a newly added project is recognized without editing this
# script.
$knownApiProjects = @(
    Get-ChildItem -Path (Join-Path $repoRoot "src") -Directory |
        Where-Object { Test-Path (Join-Path $_.FullName "PublicAPI.Unshipped.txt") } |
        ForEach-Object { $_.Name }
)

# Command lines we care about: dotnet/msbuild invocations in fenced code
# blocks and Markdown checklists, normalized for trailing whitespace and
# CRLF so line-ending drift doesn't false-positive. Checklist entries wrap
# the command in a code span ("- [ ] `dotnet ...`"), so strip the checkbox
# marker and opening backtick, and stop the capture at the closing backtick.
function Get-CommandLines {
    param([string]$content)

    $lines = $content -split "`r?`n"
    $commands = foreach ($line in $lines) {
        $trimmed = $line.TrimEnd()
        if ($trimmed -match '^\s*(?:- \[[ x]\] )?`?(dotnet|msbuild)\s+([^`]*)') {
            ($Matches[1] + ' ' + $Matches[2]).Trim()
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
# The authoritative project set is the tree itself (src/*/PublicAPI.*.txt),
# so derive it from disk rather than a hardcoded name list, then collect the
# projects each instruction file mentions — from a parenthesized list
# "(DynamoCore, DynamoCoreWpf, ...)", a bare list on the line, or a path
# mention like src/DynamoCore/PublicAPI.Unshipped.txt.
function Get-PublicApiProjects {
    param(
        [string]$content,
        [string[]]$knownProjects
    )

    $projects = @()
    foreach ($line in ($content -split "`r?`n")) {
        if ($line -notmatch 'PublicAPI') { continue }

        # Parenthesized list following a PublicAPI mention:
        # "...PublicAPI.{Shipped,Unshipped}.txt (DynamoCore, DynamoCoreWpf, ...)"
        if ($line -match 'PublicAPI[^)]*\(([^)]+)\)') {
            foreach ($p in ($Matches[1] -split ',')) {
                $n = $p.Trim()
                if ($knownProjects -contains $n) { $projects += $n }
            }
        }

        # Path mentions: src/<Project>/PublicAPI.*.txt
        foreach ($m in [regex]::Matches($line, 'src/(\w+)/PublicAPI')) {
            $n = $m.Groups[1].Value
            if ($knownProjects -contains $n) { $projects += $n }
        }

        # Bare list on a PublicAPI line: "Existing PublicAPI files: A, B, C"
        if ($line -match 'PublicAPI[^:]*:\s*([A-Z]\w*(,\s*[A-Z]\w*)+)') {
            foreach ($p in ($Matches[1] -split ',')) {
                $n = $p.Trim()
                if ($knownProjects -contains $n) { $projects += $n }
            }
        }
    }
    return $projects | Sort-Object -Unique
}

$agentsApiProjects = @(Get-PublicApiProjects -content $agentsContent -knownProjects $knownApiProjects)
$copilotApiProjects = @(Get-PublicApiProjects -content $copilotContent -knownProjects $knownApiProjects)

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

# Line budget: count lines the way `wc -l` does for a file that ends in a
# newline: split on CRLF/LF, ignoring one trailing newline.
function Get-LineCount {
    param([string]$content)

    if ([string]::IsNullOrEmpty($content)) { return 0 }
    $body = $content -replace '\r?\n\z', ''
    return @($body -split "`r?`n").Count
}

$agentsLineCount = Get-LineCount -content $agentsContent
$copilotLineCount = Get-LineCount -content $copilotContent

$lineCounts = @(
    [pscustomobject]@{ Name = 'AGENTS.md'; Lines = $agentsLineCount }
    [pscustomobject]@{ Name = '.github/copilot-instructions.md'; Lines = $copilotLineCount }
)

$overBudget = @($lineCounts | Where-Object { $_.Lines -gt $maxInstructionLines })

foreach ($file in $overBudget) {
    Write-Host "$($file.Name) is $($file.Lines) lines, over the $maxInstructionLines-line budget by $($file.Lines - $maxInstructionLines)."
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
    Write-Host "- AGENTS.md lines: $agentsLineCount of $maxInstructionLines ($($maxInstructionLines - $agentsLineCount) headroom)"
    Write-Host "- copilot-instructions.md lines: $copilotLineCount of $maxInstructionLines ($($maxInstructionLines - $copilotLineCount) headroom)"
    Write-Host "- Files over line budget: $($overBudget.Count)"
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
| AGENTS.md lines (budget $maxInstructionLines) | $agentsLineCount |
| copilot-instructions.md lines (budget $maxInstructionLines) | $copilotLineCount |
| Files over line budget | $($overBudget.Count) |
| **Total drift count** | **$driftCount** |

$( if ($driftCount -eq 0) { "✅ AGENTS.md and copilot-instructions.md are in sync" } else { "⚠️ Drift detected - align both files" } )
$( if ($overBudget.Count -eq 0) { "✅ Both files are within the $maxInstructionLines-line budget" } else { "⚠️ Line budget exceeded - move detail out of the over-budget file" } )
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
}

if ($overBudget.Count -gt 0) {
    Write-Host ""
    Write-Host "Line budget exceeded: $($overBudget.Count) instruction file(s) over $maxInstructionLines lines."
    Write-Host "Move detail into AGENTS.md sections or area READMEs and link to it."
}

if ($driftCount -gt 0 -or $overBudget.Count -gt 0) {
    exit 1
}

Write-Host "✅ AGENTS.md and copilot-instructions.md command sets are in sync and within the $maxInstructionLines-line budget."
exit 0
