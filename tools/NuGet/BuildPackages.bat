@echo off
SET base=..\..\src\DynamoInstall\harvest
if not exist %base% (
  echo Dynamo\src\DynamoInstall\harvest folder not found.
  echo Please build Dynamo\src\Install.sln before running this script!
  exit /b 1
)

:: Get version string from "DynamoCore.dll"
set count=1
for /f %%f in ('cscript //Nologo ..\install\GetFileVersion.vbs %base%\DynamoCore.dll') do (
  setlocal EnableDelayedExpansion
  if !count!==1 set Major=%%f
  if !count!==2 set Minor=%%f
  if !count!==3 set Build=%%f
  if !count!==4 set Revision=%%f
  set /a count=!count!+1
)
setlocal DisableDelayedExpansion
set version=%Major%.%Minor%.%Build%-beta%Revision%

:: Copy .nuspec files from "template" folder to "nuspec" folder
:: and replace the string "@VERSION@" with the correct value
if exist nuspec ( rmdir /s /q nuspec )
mkdir nuspec
for %%f in (template\*.nuspec) do (
  for /f "tokens=* delims=¶" %%i in ( '"type %%f"') do (
    set line=%%i
    setlocal EnableDelayedExpansion
    set line=!line:@VERSION@=%version%!
    echo !line!>>nuspec\%%~nxf
    endlocal
  )
)

:: Pack .nupkg files based on each .nuspec in the "nuspec" folder
@echo on
for %%f in (nuspec\*.nuspec) do (
  nuget pack %%f -basepath %base%
)
