<#
   Date: 07/04/2019
   Purpose: Pre Build Script of Dynamo
#>
$ErrorActionPreference = "Stop"

#=====================ASM Configuration=====================#

$ASMBranch = "225_0_0"
$ASM = "asm-a-lib_win_release_intel64_v140"
$ASMVer = "225.0.0"
$ASMBinPackageName = "asm-a_win_release_intel64_v140"
$ASMBin = "$ASMBinPackagename.$ASMVer"
$ASMSource = "autodesk3p"

#=====================TSP=====================#
$TSPLINESVer = "7.0.0"
$TSPLINESBin = "tsplines-a_win_release_intel64_v140.$TSPLINESVer"

#=====================TBB=====================#
$TBB = "TBB-2017U5-0226_win_release_intel64_v140"
$TBBVer = "1.0.1"
$TBBBin = "$TBB.$TBBVer"

#=====================Directories=====================#

$MSBuildProjectDirectory = "$env:WORKSPACE"

$NugetConfig = "$MSBuildProjectDirectory\dynamo-nuget.config"
$PackageDirectory = "$MSBuildProjectDirectory\asm\asm_sdk_$ASMBranch\packages"
$DynamoExtern = "$MSBuildProjectDirectory\extern"

try
{
	#Make Directory
	New-Item -Path "$MSBuildProjectDirectory\asm\asm_sdk_$ASMBranch" -Name "packages" -ItemType "directory" -Force

	#Donwload ASM
	Set-Location -Path $PackageDirectory
	C:\Nuget\nuget.exe install -configFile $NugetConfig $ASM -version $ASMVer -source $ASMSource

	#ASM Copy
	Copy-Item "$PackageDirectory\$ASMBin\bin\*" -Destination "$DynamoExtern\LibG_$ASMBranch\" -Recurse

	#TSP Copy
	Copy-Item "$PackageDirectory\$TSPLINESBin\bin\*.dll" -Destination "$DynamoExtern\LibG_$ASMBranch\"

	#TBB Copy
	Copy-Item "$PackageDirectory\$TBBBin\bin\*.dll" -Destination "$DynamoExtern\LibG_$ASMBranch\"


	#Docker configuration
	docker pull artifactory.dev.adskengineer.net/docker-local-v2/dynamo/buildtools2017sdk81

	docker run -m 8GB -d -t --mount type=bind,source=C:\Jenkins\workspace\Dynamo\Dynamo,target=c:\WorkspaceDynamo --name build-test artifactory.dev.adskengineer.net/docker-local-v2/dynamo/buildtools2017sdk81
}
catch
{
	docker system prune -f	
	Invoke-Item "$env:WORKSPACE\DockerUtilities\RestartDockerDesktop.ps1"
	Write-Host $error[0]
	throw $LASTEXITCODE
}