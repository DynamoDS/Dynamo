<#
Synchronizes and validates generated Copilot agent wrapper files.

Modes:
- default: regenerate mapped wrappers in .github/agents from canonical skills in .agents/skills
- -Check: validate mapped wrappers exist and match generated output, then detect orphan generated wrappers
- -VerboseReport: print summary counters; does not change pass/fail behavior
#>

param(
    [switch]$Check,
    [switch]$VerboseReport
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$generatedMarker = "AUTO-GENERATED FILE. Do not edit directly."

$wrapperMap = @(
    # Keep this map as the explicit contract between canonical skills and generated wrappers.
    @{
        CanonicalSkillPath = ".agents/skills/dynamo-dotnet-expert/SKILL.md"
        WrapperPath = ".github/agents/Dynamo Dotnet Expert.md"
        WrapperName = "Dynamo C#/Dotnet Expert"
        WrapperTitle = "Dynamo .NET Expert"
    },
    @{
        CanonicalSkillPath = ".agents/skills/dynamo-onboarding/SKILL.md"
        WrapperPath = ".github/agents/Dynamo Onboarding.md"
        WrapperName = "Dynamo Onboarding"
        WrapperTitle = "Dynamo Onboarding"
    },
    @{
        CanonicalSkillPath = ".agents/skills/dynamo-pr-description/SKILL.md"
        WrapperPath = ".github/agents/Dynamo PR Description.md"
        WrapperName = "Dynamo PR Description"
        WrapperTitle = "Dynamo PR Description"
    },
    @{
        CanonicalSkillPath = ".agents/skills/dynamo-jira-ticket/SKILL.md"
        WrapperPath = ".github/agents/Dynamo Jira Ticket.md"
        WrapperName = "Dynamo Jira Ticket"
        WrapperTitle = "Dynamo Jira Ticket"
    },
    @{
        CanonicalSkillPath = ".agents/skills/dynamo-skill-writer/SKILL.md"
        WrapperPath = ".github/agents/Dynamo Skill Writer.md"
        WrapperName = "Dynamo Skill Writer"
        WrapperTitle = "Dynamo Skill Writer"
    }
)

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
Regenerate with: ./tools/agents/sync-agent-wrappers.ps1
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
$expectedWrapperRelativePaths = @{}
$missingCount = 0
$contentDriftCount = 0
$orphanCount = 0
$generatedFilesScanned = 0

foreach ($mapping in $wrapperMap) {
    # Normalize map keys for robust path comparisons on Windows and CI.
    $normalizedWrapperPath = $mapping.WrapperPath.Replace("\", "/")
    $expectedWrapperRelativePaths[$normalizedWrapperPath] = $true
}

foreach ($mapping in $wrapperMap) {
    # Process each mapped wrapper: either verify drift (-Check) or regenerate (default mode).
    $skillPath = Join-Path $repoRoot $mapping.CanonicalSkillPath
    $wrapperPath = Join-Path $repoRoot $mapping.WrapperPath

    if (-not (Test-Path $skillPath)) {
        throw "Missing canonical skill: $($mapping.CanonicalSkillPath)"
    }

    $skillContent = Get-Content -Raw -Encoding UTF8 $skillPath
    $description = Get-SkillDescription -skillContent $skillContent

    $expected = New-WrapperContent -name $mapping.WrapperName -description $description -title $mapping.WrapperTitle -canonicalPath $mapping.CanonicalSkillPath

    if ($Check) {
        if (-not (Test-Path $wrapperPath)) {
            Write-Host "Missing wrapper: $($mapping.WrapperPath)"
            $driftCount++
            $missingCount++
            continue
        }

        $actual = Get-Content -Raw -Encoding UTF8 $wrapperPath
        if ($actual -ne $expected) {
            Write-Host "Wrapper drift detected: $($mapping.WrapperPath)"
            $driftCount++
            $contentDriftCount++
        }
    }
    else {
        $wrapperDir = Split-Path -Parent $wrapperPath
        if (-not (Test-Path $wrapperDir)) {
            New-Item -ItemType Directory -Path $wrapperDir | Out-Null
        }

        [System.IO.File]::WriteAllText(
            $wrapperPath,
            $expected,
            [System.Text.UTF8Encoding]::new($false, $true)
        )
        Write-Host "Wrote wrapper: $($mapping.WrapperPath)"
    }
}

if ($Check) {
    # In check mode, also detect orphan generated wrappers that are no longer mapped.
    $agentsDir = Join-Path $repoRoot ".github/agents"
    if (Test-Path $agentsDir) {
        $agentFiles = Get-ChildItem -Path $agentsDir -File
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

if ($VerboseReport) {
    # Optional operator-friendly diagnostics for local runs and CI troubleshooting.
    Write-Host ""
    Write-Host "Agent wrapper report"
    Write-Host "- Mapped wrappers: $($wrapperMap.Count)"
    Write-Host "- Missing mapped wrappers: $missingCount"
    Write-Host "- Mapped content drift: $contentDriftCount"
    Write-Host "- Generated wrappers scanned: $generatedFilesScanned"
    Write-Host "- Orphan generated wrappers: $orphanCount"
    Write-Host "- Total drift count: $driftCount"
}

if ($Check -and $driftCount -gt 0) {
    # Non-zero drift count must fail checks for CI enforcement.
    throw "Agent wrapper check failed. Drift count: $driftCount"
}

if ($Check) {
    Write-Host "Agent wrapper check passed."
}
