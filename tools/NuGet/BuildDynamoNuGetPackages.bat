SET base=..\..\bin\AnyCPU\Debug
SET framework=net40
SET zt=DynamoVisualProgramming.ZeroTouchLibrary
SET ui=DynamoVisualProgramming.UILibrary
SET test=DynamoVisualProgramming.Tests

rmdir /s /q .\%zt%\build
rmdir /s /q .\%zt%\content
rmdir /s /q .\%zt%\lib
rmdir /s /q .\%zt%\tools

rmdir /s /q .\%ui%\build
rmdir /s /q .\%ui%\content
rmdir /s /q .\%ui%\lib
rmdir /s /q .\%ui%\tools

rmdir /s /q .\%test%\build
rmdir /s /q .\%test%\content
rmdir /s /q .\%test%\lib
rmdir /s /q .\%test%\tools

mkdir .\%zt%\build
mkdir .\%zt%\content
mkdir .\%zt%\lib\net40
mkdir .\%zt%\tools

mkdir .\%ui%\build
mkdir .\%ui%\content
mkdir .\%ui%\lib\net40
mkdir .\%ui%\tools

mkdir .\%test%\build
mkdir .\%test%\content
mkdir .\%test%\lib\net40
mkdir .\%test%\tools

REM Copy core
copy %base%\DynamoCore.dll .\%ui%\lib\%framework%\DynamoCore.dll
copy %base%\DynamoCore.dll .\%test%\lib\%framework%\DynamoCore.dll

REM Copy Dynamo utilities
copy %base%\DynamoUtilities.dll .\%test%\lib\%framework%\DynamoUtilities.dll

REM Copy ProtoGeometry
copy %base%\ProtoGeometry.dll .\%zt%\lib\%framework%\ProtoGeometry.dll
copy %base%\ProtoGeometry.dll .\%test%\lib\%framework%\ProtoGeometry.dll
copy %base%\ProtoInterface.dll .\%zt%\lib\%framework%\ProtoInterface.dll
copy %base%\ProtoInterface.dll .\%ui%\lib\%framework%\ProtoInterface.dll
copy %base%\ProtoInterface.dll .\%test%\lib\%framework%\ProtoInterface.dll

REM Copy ProtoCore
copy %base%\ProtoCore.dll .\%ui%\lib\%framework%\ProtoCore.dll
copy %base%\ProtoCore.dll .\%test%\lib\%framework%\ProtoCore.dll

REM Copy Prism
copy %base%\Microsoft.Practices.Prism.dll .\%ui%\lib\%framework%\Microsoft.Practices.Prism.dll
copy %base%\Microsoft.Practices.Prism.dll .\%test%\lib\%framework%\Microsoft.Practices.Prism.dll

REM Copy test libraries.
copy %base%\TestServices.dll .\%test%\lib\%framework%\TestServices.dll
copy %base%\SystemTestServices.dll .\%test%\lib\%framework%\SystemTestServices.dll

REM Copy NUnit
copy %base%\nunit.framework.dll .\%test%\lib\%framework%\nunit.framework.dll

REM Copy content
copy ..\..\src\Libraries\Samples\SampleLibraryZeroTouch\HelloDynamoZeroTouch.cs .\%zt%\content\HelloDynamoZeroTouch.cs
copy ..\..\src\Libraries\Samples\SampleLibraryZeroTouch\HelloDynamoZeroTouchTests.cs .\%zt%\content\HelloDynamoZeroTouchTests.cs 

copy ..\..\src\Libraries\Samples\SampleLibraryUI\HelloDynamo.cs .\%ui%\content\HelloDynamo.cs
copy ..\..\src\Libraries\Samples\SampleLibraryUI\HelloDynamoControl.xaml .\%ui%\content\HelloDynamoControl.xaml
copy ..\..\src\Libraries\Samples\SampleLibraryUI\HelloDynamoControl.xaml.cs .\%ui%\content\HelloDynamoControl.xaml.cs

copy ..\..\src\Libraries\Samples\SampleLibraryTests\TestServices.dll.config .\%test%\content\TestServices.dll.config

nuget pack .\%zt%\ZeroTouchLibrary.nuspec
nuget pack .\%ui%\UILibrary.nuspec
nuget pack .\%test%\Tests.nuspec