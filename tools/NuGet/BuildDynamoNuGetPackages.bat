SET base=..\..\bin\AnyCPU\Debug
SET framework=net40
SET zt=DynamoVisualProgramming.ZeroTouchLibrary
SET ui=DynamoVisualProgramming.UILibrary

rmdir /s /q .\%zt%\build
rmdir /s /q .\%zt%\content
rmdir /s /q .\%zt%\lib
rmdir /s /q .\%zt%\tools

rmdir /s /q .\%ui%\build
rmdir /s /q .\%ui%\content
rmdir /s /q .\%ui%\lib
rmdir /s /q .\%ui%\tools

mkdir .\%zt%\build
mkdir .\%zt%\content
mkdir .\%zt%\lib\net40
mkdir .\%zt%\tools

mkdir .\%ui%\build
mkdir .\%ui%\content
mkdir .\%ui%\lib\net40
mkdir .\%ui%\tools

REM Copy core
copy %base%\DynamoCore.dll .\%zt%\lib\%framework%\DynamoCore.dll
copy %base%\DynamoCore.dll .\%ui%\lib\%framework%\DynamoCore.dll

REM Copy core nodes
copy %base%\DSCoreNodes.dll .\%zt%\lib\%framework%\DSCoreNodes.dll 
copy %base%\DSCoreNodes.dll .\%ui%\lib\%framework%\DSCoreNodes.dll 

REM Copy ui nodes
copy %base%\nodes\DSCoreNodesUI.dll .\%ui%\lib\%framework%\DSCoreNodesUI.dll 

REM Copy ProtoGeometry
copy %base%\ProtoGeometry.dll .\%zt%\lib\%framework%\ProtoGeometry.dll
copy %base%\ProtoInterface.dll .\%zt%\lib\%framework%\ProtoInterface.dll
copy %base%\ProtoInterface.dll .\%ui%\lib\%framework%\ProtoInterface.dll

REM Copy ProtoCore
copy %base%\ProtoCore.dll .\%ui%\lib\%framework%\ProtoCore.dll

REM Copy Prism
copy %base%\Microsoft.Practices.Prism.dll .\%ui%\lib\%framework%\Microsoft.Practices.Prism.dll

REM Copy test libraries.
copy %base%\TestServices.dll .\%zt%\lib\%framework%\TestServices.dll

REM Copy content
copy ..\..\src\Libraries\Samples\SampleLibraryZeroTouch\HelloDynamoZeroTouch.cs .\%zt%\content\HelloDynamoZeroTouch.cs
copy ..\..\src\Libraries\Samples\SampleLibraryZeroTouch\HelloDynamoZeroTouchTests.cs .\%zt%\content\HelloDynamoZeroTouchTests.cs 

copy ..\..\src\Libraries\Samples\SampleLibraryUI\HelloDynamo.cs .\%ui%\content\HelloDynamo.cs
copy ..\..\src\Libraries\Samples\SampleLibraryUI\HelloDynamoControl.xaml .\%ui%\content\HelloDynamoControl.xaml
copy ..\..\src\Libraries\Samples\SampleLibraryUI\HelloDynamoControl.xaml.cs .\%ui%\content\HelloDynamoControl.xaml.cs

nuget pack .\%zt%\ZeroTouchLibrary.nuspec
nuget pack .\%ui%\UILibrary.nuspec