SET base=..\..\bin\AnyCPU\Release
SET framework=net45

SET test=DynamoVisualProgramming.Tests

rmdir /s /q .\%test%\build
rmdir /s /q .\%test%\content
rmdir /s /q .\%test%\lib
rmdir /s /q .\%test%\tools

mkdir .\%test%\build\net45
mkdir .\%test%\content
mkdir .\%test%\lib\net45
mkdir .\%test%\tools

copy %base%\TestServices.dll .\%test%\lib\%framework%\TestServices.dll
copy %base%\SystemTestServices.dll .\%test%\lib\%framework%\SystemTestServices.dll

nuget pack .\%test%\Tests.nuspec