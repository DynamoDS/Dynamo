# Check IDSDK Exports
#
# Guards against shipping an AdskIdentitySDK.dll that does not export the IDSDK MCP
# token-validation API used by Dynamo's Autodesk Assistant / MCP integration.
#
# Background - DYN-10773:
# Dynamo 4.2.0 shipped the Autodesk.IDSDK 1.2.6 package, whose native AdskIdentitySDK.dll
# (file version 1.16.5.1) does NOT export idsdk_mcp_validate_token. The MCP entry points only
# landed in IDSDK 1.17.0. Every MCP token-validation call therefore failed and Autodesk
# Assistant was 100% broken in the shipped release. Nothing in the build or tests caught it.
#
# Why this check reads the export table and not the file version:
# The broken DLL has a HIGHER file version than a known-good one, so a version comparison is
# actively misleading:
#
#   Autodesk.IDSDK 1.2.5           AdskIdentitySDK.dll 1.15.3.5   exports MCP API: no
#   Autodesk.IDSDK 1.2.6 (broken)  AdskIdentitySDK.dll 1.16.5.1   exports MCP API: no
#   Revit 2027.3                   AdskIdentitySDK.dll 1.16.4.7   exports MCP API: YES
#   Autodesk.IDSDK 1.2.9 (fixed)   AdskIdentitySDK.dll 1.18.2.1   exports MCP API: YES
#
# AdskIdentitySDK.dll is additionally exempt from .github/scripts/check_file_version.ps1, so
# nothing else in CI inspects it at all. The PE export directory is the only reliable signal.
#
# The PE export directory is parsed directly in PowerShell. dumpbin is deliberately NOT used:
# it ships with the Visual Studio C++ workload and is not guaranteed to exist on CI agents.
#
# Usage:
#   .\check_idsdk_exports.ps1                             # auto-discover (build output, then NuGet cache)
#   .\check_idsdk_exports.ps1 <dir>                       # search a build-output directory
#   .\check_idsdk_exports.ps1 -DllPath <file>             # check one specific DLL
#   .\check_idsdk_exports.ps1 -DllPath <file> -RequiredExports a,b
#
# PE format reference: https://learn.microsoft.com/en-us/windows/win32/debug/pe-format

[CmdletBinding()]
param (
    # Directory to search recursively for AdskIdentitySDK.dll (typically a build output folder).
    [Parameter(Mandatory = $false, Position = 0)][string]$Path,

    # Explicit path to a single AdskIdentitySDK.dll. Takes precedence over -Path.
    [Parameter(Mandatory = $false)][string]$DllPath,

    # Exported symbols that must be present. Defaults to the MCP token-validation entry point.
    [Parameter(Mandatory = $false)][string[]]$RequiredExports = @('idsdk_mcp_validate_token')
)

$ErrorActionPreference = "Stop"

$DllName = 'AdskIdentitySDK.dll'

# $IsWindows only exists in PowerShell 6+; on Windows PowerShell 5.1 its absence implies Windows.
$onWindows = if ($null -eq $IsWindows) { $true } else { $IsWindows }

# AdskIdentitySDK.dll is a Windows-only native binary and is not produced by the Linux/macOS
# DynamoCore.sln build. Skipping (rather than failing) keeps this script safe to call from a
# cross-platform job; the Windows-only Build DynamoAll.sln job is what enforces the guard.
if (-not $onWindows) {
    Write-Output "::notice::$DllName is Windows-only - skipping IDSDK export check on this platform."
    exit 0
}

<#
.SYNOPSIS
    Returns the names exported by a native PE image by parsing its export directory.
.DESCRIPTION
    Walks the DOS header, PE/COFF header, optional header data directories and section table to
    locate the export directory, then reads the exported-name table. Supports both PE32 and PE32+.
    Returns an empty array for an image with no export directory.
.PARAMETER ImagePath
    Full path to the native PE image to inspect.
.OUTPUTS
    System.String[] - the exported symbol names.
