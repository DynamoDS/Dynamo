SET base=%1

rmdir /s /q .\build
rmdir /s /q .\content
rmdir /s /q .\lib
rmdir /s /q .\tools

mkdir build
mkdir content
mkdir lib
mkdir tools

REM Copy core
copy %base%\DynamoCore.dll .\lib\DynamoCore.dll

REM Copy nodes
copy %base%\DSCoreNodes.dll .\lib\DynamoCore.dll
copy %base%\nodes\DSCoreNodeUIs.dll .\lib\DSCoreNodesUI.dll 

REM Copy ProtoGeometry
copy %base%\ProtoGeometry.dll .\lib\ProtoGeometry.dll
copy %base%\ProtoInterface.dll .\lib\ProtoGeometry.dll

REM Copy test libraries.
copy %base%\TestServices.dll .\lib\TestServices.dll

REM Copy .cs files
xcopy ..\..\src\Libraries\Samples\SampleLibraryZeroTouch\*.cs .\content\