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

try {
    Write-Host "Checking if adsk npm registry is reachable..." -ForegroundColor Blue
    $response = Invoke-WebRequest -Uri $adskNpmRegistry -TimeoutSec 20 -ErrorAction Stop

    if ($response.StatusCode -eq 200) {
        Write-Host "adsk npm registry is reachable" -ForegroundColor Green
        createNpmrcFile -registry $adskNpmRegistry
        Write-Output "//npm.autodesk.com/artifactory/api/npm/:_authToken=`${NPM_TOKEN}" | Out-File -FilePath .npmrc -Encoding UTF8 -Append
    }
    else {
        Write-Host "adsk npm registry is not reachable" -ForegroundColor Red
        createNpmrcFile -registry $npmRegistry
    }
}
catch {
    Write-Host "adsk npm registry is not reachable" -ForegroundColor Red
    createNpmrcFile -registry $npmRegistry
}
