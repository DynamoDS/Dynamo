set packagesDir=%APPDATA%\Dynamo\0.8\packages\Dynamo Samples
set binDir=%packagesDir%\bin
set nodesDir=%packagesDir%\bin\nodes
set dyfDir=%packagesDir%\dyf
set extraDir=%packagesDir%\extra
set resourcesDir=%binDir%\en-US

set sampleLibraryTestsDir=%cd%\SampleLibraryTests\bin\Debug
set sampleLibraryUIDir=%cd%\SampleLibraryUI\bin\Debug
set sampleLibraryUIResourcesDir=%cd%\SampleLibraryUI\bin\Debug\en-us
set sampleLibraryZeroTouchDir=%cd%\SampleLibraryZeroTouch\bin\Debug

if not exist "%packagesDir%" mkdir "%packagesDir%"
if not exist "%binDir%" mkdir "%binDir%"
if not exist "%dyfDir%" mkdir "%dyfDir%"
if not exist "%extraDir%" mkdir "%extraDir%"
if not exist "%resourcesDir%" mkdir "%resourcesDir%"

REM xcopy "%sampleLibraryTestsDir%\*.dll" "%binDir%" /Y
REM xcopy "%sampleLibraryTestsDir%\*.pdb" "%binDir%" /Y
REM xcopy "%sampleLibraryTestsDir%\*.dyn" "%extraDir%" /Y

xcopy "%sampleLibraryUIDir%\*.dll" "%binDir%" /Y
xcopy "%sampleLibraryUIDir%\*.pdb" "%binDir%" /Y
xcopy "%sampleLibraryUIDir%\*.xml" "%binDir%" /Y
xcopy "%sampleLibraryUIResourcesDir%\*" "%resourcesDir%" /Y

xcopy "%sampleLibraryZeroTouchDir%\*.dll" "%binDir%" /Y
xcopy "%sampleLibraryZeroTouchDir%\*.pdb" "%binDir%" /Y
xcopy "%sampleLibraryZeroTouchDir%\*.xml" "%binDir%" /Y

xcopy "%cd%\*.json" "%packagesDir%" /Y





