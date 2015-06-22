set baseDir=%1
set packagesDir=%APPDATA%\Dynamo\0.8\packages\Dynamo Samples
set binDir=%packagesDir%\bin
set nodesDir=%packagesDir%\bin\nodes
set dyfDir=%packagesDir%\dyf
set extraDir=%packagesDir%\extra
set resourcesDir=%binDir%\en-US

set sampleLibraryTestsDir=%baseDir%\SampleLibraryTests\bin\Debug
set sampleLibraryUIDir=%baseDir%\SampleLibraryUI\bin\Debug
set sampleLibraryUIResourcesDir=%baseDir%\SampleLibraryUI\bin\Debug\en-us
set sampleLibraryZeroTouchDir=%baseDir%\SampleLibraryZeroTouch\bin\Debug

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

xcopy "%baseDir%\*.json" "%packagesDir%" /Y





