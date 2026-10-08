# Check File Signatures
#
# Verifies that the upstream-signed binaries in an extracted Dynamo release zip really do carry an
# Authenticode signature from "Autodesk, Inc.".
#
# Background - Dynamo 4.1.2:
# 4.1.2 shipped unsigned DLLs into Civil 3D. C3D QA caught it AFTER publication. Nothing in the
# Dynamo build inspected Authenticode signatures at all, so there was no stage at which the
# regression could have been noticed on our side.
#
# Why this is a separate script from check_file_version.ps1:
# check_file_version.ps1 is structurally blind to exactly the files that broke. It EXCLUDES
# "Autodesk*.dll" (line 25), "LibG*.dll" (line 41), "ProtoGeometry*.dll" (line 57) and the
# per-package "*.resources.dll" entries, because those binaries are versioned upstream instead of
# being stamped with the DynamoSandbox.exe version. That exclusion list and this script's inclusion
# list are two views of one set: a binary that does not carry Dynamo's own version is a binary that
# arrived pre-built from somewhere else, and therefore one that must ALREADY be signed by the time
# it lands in the build output. Version identity and signature identity are the two halves of
# "where did this file come from", and we were only ever checking one of them.
#
# Why an allow-list, and not "everything must be signed":
# Dynamo's own assemblies are deliberately unsigned in a GitHub Actions build - Garasign signing
# happens later, at the installer step. Measured against a real bin\AnyCPU\Release tree, 645 of 859
# dll/exe files are unsigned. A naive "all must be signed" gate would fail 100% of builds, would be
# switched off within a week, and would leave us exactly where 4.1.2 left us.
#
# How the allow-list below was derived (measured, not guessed):
# Every .dll/.exe in a full bin\AnyCPU\Release tree was enumerated with Get-AuthenticodeSignature
# and grouped by signer. Exactly 90 files are signed by "Autodesk, Inc."; the patterns below match
# 92 files, of which 90 are signed and 2 are the documented exceptions in $excludedFiles. The list
# is therefore a complete, empirically verified cover of the Autodesk-signed set rather than a
# subjective selection - so any file that drops out of it is a real regression.
#
# Note on *.resources.dll - it cannot be a blanket "**/*.resources.dll" rule:
# 493 of the 601 *.resources.dll files in the build output are Dynamo's own satellite assemblies,
# and they are legitimately unsigned at this stage. Only the satellites of upstream packages
# (LibG.ProtoInterface, ProtoGeometry, TuneUp, DSPythonNet3) are signed, and each is already
# covered by its package's prefix below.
#
# Usage:
#   pwsh -NoProfile -File .\check_file_signatures.ps1 <extracted-release-zip-dir>
#   pwsh -NoProfile -File .\check_file_signatures.ps1 <dir> -Patterns 'LibG*.dll'
#   pwsh -NoProfile -File .\check_file_signatures.ps1 <dir> -AllowUnverifiedChain
#
# Runs standalone: no credentials, no network.
#
# Every matched file must evaluate to Valid. Matching the signer's common name is not enough on
# its own: anyone can mint a self-signed certificate with CN "Autodesk, Inc.", and a file signed
# with one reports the right name, a correct hash, and a status of UnknownError ("terminated in a
# root certificate which is not trusted") - not NotTrusted. Only the chain evaluation tells a real
# Autodesk signature from a forged one, so a non-Valid status is a failure by default.
#
# -AllowUnverifiedChain downgrades that to a warning, for a machine whose certificate store is
# known to be incomplete. It is an explicit, visible opt-out; do not use it on a release gate.
#
# https://learn.microsoft.com/en-us/dotnet/api/system.management.automation.signaturestatus
[CmdletBinding()]
param (
    # Directory containing an extracted Dynamo release zip (or a bin\AnyCPU\Release build output).
    [Parameter(Mandatory = $true, Position = 0)][string]$path,

    # Override the allow-list entirely. Intended for narrowing a manual run to a single package.
    [Parameter(Mandatory = $false)][string[]]$Patterns,

    # Additional leaf-name patterns to exclude, on top of the documented $excludedFiles below.
    [Parameter(Mandatory = $false)][string[]]$ExcludePatterns = @(),

    # The certificate common name every matched file must be signed with.
    [Parameter(Mandatory = $false)][string]$ExpectedSigner = 'Autodesk, Inc.',

    # Downgrade "signed by the expected name, but the chain did not evaluate to Valid" from a
    # failure to a warning. Only for a machine with a known-incomplete certificate store - with it
    # set, a binary signed by a forged "Autodesk, Inc." certificate passes.
    [Parameter(Mandatory = $false)][switch]$AllowUnverifiedChain
)

$ErrorActionPreference = "Stop"

