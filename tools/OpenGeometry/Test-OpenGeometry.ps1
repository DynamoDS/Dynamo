[CmdletBinding()]
param(
    [string]$UsdBrepSource = (Join-Path $PSScriptRoot '../../../usd-brep'),
    [string]$CMakeGenerator = 'Visual Studio 17 2022',
    [string]$MSBuildPath
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$nativeSource = Join-Path $repo 'src/OpenGeometry/native'
$nativeBuild = Join-Path $repo 'bin/open-geometry/native'
$testProject = Join-Path $repo 'test/OpenGeometry/OpenGeometryTests.csproj'
$UsdBrepSource = (Resolve-Path -LiteralPath $UsdBrepSource).Path

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

    # The Dynamo engine build uses the Windows SDK's ResGen and AL tasks.
    & $MSBuildPath $testProject /restore /p:Configuration=Release /m:1 /p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) { throw 'The managed geometry test build failed.' }

    & dotnet test $testProject -c Release --no-build --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'The geometry tests failed.' }
}
finally {
    Pop-Location
}
