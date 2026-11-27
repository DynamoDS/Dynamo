[CmdletBinding()]
param(
    [string]$UsdBrepSource = (Join-Path $PSScriptRoot '../../../usd-brep'),
    [string]$CMakeGenerator = 'Visual Studio 17 2022',
    [string]$Configuration = 'Release',
    [string]$StagingDir,
    [string]$MSBuildPath,
    [switch]$CleanStaging
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$nativeSource = Join-Path $repo 'src/OpenGeometry/native'
$nativeBuild = Join-Path $repo 'bin/open-geometry/native'
$nativeOut = Join-Path $nativeBuild 'Release'
$sandboxProj = Join-Path $repo 'src/OpenGeometry/OpenGeometrySandbox.proj'
$UsdBrepSource = (Resolve-Path -LiteralPath $UsdBrepSource).Path

if (-not $StagingDir) {
    $StagingDir = Join-Path $repo "bin/open-geometry/sandbox/$Configuration"
}

if (-not $MSBuildPath) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
    $MSBuildPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\Current\Bin\amd64\MSBuild.exe' | Select-Object -First 1
    if (-not $MSBuildPath) {
        throw 'Install Visual Studio MSBuild for .NET 10 or pass -MSBuildPath.'
    }
}

Push-Location $repo
try {
    & cmake -S $nativeSource -B $nativeBuild -G $CMakeGenerator -A x64 "-DUSD_BREP_SOURCE=$UsdBrepSource"
    if ($LASTEXITCODE -ne 0) { throw 'CMake configuration failed.' }

    & cmake --build $nativeBuild --config Release --target OpenGeometryBridge -- /m:1
    if ($LASTEXITCODE -ne 0) { throw 'The native geometry build failed.' }

    if ($CleanStaging -and (Test-Path -LiteralPath $StagingDir)) {
        Remove-Item -LiteralPath $StagingDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $StagingDir -Force | Out-Null

    $commonProps = @(
        "/p:Configuration=$Configuration",
        '/p:Platform=AnyCPU',
        '/p:UseOpenGeometry=true',
        "/p:OpenGeometryNativeDirectory=$nativeOut",
        "/p:OpenGeometryStagingDir=$StagingDir",
        '/m:1',
        '/p:UseSharedCompilation=false'
    )

    & $MSBuildPath $sandboxProj /t:Restore @commonProps
    if ($LASTEXITCODE -ne 0) { throw 'OpenGeometry Sandbox restore failed.' }

    & $MSBuildPath $sandboxProj /t:Build @commonProps
    if ($LASTEXITCODE -ne 0) { throw 'OpenGeometry Sandbox build failed.' }

    Write-Host "Staged OpenGeometry Sandbox at: $StagingDir"
}
finally {
    Pop-Location
}