# Files that MUST carry an "Autodesk, Inc." Authenticode signature in the build output.
# Grouped by where the binary comes from. Every entry is backed by a measured, currently-signed
# file; do not add speculative patterns, and do not add anything Dynamo itself compiles.
$includedFiles = @(
    # Geometry library - the LibG package. 27 files, incl. LibG.ProtoInterface.resources.dll.
    "LibG*.dll",
    # ProtoGeometry ships alongside LibG and is versioned with it, not with Dynamo.
    "ProtoGeometry*.dll",
    # Autodesk.IDSDK, Autodesk.GeometryPrimitives.Dynamo, AutodeskAssistantViewExtension.
    "Autodesk*.dll",
    "AdskIdentitySDK.dll",
    # Analytics / ADP.
    "AdpSDKCSharpWrapper.dll",
    "Analytics.NET.*.dll",
    # Units / Forge.
    "ForgeUnitsManaged.dll",
    "Units.dll",
    # Python built-in package. Covers DSPythonNet3.resources.dll.
    # "Python.*.dll" is deliberately dot-qualified: a bare "Python*" would also swallow Dynamo's
    # own unsigned PythonNodeModels* assemblies.
    "DSPythonNet3*.dll",
    "Python.*.dll",
    # DynamoPlayer built-in package.
    "DynamoPlayer.*.dll",
    # DynamoMCP built-in package (DYN-10553).
    "MCPExtension.dll",
    "MCPServer.dll",
    "ModelContextProtocol*.dll",
    "JsonSchema.Net.dll",
    "JsonPointer.Net.dll",
    "Json.More.dll",
    # TuneUp built-in package. Covers TuneUp.resources.dll.
    "TuneUp*.dll",
    # glTF export / imaging.
    "SharpGLTF.*.dll",
    "BigGustave.dll"
)

# This list is a snapshot of what is signed TODAY, and it is scheduled to grow.
# DYN-10749 (Critical, 4.3) tracks ~43 third-party binaries that Civil 3D's install team requires
# signed ahead of the stricter Autodesk 2028/Global Launch requirements - HelixToolkit*, SharpDX*,
# Lucene.Net*, LiveChartsCore*, StarMath, RestSharp, ICSharpCode.AvalonEdit, Player\ffmpeg.dll and
# others. They are deliberately absent here because they are unsigned today and this gate must
# reflect reality rather than intent; adding them now would fail every build.
# When DYN-10749 lands, add them here in the same commit - otherwise they become signed and
# silently unchecked, which is the same blind spot in a new place.
# Note two of them need care: libg_231_0_0\libiconv.dll and libg_231_0_0\libintl.dll live inside
# the LibG folders but are NOT matched by "LibG*.dll", so they need their own entries.

# Known-unsigned files that match a pattern above. Each is a real gap rather than a false alarm,
# but neither is something this gate can fix, so failing on them would only teach the operator to
# ignore the gate. Keep this list short and dated, and re-check it whenever it changes.
$excludedFiles = @(
    # Generated by Dynamo's own build from ProtoGeometry_DynamoCustomization.xml - Dynamo output
    # that merely inherits the ProtoGeometry prefix, not an upstream binary.
    "ProtoGeometry.customization.dll",
    # AutodeskAssistantViewExtension.dll IS signed by its own pipeline, but its satellite assembly
    # is not. Verified 2026-09-21. Worth raising with the AutodeskAssistant pipeline owners - it is
    # a gap in that pipeline, not something this repo can correct.
    "AutodeskAssistantViewExtension.resources.dll"
)

if ($Patterns) { $includedFiles = $Patterns }
$excludedFiles += $ExcludePatterns

if (-not (Test-Path -LiteralPath $path -PathType Container)) {
    Write-Output "::error title=Signature check::The path '$path' does not exist or is not a directory. The signature gate could not run, so it is failing rather than passing silently."
    exit 1
}

$resolvedPath = (Resolve-Path -LiteralPath $path).Path
Write-Output "::notice::Checking Authenticode signatures under $resolvedPath (expected signer: $ExpectedSigner)"

$files = @(Get-ChildItem -Path $resolvedPath -Include $includedFiles -Exclude $excludedFiles -Recurse -File |
    Sort-Object -Property FullName -Unique)

