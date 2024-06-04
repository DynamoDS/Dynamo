# Check File Version
#
# This script checks the version exe and dll the files in the Dynamo's bin directory.
# It compares the version of each file with the version of DynamoSandbox.exe.
# If the version of a file doesn't match the version of DynamoSandbox.exe, the script will exit with an error code.
#
# https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.fileversioninfo
param (
    [Parameter(Mandatory = $true)][string]$path
)

$ErrorActionPreference = "Stop"

$includedFiles = @("*.exe", "*.dll")
$excludedFiles = @(
    "*.config",
    "*.ds",
    "*.json",
    "*.md",
    "*.rtf",
    "*.xml",
    "AdpSDKCSharpWrapper.dll",
    "AdskIdentitySDK.dll",
    "Analytics.NET.*.dll",
    "Autodesk*.dll",
    "CommandLine.dll",
    "Cyotek.Drawing.BitmapFont.dll",
    "DiffPlex.dll",
    "DocumentFormat.OpenXml.dll",
    "DotNetProjects.Wpf.Extended.Toolkit.dll",
    "Dynamo.Microsoft.Xaml.Behaviors.dll",
    "FontAwesome5*.dll",
    "ForgeUnitsManaged.dll",
    "Greg.dll",
    "HarfBuzzSharp.dll",
    "HelixToolkit*.dll",
    "ICSharpCode.AvalonEdit.dll",
    "J2N.dll",
    "JUnit.TestLogger.dll",
    "LaunchDarkly.*",
    "LibG*.dll",
    "libiconv.dll",         # https://jira.autodesk.com/browse/DYN-7069
    "libintl.dll",          # https://jira.autodesk.com/browse/DYN-7069
    "libHarfBuzzSharp.dll", # https://jira.autodesk.com/browse/DYN-6598
    "libSkiaSharp.dll",     # https://jira.autodesk.com/browse/DYN-6598
    "LiveChartsCore*.dll",
    "Lucene.Net*.dll",
    "MIConvexHull.NET Standard.dll",
    "Microsoft.*",
    "MimeMapping.dll",
    "Moq.dll",
    "Newtonsoft.Json.dll",
    "NuGet.Frameworks.dll",
    "nunit*.dll",
    "NUnit3*.dll",
    "Prism.dll",
    "ProtoGeometry*.dll",
    "Python*.dll",
    "RestSharp.dll",
    "SharpDX*.dll",
    "SkiaSharp*.dll",
    "Spekt.TestLogger.dll",
    "StarMath.dll",
    "System.*.dll",
    "testcentric.engine.metadata.dll",
    "testhost.dll",
    "testhost.exe",
    "Units.dll",
    "Webview2Loader.dll"
)

$dynamoSandbox = Join-Path -Path $path -ChildPath "DynamoSandbox.exe"
if (Test-Path $dynamoSandbox) {
    $fileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dynamoSandbox).FileVersion
    $dynamoVersion = [System.Version]::Parse($fileVersion)
    Write-Host "ℹ️ DynamoSandbox.exe - $dynamoVersion`n"
} else {
    Write-Host "⚠️ DynamoSandbox.exe was not found"
    exit 1
}

$files = Get-ChildItem -Path $path -Include $includedFiles -Exclude $excludedFiles -Recurse -File
foreach ($file in $files) {
    $name = $file.Name
    $fullName = $file.FullName
    try {
        $fileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($file).FileVersion
        $version = [System.Version]::Parse($fileVersion)
        if ($version -eq $dynamoVersion) {
            Write-Host "✅ $name - $version"
        } else {
            Write-Host "❌ $name - $version doesn't match the expected version: $dynamoVersion.`n$fullName`n"
            exit 1
        }
    } catch {
        Write-Host "❌ $name - Failed to get the version. `n$fullName`n"
        exit 1
    }
}
