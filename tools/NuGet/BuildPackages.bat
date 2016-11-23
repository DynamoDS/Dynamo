@echo off
SET base=..\..\bin\AnyCPU\Release
SET framework=net45

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
set shortversion=%Major%.%Minor%.%Build%
set pkgversion=%shortversion%-beta%Revision%

:: Initialize folder structure and copy .nuspec to each package folder
:: with strings @PKGVERSION@ and @SHORTVERSION@ replaced
rmdir /s /q pack
for %%f in (nuspec\*.nuspec) do (
  setlocal DisableDelayedExpansion
  mkdir pack\%%~nf\build\net45
  mkdir pack\%%~nf\content
  mkdir pack\%%~nf\lib\net45
  mkdir pack\%%~nf\tools
  
  for /f "tokens=* delims=¶" %%i in ( '"type %%f"') do (
    set line=%%i
    setlocal EnableDelayedExpansion
    set line=!line:@PKGVERSION@=%pkgversion%!
    set line=!line:@SHORTVERSION@=%shortversion%!
    echo !line!>>pack\%%~nf\%%~nxf
    endlocal
  )
)

:: Core package
SET pkg=DynamoVisualProgramming.Core
copy %base%\DynamoCore.dll pack\%pkg%\lib\%framework%\DynamoCore.dll
copy %base%\DynamoCore.xml pack\%pkg%\build\%framework%\DynamoCore.xml
copy %base%\ProtoCore.dll pack\%pkg%\lib\%framework%\ProtoCore.dll
copy %base%\DynamoShapeManager.dll pack\%pkg%\lib\%framework%\DynamoShapeManager.dll
copy %base%\DynamoUtilities.dll pack\%pkg%\lib\%framework%\DynamoUtilities.dll

:: DynamoCoreNodes package
SET pkg=DynamoVisualProgramming.DynamoCoreNodes
copy %base%\Display.dll pack\%pkg%\lib\%framework%\Display.dll
copy %base%\Display.xml pack\%pkg%\build\%framework%\Display.xml
copy %base%\DSCoreNodes.dll pack\%pkg%\lib\%framework%\DSCoreNodes.dll
copy %base%\DSCoreNodes.xml pack\%pkg%\build\%framework%\DSCoreNodes.xml

:: DynamoServices package
SET pkg=DynamoVisualProgramming.DynamoServices
copy %base%\DynamoServices.dll pack\%pkg%\lib\%framework%\DynamoServices.dll
copy %base%\DynamoServices.xml pack\%pkg%\build\%framework%\DynamoServices.xml

:: Tests package
SET pkg=DynamoVisualProgramming.Tests
copy %base%\TestServices.dll pack\%pkg%\lib\%framework%\TestServices.dll
copy %base%\SystemTestServices.dll pack\%pkg%\lib\%framework%\SystemTestServices.dll

:: WpfUILibrary package
SET pkg=DynamoVisualProgramming.WpfUILibrary
copy %base%\DynamoCoreWpf.dll pack\%pkg%\lib\%framework%\DynamoCoreWpf.dll
copy %base%\DynamoCoreWpf.xml pack\%pkg%\build\%framework%\DynamoCoreWpf.xml
copy %base%\nodes\CoreNodeModels.dll pack\%pkg%\lib\%framework%\CoreNodeModels.dll
copy %base%\nodes\CoreNodeModels.xml pack\%pkg%\build\%framework%\CoreNodeModels.xml
copy %base%\nodes\CoreNodeModelsWpf.dll pack\%pkg%\lib\%framework%\CoreNodeModelsWpf.dll
copy %base%\nodes\CoreNodeModelsWpf.xml pack\%pkg%\build\%framework%\CoreNodeModelsWpf.xml

:: ZeroTouchLibrary package
SET pkg=DynamoVisualProgramming.ZeroTouchLibrary
copy %base%\ProtoGeometry.dll pack\%pkg%\lib\%framework%\ProtoGeometry.dll
copy %base%\ProtoGeometry.xml pack\%pkg%\build\%framework%\ProtoGeometry.xml
copy %base%\DynamoUnits.dll pack\%pkg%\lib\%framework%\DynamoUnits.dll

:: Pack NuGet packages
@echo on
for /r "pack" %%f in (*.nuspec) do (
  nuget pack %%f
)