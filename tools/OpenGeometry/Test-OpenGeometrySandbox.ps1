[CmdletBinding()]
param(
    [string]$UsdBrepSource = (Join-Path $PSScriptRoot '../../../usd-brep'),
    [string]$CMakeGenerator = 'Visual Studio 17 2022',
    [string]$Configuration = 'Release',
    [string]$StagingDir,
    [string]$MSBuildPath,
    [string]$Filter,
    [switch]$SkipSandboxBuild
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$nativeOut = Join-Path $repo 'bin/open-geometry/native/Release'
$testProject = Join-Path $repo 'test/OpenGeometry/SandboxTests/OpenGeometrySandboxTests.csproj'
$buildScript = Join-Path $PSScriptRoot 'Build-OpenGeometrySandbox.ps1'

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
    if (-not $SkipSandboxBuild) {
        & $buildScript -UsdBrepSource $UsdBrepSource -CMakeGenerator $CMakeGenerator -Configuration $Configuration -StagingDir $StagingDir -MSBuildPath $MSBuildPath
    }

    & $MSBuildPath $testProject /restore `
        "/p:Configuration=$Configuration" `
        '/p:UseOpenGeometry=true' `
        "/p:OpenGeometryNativeDirectory=$nativeOut" `
        "/p:OpenGeometryStagingDir=$StagingDir" `
        '/m:1' '/p:UseSharedCompilation=false'
    if ($LASTEXITCODE -ne 0) { throw 'OpenGeometry Sandbox test build failed.' }

    $env:OPEN_GEOMETRY_STAGING_DIR = $StagingDir
    $env:OPEN_GEOMETRY_REPO_ROOT = $repo
    $testArgs = @('test', $testProject, '-c', $Configuration, '--no-build', '--no-restore', '-p:UseOpenGeometry=true', '-v', 'normal')
    if ($Filter) {
        $testArgs += @('--filter', $Filter)
    }

    & dotnet @testArgs
    if ($LASTEXITCODE -ne 0) { throw 'OpenGeometry Sandbox tests failed.' }
}
finally {
    Pop-Location
}
