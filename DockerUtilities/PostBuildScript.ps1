<#
   Date: 07/04/2019
   Purpose: Post Build Script of Dynamo
#>
$ErrorActionPreference = "Stop"

try
{
	docker container stop build-test
	docker container rm build-test
}
catch
{
	Write-Host $error[0]
	throw $LASTEXITCODE
}
finally
{
	docker system prune -f
	Invoke-Item "$env:WORKSPACE\DockerUtilities\RestartDockerDesktop.ps1"
}

