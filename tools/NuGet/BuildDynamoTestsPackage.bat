SET base=..\..\bin\AnyCPU\Debug
SET framework=net40

SET test=DynamoVisualProgramming.Tests

rmdir /s /q .\%test%\build
rmdir /s /q .\%test%\content
rmdir /s /q .\%test%\lib
rmdir /s /q .\%test%\tools

mkdir .\%test%\build
mkdir .\%test%\content
mkdir .\%test%\lib\net40
mkdir .\%test%\tools

copy %base%\DynamoCore.dll .\%test%\lib\%framework%\DynamoCore.dll
copy %base%\DynamoUtilities.dll .\%test%\lib\%framework%\DynamoUtilities.dll
copy %base%\ProtoGeometry.dll .\%test%\lib\%framework%\ProtoGeometry.dll
copy %base%\ProtoInterface.dll .\%test%\lib\%framework%\ProtoInterface.dll
copy %base%\ProtoCore.dll .\%test%\lib\%framework%\ProtoCore.dll
copy %base%\TestServices.dll .\%test%\lib\%framework%\TestServices.dll
copy %base%\SystemTestServices.dll .\%test%\lib\%framework%\SystemTestServices.dll
copy %base%\DynamoShapeManager.dll .\%test%\lib\%framework%\DynamoShapeManager.dll

copy ..\..\src\Libraries\Samples\SampleLibraryTests\TestServices.dll.config .\%test%\content\TestServices.dll.config

nuget pack .\%test%\Tests.nuspec