#>
function Get-PEExportedName {
    [CmdletBinding()]
    [OutputType([string[]])]
    param (
        [Parameter(Mandatory = $true)][string]$ImagePath
    )

    $bytes = [System.IO.File]::ReadAllBytes($ImagePath)

    if ($bytes.Length -lt 0x40) {
        throw "'$ImagePath' is too small to be a PE image ($($bytes.Length) bytes)."
    }

    # IMAGE_DOS_HEADER.e_magic == 'MZ'
    if ($bytes[0] -ne 0x4D -or $bytes[1] -ne 0x5A) {
        throw "'$ImagePath' is not a PE image (missing MZ signature)."
    }

    # IMAGE_DOS_HEADER.e_lfanew
    $peOffset = [BitConverter]::ToInt32($bytes, 0x3C)
    if ($peOffset -le 0 -or ($peOffset + 24) -ge $bytes.Length) {
        throw "'$ImagePath' has an invalid PE header offset ($peOffset)."
    }

    # IMAGE_NT_HEADERS.Signature == 'PE\0\0'
    if ([BitConverter]::ToUInt32($bytes, $peOffset) -ne 0x00004550) {
        throw "'$ImagePath' is not a PE image (missing PE signature)."
    }

    # IMAGE_FILE_HEADER follows the 4-byte signature.
    $numberOfSections     = [BitConverter]::ToUInt16($bytes, $peOffset + 6)
    $sizeOfOptionalHeader = [BitConverter]::ToUInt16($bytes, $peOffset + 20)
    $optionalHeaderOffset = $peOffset + 24

    # IMAGE_OPTIONAL_HEADER.Magic: 0x10B = PE32, 0x20B = PE32+. The only difference that matters
    # here is where the data directory array starts.
    $magic = [BitConverter]::ToUInt16($bytes, $optionalHeaderOffset)
    if ($magic -eq 0x10B) {
        $dataDirectoryOffset = $optionalHeaderOffset + 96
    }
    elseif ($magic -eq 0x20B) {
        $dataDirectoryOffset = $optionalHeaderOffset + 112
    }
    else {
        throw ("'$ImagePath' has an unrecognized optional header magic (0x{0:X})." -f $magic)
    }

    # Data directory entry 0 is IMAGE_DIRECTORY_ENTRY_EXPORT.
    $exportRva = [BitConverter]::ToUInt32($bytes, $dataDirectoryOffset)
    if ($exportRva -eq 0) {
        return @()
    }

    # IMAGE_SECTION_HEADER table, 40 bytes per entry, immediately after the optional header.
    $sectionTableOffset = $optionalHeaderOffset + $sizeOfOptionalHeader
    $sections = @()
    for ($i = 0; $i -lt $numberOfSections; $i++) {
        $s = $sectionTableOffset + ($i * 40)
        if (($s + 40) -gt $bytes.Length) {
            throw "'$ImagePath' has a truncated section table."
        }
        $sections += [PSCustomObject]@{
            VirtualSize      = [BitConverter]::ToUInt32($bytes, $s + 8)
            VirtualAddress   = [BitConverter]::ToUInt32($bytes, $s + 12)
            SizeOfRawData    = [BitConverter]::ToUInt32($bytes, $s + 16)
            PointerToRawData = [BitConverter]::ToUInt32($bytes, $s + 20)
        }
    }

    # Translate a relative virtual address into a file offset via the owning section.
    $rvaToOffset = {
        param([uint32]$rva)
        foreach ($section in $sections) {
            # VirtualSize can be 0 in object files; fall back to the raw size.
            $span = if ($section.VirtualSize -gt 0) { $section.VirtualSize } else { $section.SizeOfRawData }
            if ($rva -ge $section.VirtualAddress -and $rva -lt ($section.VirtualAddress + $span)) {
                return [int]($section.PointerToRawData + ($rva - $section.VirtualAddress))
            }
        }
        throw ("RVA 0x{0:X} in '$ImagePath' does not fall inside any section." -f $rva)
    }

    # IMAGE_EXPORT_DIRECTORY
    $exportOffset      = & $rvaToOffset $exportRva
    $numberOfNames     = [BitConverter]::ToUInt32($bytes, $exportOffset + 24)
    $addressOfNamesRva = [BitConverter]::ToUInt32($bytes, $exportOffset + 32)
    if ($numberOfNames -eq 0 -or $addressOfNamesRva -eq 0) {
        return @()
    }

    $namesTableOffset = & $rvaToOffset $addressOfNamesRva
    $names = [string[]]::new($numberOfNames)
    for ($i = 0; $i -lt $numberOfNames; $i++) {
        $nameRva    = [BitConverter]::ToUInt32($bytes, $namesTableOffset + ($i * 4))
        $nameOffset = & $rvaToOffset $nameRva
        $end = $nameOffset
        while ($end -lt $bytes.Length -and $bytes[$end] -ne 0) { $end++ }
        $names[$i] = [System.Text.Encoding]::ASCII.GetString($bytes, $nameOffset, $end - $nameOffset)
    }

    return $names
}

<#
.SYNOPSIS
    Resolves the AdskIdentitySDK.dll to inspect.
.DESCRIPTION
    Resolution order: an explicit -DllPath, then a recursive search of -Path, then the repository's
    default build-output folders, then the Autodesk.IDSDK NuGet package version referenced by
    DynamoCore.csproj. Returns $null when nothing is found so the caller can fail loudly.
.OUTPUTS
    System.String - the resolved DLL path, or $null when it cannot be found.
