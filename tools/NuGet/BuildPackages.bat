:: Argument %1: path to template folder
:: Argument %2: path to dynamo build directory
::

@echo off
if not "%2"=="" goto :harvestpathdefined

echo The path to the Dynamo dlls was not specified. Using the dlls in the harvest directory...
:: Get the bin release folder path for building symbol nupkg
set releasePath=..\..\bin\AnyCPU\Release
:: Get the harvest folder path for building rumtime nupkg
set harvestPath=..\..\src\DynamoInstall\harvest

if not exist %harvestPath% (
  echo Dynamo\src\DynamoInstall\harvest folder not found.
  echo Please build Dynamo\src\Install.sln before running this script!
  exit /b 1
)
goto :build

:: When the second argument for running the batch file exists, update both paths to use argument
:harvestpathdefined
set harvestPath=%2
set releasePath=%2

:build
:: Get version string from "DynamoCore.dll"
set count=1
for /f %%f in ('cscript //Nologo ..\install\GetFileVersion.vbs %harvestPath%\DynamoCore.dll') do (
  setlocal EnableDelayedExpansion
  if !count!==1 set Major=%%f
  if !count!==2 set Minor=%%f
  if !count!==3 set Build=%%f
  if !count!==4 set Revision=%%f
  set /a count=!count!+1
)
setlocal DisableDelayedExpansion
set version=%Major%.%Minor%.%Build%.%Revision%

:: Clean files generated from the previous run
if exist *.nupkg ( del *.nupkg )
if exist nuspec ( rmdir /s /q nuspec )
mkdir nuspec

:: Copy .nuspec files from template folder to "nuspec" folder
:: and replace the string "@VERSION@" with the correct value
for %%f in (%1\*.nuspec) do (
  for /f "tokens=* delims=¶" %%i in ( '"type %%f"') do (
    set line=%%i
    setlocal EnableDelayedExpansion
    set line=!line:@VERSION@=%version%!
    echo !line!>>nuspec\%%~nxf
    endlocal
  )
)

:: Pack .nupkg files based on each .nuspec in the "nuspec" folder
for %%f in (nuspec\*.nuspec) do (
  :: Check if nuspec file name containing "Symbols"
  echo %%f|find "Symbols" >nul
  :: When nuget pack symbols, set to release path where the symbol files live
  if errorlevel 1 ( nuget pack %%f -basepath %harvestPath% ) else (nuget pack %%f -basepath %releasePath%)
  if not exist %%~nf.%version%.nupkg (
    exit /b 1
  )
)