# A path typo, a wrong working directory, or a zip that extracted one level deeper than expected
# all present as "nothing matched". Passing here would turn any of them into a false green - the
# single most dangerous outcome for a gate whose entire job is to notice an absence.
if ($files.Count -eq 0) {
    $message = @(
        "No files matched the upstream-signed allow-list under '$resolvedPath'.",
        "This is treated as a FAILURE, not a pass: an empty match almost always means the path is",
        "wrong rather than that a release genuinely contains no Autodesk-signed binaries.",
        "Patterns searched: $($includedFiles -join ', ')"
    ) -join "`n"
    Write-Host "`n$message"
    Write-Output "::error title=Signature check found no files::$($message -replace "`n", '%0A')"
    exit 1
}

<#
.SYNOPSIS
    Returns the common name of an Authenticode signer certificate.
.DESCRIPTION
    Uses X509Certificate2.GetNameInfo rather than parsing Subject, because the Autodesk subject RDN
    is quoted ('CN="Autodesk, Inc."'). Naive comma splitting or a regex over Subject yields a
    truncated 'Autodesk' that would happily match an impostor such as 'Autodesk Fake Ltd'.
.PARAMETER Certificate
    The signer certificate from Get-AuthenticodeSignature, or $null for an unsigned file.
.OUTPUTS
    System.String - the certificate common name, or $null when there is no certificate.
#>
function Get-SignerCommonName {
    [CmdletBinding()]
    [OutputType([string])]
    param (
        [Parameter(Mandatory = $true)][AllowNull()]$Certificate
    )

    if ($null -eq $Certificate) { return $null }
    return $Certificate.GetNameInfo([System.Security.Cryptography.X509Certificates.X509NameType]::SimpleName, $false)
}

$failures = @()
$warnings = @()
$passed = 0

foreach ($file in $files) {
    $name = $file.Name
    $signature = Get-AuthenticodeSignature -LiteralPath $file.FullName
    $status = $signature.Status
    $signer = Get-SignerCommonName -Certificate $signature.SignerCertificate
    $signerText = if ([string]::IsNullOrWhiteSpace($signer)) { '<unsigned>' } else { $signer }

    $reason = $null
    $warned = $false

    if ($status -eq 'NotSigned') {
        $reason = "not signed"
    }
    elseif ($signer -ne $ExpectedSigner) {
        $reason = "signed by '$signerText', expected '$ExpectedSigner'"
    }
    elseif ($status -eq 'HashMismatch') {
        # The signature does not match the file contents. Always fatal - this is tampering or
        # corruption, never a benign local trust-store gap.
        $reason = "hash mismatch - the file does not match its signature"
    }
    elseif ($status -ne 'Valid') {
        # The right name, but the chain did not evaluate to Valid. This is what a forged
        # certificate looks like (UnknownError: untrusted root), so it fails unless the caller has
        # explicitly declared this machine's trust store unreliable.
        if ($AllowUnverifiedChain) {
            $warned = $true
            $warnings += [PSCustomObject]@{ File = $file; Status = $status; Signer = $signerText; Message = $signature.StatusMessage }
        }
        else {
            $reason = "signature status is '$status', not Valid - the name matches but the chain does not verify ($($signature.StatusMessage))"
        }
    }

    if ($reason) {
        Write-Host "❌ $name - $status - $signerText"
        $failures += [PSCustomObject]@{ File = $file; Status = $status; Signer = $signerText; Reason = $reason }
    }
    elseif ($warned) {
        Write-Host "⚠️ $name - $status - $signerText"
        $passed++
    }
    else {
        Write-Host "✅ $name - $status - $signerText"
        $passed++
    }
}

Write-Host "`n`e[4mSignature check summary`e[24m"
Write-Host "  Path:          $resolvedPath"
Write-Host "  Files checked: $($files.Count)"
Write-Host "  Passed:        $passed"
Write-Host "  Failed:        $($failures.Count)"
Write-Host "  Warnings:      $($warnings.Count)"
Write-Host "  Expected signer: $ExpectedSigner"
Write-Host "  Exclusions:    $($excludedFiles -join ', ')"

if ($warnings.Count -gt 0) {
    Write-Host "`n`e[4mThe following file(s) are signed by $ExpectedSigner but did not evaluate to Valid`e[24m"
    $title = "Signature did not evaluate to Valid"
    foreach ($warning in $warnings) {
        $message = "$($warning.File.Name) - $($warning.File.FullName) - $($warning.Status) - $($warning.Message)"
        Write-Output "::warning title=$title::$message"
    }
}

if ($failures.Count -gt 0) {
    Write-Host "`n`e[4mThe following file(s) failed the signature check`e[24m"
    $title = "Unsigned or mis-signed file"
    foreach ($failure in $failures) {
        Write-Host "  $($failure.File.Name)"
        Write-Host "    Path:   $($failure.File.FullName)"
        Write-Host "    Status: $($failure.Status)"
        Write-Host "    Signer: $($failure.Signer)"
        Write-Host "    Reason: $($failure.Reason)"
        $message = "$($failure.File.Name) - $($failure.File.FullName) - status: $($failure.Status) - signer: $($failure.Signer) - $($failure.Reason)"
        Write-Output "::error title=$title::$message"
    }

    $summary = @(
        "$($failures.Count) of $($files.Count) upstream binaries are not correctly signed ($ExpectedSigner).",
        "These files are expected to arrive already signed from their upstream package - Dynamo's own",
        "Garasign signing at the installer step will NOT cover them. Shipping in this state is the",
        "Dynamo 4.1.2 defect that reached Civil 3D and was only caught by C3D QA after publication."
    ) -join "`n"
    Write-Host "`n$summary"
    exit 1
}

Write-Output "::notice::All $($files.Count) upstream binaries are correctly signed ($ExpectedSigner)."
exit 0
