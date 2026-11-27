[CmdletBinding()]
param(
    [string]$UsdBrepSource = (Join-Path $PSScriptRoot '../../../usd-brep'),
    [string]$RehearsalRef = '3fd5b95',
    [string]$CMakeGenerator = 'Visual Studio 17 2022',
    [string]$MSBuildPath
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$nativeSource = Join-Path $repo 'src/OpenGeometry/native'
$refFile = Join-Path $nativeSource 'usd-brep.ref'
$testProject = Join-Path $repo 'test/OpenGeometry/OpenGeometryTests.csproj'
$UsdBrepSource = (Resolve-Path -LiteralPath $UsdBrepSource).Path

$pinnedRef = (Get-Content -LiteralPath $refFile | Select-Object -First 1).Trim()
$currentHead = (& git -C $UsdBrepSource rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $currentHead -ne $pinnedRef) {
    throw "Expected $UsdBrepSource to be on pinned ref $pinnedRef (found '$currentHead')."
}

$rehearsalSha = (& git -C $UsdBrepSource rev-parse --verify "$RehearsalRef^{commit}").Trim()
if ($LASTEXITCODE -ne 0 -or -not $rehearsalSha) {
    throw "Could not resolve rehearsal commit '$RehearsalRef' in $UsdBrepSource."
}
if ($rehearsalSha -eq $pinnedRef) {
    throw "RehearsalRef '$RehearsalRef' resolves to the pinned ref $pinnedRef; choose a distinct revision."
}

if (-not $MSBuildPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
    $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\Current\Bin\amd64\MSBuild.exe' | Select-Object -First 1
    if (-not $MSBuildPath) {
        throw 'Install Visual Studio MSBuild for .NET 10 or pass -MSBuildPath.'
    }
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("OpenGeometryRehearsal_" + [Guid]::NewGuid().ToString('N'))
$worktreePath = Join-Path $tempRoot 'usd-brep'
$pinCheckBuild = Join-Path $tempRoot 'pin-check'
$rehearsalBuild = Join-Path $tempRoot 'native-rehearsal'
$rehearsalTestDir = Join-Path $tempRoot 'test-bin'
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

$worktreeAdded = $false
Push-Location $repo
try {
    Write-Host "Creating temporary usd-brep worktree at $rehearsalSha ($RehearsalRef)..."
    & git -C $UsdBrepSource worktree add --detach $worktreePath $rehearsalSha
    if ($LASTEXITCODE -ne 0) { throw 'Failed to create temporary usd-brep worktree.' }
    $worktreeAdded = $true

    Write-Host 'Step 1: Verifying strict usd-brep.ref pin enforcement rejects mismatched revision...'
    $pinOutput = & cmake -S $nativeSource -B $pinCheckBuild -G $CMakeGenerator -A x64 "-DUSD_BREP_SOURCE=$worktreePath" 2>&1
    $pinExit = $LASTEXITCODE
    if ($pinExit -eq 0) {
        throw "Pin enforcement failed: CMake unexpectedly accepted unpinned commit $rehearsalSha."
    }
    $pinText = ($pinOutput | Out-String)
    if ($pinText -notmatch [regex]::Escape($pinnedRef)) {
        throw "Pin enforcement error did not reference expected pin $pinnedRef.`n$pinText"
    }
    Write-Host "  Pin check correctly rejected $rehearsalSha (expected $pinnedRef)."

    Write-Host "Step 2: Building native bridge against rehearsal revision $rehearsalSha..."
    & cmake -S $nativeSource -B $rehearsalBuild -G $CMakeGenerator -A x64 "-DUSD_BREP_SOURCE=$worktreePath" "-DUSD_BREP_EXPECTED_REF=$rehearsalSha"
    if ($LASTEXITCODE -ne 0) { throw 'Rehearsal CMake configuration failed.' }

    & cmake --build $rehearsalBuild --config Release --target OpenGeometryBridge -- /m:1
    if ($LASTEXITCODE -ne 0) { throw 'Rehearsal native build failed.' }

    Write-Host 'Step 3: Running OpenGeometryTests against rehearsal native DLLs...'
    & $MSBuildPath $testProject /restore /p:Configuration=Release /m:1 /p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) { throw 'Managed test build failed.' }

    $managedBin = Join-Path $repo 'test/OpenGeometry/bin/Release/net10.0'
    Copy-Item -Path $managedBin -Destination $rehearsalTestDir -Recurse -Force
    foreach ($dll in @('SMLib.dll', 'SM_API.dll', 'OpenGeometryBridge.dll')) {
        Copy-Item -LiteralPath (Join-Path $rehearsalBuild "Release/$dll") -Destination (Join-Path $rehearsalTestDir $dll) -Force
    }

    $testAssembly = Join-Path $rehearsalTestDir 'OpenGeometryTests.dll'
    & dotnet vstest $testAssembly
    if ($LASTEXITCODE -ne 0) { throw "OpenGeometryTests failed against rehearsal revision $rehearsalSha." }

    Write-Host "Update rehearsal succeeded: pin guard enforced $pinnedRef and all tests passed against $rehearsalSha."
}
finally {
    Pop-Location
    if ($worktreeAdded) {
        & git -C $UsdBrepSource worktree remove --force $worktreePath 2>$null
    }
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
    $postHead = (& git -C $UsdBrepSource rev-parse HEAD).Trim()
    if ($postHead -ne $pinnedRef) {
        throw "Primary usd-brep checkout HEAD drifted to $postHead (expected $pinnedRef)."
    }
}