#>
function Resolve-IdsdkDll {
    [CmdletBinding()]
    [OutputType([string])]
    param ()

    if ($DllPath) {
        if (-not (Test-Path -LiteralPath $DllPath -PathType Leaf)) {
            throw "The -DllPath '$DllPath' does not exist."
        }
        return (Resolve-Path -LiteralPath $DllPath).Path
    }

    # <repo>/.github/scripts/this.ps1 -> <repo>
    $repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

    $searchRoots = @()
    if ($Path) {
        if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
            throw "The -Path '$Path' does not exist or is not a directory."
        }
        $searchRoots += $Path
    }
    else {
        $searchRoots += (Join-Path $repoRoot 'bin/AnyCPU/Release')
        $searchRoots += (Join-Path $repoRoot 'bin/AnyCPU/Debug')
    }

    foreach ($root in $searchRoots) {
        if (-not (Test-Path -LiteralPath $root -PathType Container)) { continue }
        $hit = Get-ChildItem -LiteralPath $root -Filter $DllName -Recurse -File -ErrorAction SilentlyContinue |
            Select-Object -First 1
        if ($hit) { return $hit.FullName }
    }

    # An explicit -Path was a direct instruction; do not silently fall back to the package cache.
    if ($Path) { return $null }

    # Fall back to the NuGet package the build would restore, so the guard is still meaningful
    # for a developer who has restored but not yet built.
    $csproj = Join-Path $repoRoot 'src/DynamoCore/DynamoCore.csproj'
    if (Test-Path -LiteralPath $csproj) {
        $match = Select-String -LiteralPath $csproj -Pattern 'Include="Autodesk\.IDSDK"\s+Version="([^"]+)"' |
            Select-Object -First 1
        if ($match) {
            $version = $match.Matches[0].Groups[1].Value
            $packagesRoot = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $HOME '.nuget/packages' }
            $candidate = Join-Path $packagesRoot "autodesk.idsdk/$version/build/filesToInclude/$DllName"
            if (Test-Path -LiteralPath $candidate -PathType Leaf) {
                # Deferred rather than written here: anything this function writes to the output
                # stream would be captured into its return value alongside the path.
                $script:fallbackNotice = "::notice::No build output found - falling back to the restored Autodesk.IDSDK $version package."
                return (Resolve-Path -LiteralPath $candidate).Path
            }
        }
    }

    return $null
}

$script:fallbackNotice = $null

try {
    $resolved = Resolve-IdsdkDll
} catch {
    Write-Output "::error title=IDSDK export check::$($_.Exception.Message)"
    exit 1
}

if (-not $resolved) {
    $searched = if ($Path) { "'$Path'" } else { "the default build output (bin/AnyCPU/Release, bin/AnyCPU/Debug) and the restored Autodesk.IDSDK package" }
    Write-Output "::error title=IDSDK export check::Could not locate $DllName in $searched. The IDSDK MCP export guard could not run, so it is failing rather than passing silently. Build Dynamo.All.sln first, or pass -Path/-DllPath explicitly."
    exit 1
}

if ($script:fallbackNotice) { Write-Output $script:fallbackNotice }

$fileVersion = try { [System.Diagnostics.FileVersionInfo]::GetVersionInfo($resolved).FileVersion } catch { $null }
if ([string]::IsNullOrWhiteSpace($fileVersion)) { $fileVersion = '<unavailable>' }
Write-Output "::notice::Checking $DllName - $fileVersion - $resolved"

try {
    $exports = Get-PEExportedName -ImagePath $resolved
} catch {
    Write-Output "::error title=IDSDK export check::Failed to read the PE export table of '$resolved' ($fileVersion): $($_.Exception.Message)"
    exit 1
}

$missing = @($RequiredExports | Where-Object { $exports -notcontains $_ })

foreach ($export in $RequiredExports) {
    if ($missing -contains $export) {
        Write-Host "[FAIL] $export"
    } else {
        Write-Host "[ OK ] $export"
    }
}

if ($missing.Count -gt 0) {
    $message = @(
        "$DllName is missing the IDSDK MCP validation API.",
        "  DLL:                    $resolved",
        "  FileVersion:            $fileVersion",
        "  Missing export(s):      $($missing -join ', ')",
        "  Exported symbols found: $($exports.Count)",
        "",
        "MCP token validation will fail for EVERY request, breaking Autodesk Assistant and all MCP",
        "features at runtime - this is exactly the regression that shipped in Dynamo 4.2.0 (DYN-10773).",
        "The MCP entry points require IDSDK 1.17.0 or newer; the Autodesk.IDSDK package reference in",
        "src/DynamoCore/DynamoCore.csproj must be at least 1.2.9.",
        "",
        "Do NOT 'fix' this by comparing file versions: the broken 1.2.6 DLL (1.16.5.1) has a HIGHER",
        "version than a known-good one (Revit's 1.16.4.7). Only the export table is trustworthy."
    ) -join "`n"

    Write-Host "`n$message"
    # Newlines are percent-encoded so the whole explanation survives as one GitHub error annotation.
    Write-Output "::error title=IDSDK MCP export missing (DYN-10773)::$($message -replace "`n", '%0A')"
    exit 1
}

Write-Output "::notice::$DllName ($fileVersion) exports the IDSDK MCP validation API."
exit 0
