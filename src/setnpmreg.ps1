# This script checks if the adsk network is reachable
# and creates .npmrc file with appropriate value for the npm registry

$npmRegistry = "https://registry.npmjs.org"
$adskNpmRegistry = "https://npm.autodesk.com/artifactory/api/npm/autodesk-npm-virtual/"

function createNpmrcFile {
    param (
        [Parameter(Mandatory = $true)][string]$registry
    )
    Write-Host "Creating .npmrc file with registry=$registry" -ForegroundColor Blue
    New-Item -Path . -Name .npmrc -ItemType File -Value "registry=$registry`n" -Force
}

function Test-UrlReachable {
    param ([string]$url)
    $response = $null
    try {
        $request = [System.Net.WebRequest]::Create($url)
        $request.Method = "HEAD"
        $request.Timeout = 20000  # 20 seconds
        $response = $request.GetResponse()
        return $true
    }
    catch {
        return $false
    }
    finally {
        if ($response) { $response.Dispose() }
    }
}

Write-Host "Checking if adsk npm registry is reachable..." -ForegroundColor Blue

if (Test-UrlReachable -url $adskNpmRegistry) {
    Write-Host "adsk npm registry is reachable" -ForegroundColor Green
    createNpmrcFile -registry $adskNpmRegistry
    Write-Output "//npm.autodesk.com/artifactory/api/npm/:_authToken=`${NPM_TOKEN}" | Out-File -FilePath .npmrc -Encoding UTF8 -Append
}
else {
    Write-Host "adsk npm registry is not reachable" -ForegroundColor Red
    createNpmrcFile -registry $npmRegistry
}
