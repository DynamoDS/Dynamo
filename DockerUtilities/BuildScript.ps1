<#
   Date: 07/04/2019
   Purpose: Build Script of Dynamo
#>
$ErrorActionPreference = "Stop"

try
{
	$WorkSpaceElements = "$env:WORKSPACE" -split '\\'
	$WorkSpaceName = $WorkSpaceElements[$WorkSpaceElements.Length - 1]
	docker exec build-test msbuild "c:\WorkspaceDynamo\$WorkSpaceName\src\build.xml"
}
catch
{
	docker system prune -f	
	Invoke-Item "$env:WORKSPACE\DockerUtilities\RestartDockerDesktop.ps1"
	Write-Host $error[0]
	throw $LASTEXITCODE
}