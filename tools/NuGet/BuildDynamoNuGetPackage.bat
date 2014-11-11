SET base=..\..\bin\AnyCPU\Debug
SET framework=net40

rmdir /s /q .\build
rmdir /s /q .\content
rmdir /s /q .\lib
rmdir /s /q .\tools

mkdir build
mkdir content
mkdir lib\net40
mkdir tools

REM Copy core
copy %base%\DynamoCore.dll .\lib\%framework%\DynamoCore.dll

REM Copy nodes
copy %base%\DSCoreNodes.dll .\lib\%framework%\DynamoCore.dll
copy %base%\nodes\DSCoreNodeUIs.dll .\lib\%framework%\DSCoreNodesUI.dll 

REM Copy ProtoGeometry
copy %base%\ProtoGeometry.dll .\lib\%framework%\ProtoGeometry.dll
copy %base%\ProtoInterface.dll .\lib\%framework%\ProtoInterface.dll

REM Copy test libraries.
copy %base%\TestServices.dll .\lib\%framework%\TestServices.dll

REM Copy .cs files
xcopy ..\..\src\Libraries\Samples\SampleLibraryZeroTouch\*.cs .\content\

nuget pack Dynamo.nuspec