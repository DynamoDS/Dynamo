@echo off

REM ***********************************************************************
REM This bat file uses Aget: https://git.autodesk.com/Dynamo/Aget to 
REM download the nuget packages from closest Artifactory server for 
REM
REM Santa Clara: https://art-bobcat.autodesk.com
REM Novi:        https://art-cougar.autodesk.com
REM Singapore:   https://art-lion.autodesk.com
REM Shanghai:    https://art-panda.autodesk.com
REM Neuchatel:   https://art-chamois.autodesk.com
REM
REM ***********************************************************************

setlocal EnableDelayedExpansion
setlocal EnableExtensions

set DynamoPackagePath=%~dp0\packages\_packages
set DynamoNugetPath = 
set CurrentDir=%~dp0
if %CurrentDir:~-1%==\ (
        set CurrentDir=%CurrentDir:~0,-1%
)

REM 1. setup the global nuget package location: DynamoPackages
:SetupGlobalPackageLocation
    if "!DynamoPackages!"=="" (
            echo The environment variable: DynamoPackages was not defined, will use the default location: %DynamoPackagePath% to store the global nuget packages for Dynamo.
            set DynamoPackages=%DynamoPackagePath%
    )
    REM remove "" in DynamoPackages
    echo Break Test
    set DynamoPackages=%DynamoPackages:"=%


REM 2. Set AF_MIRROR to the online public nuget server
set AF_MIRROR = "https://www.nuget.org/api/v2"
REM 3. download nuget.exe from closest AF server
:DownloadNuget

REM 4 set the directory to find nugetCLI ie. nuget.exe               
set NugetExe=%CurrentDir%\Tools\NugetCLI\nuget.exe

REM 5 download 3rdParty packages by Aget.py
:DownloadNugetPackages
    
    set AgetFile=%CurrentDir%\Tools\Aget\aget.exe
    echo !AgetFile!

    set nugetConfig = %CurrentDir%\..\dynamo-nuget.config
    set PythonAget="!AgetFile!" -os win -config release -iset intel64 -toolchain v140 -linkage shared -packagesDir "!DynamoPackages!" -nuget "!NugetExe!" -framework net45 -nugetConfig "!nugetConfig!"

    call :TrackTime "[Aget] Downloading 3rdParty packages from Artifactory server: !AF_MIRROR!, might take a while if running for the first time."
    ::"https://www.nuget.org/api/v2"
    :: Symlinks are generated here
    set ThirdPartyDir=%CurrentDir%\packages
    !PythonAget! -agettable "%CurrentDir%\Config\packages.aget" -refsDir !ThirdPartyDir!
    if ERRORLEVEL 1 (
            echo ERROR: Failed to update Dynamo 3rdParty nuget packages in !ThirdPartyDir!\packages.aget
            exit /b 1
    )
    call :TrackTime "%~n0: finished successfully"
    endlocal
    exit /b 0
:TrackTime
echo ========================================================
echo %~1
time /t
echo ========================================================
