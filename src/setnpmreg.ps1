# This script checks if the adsk network is reachable
# and creates .npmrc file with appropriate value for the npm registry
$ErrorActionPreference = "Stop"

$npmRegistry = "https://registry.npmjs.org"
$adskNpmRegistry = "https://npm.autodesk.com/artifactory/api/npm/autodesk-npm-virtual/"

function createNpmrcFile {
    param (
        [Parameter(Mandatory = $true)][string]$registry
    )
    Write-Host "Creating .npmrc file with registry=$registry" -ForegroundColor Blue
    New-Item -Path . -Name ".npmrc" -ItemType "file" -Value "registry=$registry" -Force
}

try {
    Write-Host "Checking if adsk npm registry is reachable..." -ForegroundColor Blue
    $httpClient = New-Object System.Net.Http.HttpClient
    $httpClient.Timeout = [TimeSpan]::FromSeconds(5)
    $response = $httpClient.GetAsync($adskNpmRegistry).Result

    if ($response.IsSuccessStatusCode) {
        Write-Host "adsk npm registry is reachable" -ForegroundColor Green
        createNpmrcFile -registry $adskNpmRegistry
    }
    else {
        Write-Host "adsk npm registry is not reachable" -ForegroundColor Red
        createNpmrcFile -registry $npmRegistry
    }

    $httpClient.Dispose()
}
catch {
    Write-Host "An error occurred: $_" -ForegroundColor Red
    createNpmrcFile -registry $npmRegistry
}
