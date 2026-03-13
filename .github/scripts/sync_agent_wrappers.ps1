<#
Synchronizes and validates generated Copilot agent wrapper files.

Modes:
- default: regenerate mapped wrappers in .github/agents from canonical skills in .agents/skills
- -Check: validate mapped wrappers exist and match generated output, then detect orphan generated wrappers
- -Report: print summary counters; does not change pass/fail behavior
#>

param(
    [switch]$Check,
    [switch]$Report
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$generatedMarker = "AUTO-GENERATED FILE. Do not edit directly."
$githubAgentsDir = Join-Path $repoRoot ".github/agents"

# List of canonical skills to generate wrappers for
$canonicalSkills = @(
    ".agents/skills/dynamo-dotnet-expert/SKILL.md",
    ".agents/skills/dynamo-onboarding/SKILL.md",
    ".agents/skills/dynamo-pr-description/SKILL.md",
    ".agents/skills/dynamo-jira-ticket/SKILL.md",
    ".agents/skills/dynamo-skill-writer/SKILL.md",
    ".agents/skills/dynamo-unit-testing/SKILL.md"
)

function Get-SkillName {
    # Extracts skill name from frontmatter
    param([string]$skillContent)

    $nameMatch = [regex]::Match($skillContent, '(?ms)^---\s*.*?^name:\s*(.+?)\s*$.*?^---\s*')
    if (-not $nameMatch.Success) {
        throw "Could not parse frontmatter name from skill file."
    }

    $raw = $nameMatch.Groups[1].Value.Trim()
    if (($raw.StartsWith('"') -and $raw.EndsWith('"')) -or ($raw.StartsWith("'") -and $raw.EndsWith("'"))) {
        return $raw.Substring(1, $raw.Length - 2)
    }

    return $raw
}

function Get-SkillDescription {
    # Extracts the frontmatter description from a canonical SKILL.md file.
    param([string]$skillContent)

    $match = [regex]::Match($skillContent, '(?ms)^---\s*.*?^description:\s*(.+?)\s*$.*?^---\s*')
    if (-not $match.Success) {
        throw "Could not parse frontmatter description from skill file."
    }

    $raw = $match.Groups[1].Value.Trim()

    if (($raw.StartsWith('"') -and $raw.EndsWith('"')) -or ($raw.StartsWith("'") -and $raw.EndsWith("'"))) {
        return $raw.Substring(1, $raw.Length - 2)
    }

    return $raw
}

function Get-SkillTitle {
    # Extracts title from skill content, or derives from skill name as fallback
    param([string]$skillContent, [string]$skillName)

    # Try to extract title from frontmatter first
    $titleMatch = [regex]::Match($skillContent, '(?ms)^---\s*.*?^title:\s*(.+?)\s*$.*?^---\s*')
    if ($titleMatch.Success) {
        $raw = $titleMatch.Groups[1].Value.Trim()
        if (($raw.StartsWith('"') -and $raw.EndsWith('"')) -or ($raw.StartsWith("'") -and $raw.EndsWith("'"))) {
            return $raw.Substring(1, $raw.Length - 2)
        }
        return $raw
    }

    # Fallback: derive from skill name
    $title = $skillName -replace '-', ' '
    $title = (Get-Culture).TextInfo.ToTitleCase($title)

    return $title
}

function New-WrapperContent {
    # Produces deterministic wrapper file contents for drift-safe generation/checking.
    param(
        [string]$name,
        [string]$description,
        [string]$title,
        [string]$canonicalPath
    )

@"
<!--
AUTO-GENERATED FILE. Do not edit directly.
Canonical source: $canonicalPath
Regenerate with: ./.github/scripts/sync_agent_wrappers.ps1
-->

---
name: $name
description: $description
---

# $title

This is a thin compatibility wrapper for the canonical skill.

Canonical source of truth:
- $canonicalPath

Usage guidance:
- Apply the full instructions from the canonical skill file above.
- If this wrapper and the canonical skill ever differ, the canonical skill wins.

Maintenance note:
- Keep this file lightweight to avoid drift across tools (Copilot/Cursor/Claude).
"@
}

function Resolve-RepoRelativePath {
    param(
        [string]$basePath,
        [string]$targetPath
    )

    $method = [System.IO.Path].GetMethod(
        "GetRelativePath",
        [System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Static,
        $null,
        [Type[]]@([string], [string]),
        $null
    )

    if ($null -ne $method) {
        return [System.IO.Path]::GetRelativePath($basePath, $targetPath)
    }

    $relative = Resolve-Path -Path $targetPath -Relative
    if ($relative.StartsWith(".\\")) {
        return $relative.Substring(2)
    }

    return $relative
}

$driftCount = 0
$missingCount = 0
$contentDriftCount = 0
$orphanCount = 0
$generatedFilesScanned = 0
$expectedWrapperRelativePaths = @{}

foreach ($canonicalPath in $canonicalSkills) {
    # Process each canonical skill: either verify drift (-Check) or regenerate (default mode).
    $skillPath = Join-Path $repoRoot $canonicalPath

    if (-not (Test-Path $skillPath)) {
        throw "Missing canonical skill: $canonicalPath"
    }

    $skillContent = Get-Content -Raw -Encoding UTF8 $skillPath
    $skillName = Get-SkillName -skillContent $skillContent
    $wrapperRelativePath = ".github/agents/$skillName.agent.md"
    $wrapperFullPath = Join-Path $repoRoot $wrapperRelativePath

    # Build expected wrapper paths for orphan detection during check mode
    if ($Check) {
        $normalizedWrapperPath = $wrapperRelativePath.Replace("\", "/")
        $expectedWrapperRelativePaths[$normalizedWrapperPath] = $true
    }

    $description = Get-SkillDescription -skillContent $skillContent
    $title = Get-SkillTitle -skillContent $skillContent -skillName $skillName

    $expected = New-WrapperContent -name $skillName -description $description -title $title -canonicalPath $canonicalPath

    if ($Check) {
        if (-not (Test-Path $wrapperFullPath)) {
            Write-Host "Missing wrapper: $wrapperRelativePath"
            $driftCount++
            $missingCount++
            continue
        }

        $actual = Get-Content -Raw -Encoding UTF8 $wrapperFullPath
        if ($actual -ne $expected) {
            Write-Host "Wrapper drift detected: $wrapperRelativePath"
            $driftCount++
            $contentDriftCount++
        }
    }
    else {
        $wrapperDir = Split-Path -Parent $wrapperFullPath
        if (-not (Test-Path $wrapperDir)) {
            New-Item -ItemType Directory -Path $wrapperDir | Out-Null
        }

        [System.IO.File]::WriteAllText(
            $wrapperFullPath,
            $expected,
            [System.Text.UTF8Encoding]::new($false, $true)
        )
        Write-Host "Wrote wrapper: $wrapperRelativePath"
    }
}

if ($Check) {
    # In check mode, also detect orphan generated wrappers that are no longer mapped.
    if (Test-Path $githubAgentsDir) {
        $agentFiles = Get-ChildItem -Path $githubAgentsDir -File
        foreach ($agentFile in $agentFiles) {
            # Compute a path relative to the repository root, not the current working directory.
            $relativePath = Resolve-RepoRelativePath -basePath $repoRoot -targetPath $agentFile.FullName
            $relativePath = $relativePath.Replace("\", "/")
            $content = Get-Content -Raw -Encoding UTF8 $agentFile.FullName

            $isGenerated = $content.Contains($generatedMarker)
            if ($isGenerated) {
                $generatedFilesScanned++
            }

            if ($isGenerated -and -not $expectedWrapperRelativePaths.ContainsKey($relativePath)) {
                Write-Host "Orphan generated wrapper detected: $relativePath"
                $driftCount++
                $orphanCount++
            }
        }
    }
}

if ($Report) {
    # Optional operator-friendly diagnostics for local runs and CI troubleshooting.
    Write-Host ""
    Write-Host "Agent wrapper report"
    Write-Host "- Mapped wrappers: $($canonicalSkills.Count)"
    Write-Host "- Missing mapped wrappers: $missingCount"
    Write-Host "- Mapped content drift: $contentDriftCount"
    Write-Host "- Generated wrappers scanned: $generatedFilesScanned"
    Write-Host "- Orphan generated wrappers: $orphanCount"
    Write-Host "- Total drift count: $driftCount"

    # Write GitHub Action summary if running in GitHub Actions environment
    if ($env:GITHUB_REPOSITORY -and $env:GITHUB_RUN_ID) {
        $summaryContent = @"
## Agent Wrapper Sync Report

| Metric | Count |
|--------|-------|
| Mapped wrappers | $($canonicalSkills.Count) |
| Missing mapped wrappers | $missingCount |
| Mapped content drift | $contentDriftCount |
| Generated wrappers scanned | $generatedFilesScanned |
| Orphan generated wrappers | $orphanCount |
| **Total drift count** | **$driftCount** |

$( if ($driftCount -eq 0) { "✅ All wrapper files are synchronized" } else { "⚠️ Drift detected - run sync script to update wrappers" } )
"@
        Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value $summaryContent
    }
}

if ($Check -and $driftCount -gt 0) {
    # Non-zero drift count must fail checks for CI enforcement.
    throw "Agent wrapper check failed. Drift count: $driftCount"
}

if ($Check) {
    Write-Host "Agent wrapper check passed."
}
