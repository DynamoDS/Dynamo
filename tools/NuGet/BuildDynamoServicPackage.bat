SET base=..\..\bin\AnyCPU\Release
SET framework=net45

SET zt=DynamoVisualProgramming.DynamoServices

rmdir /s /q .\%zt%\build
rmdir /s /q .\%zt%\content
rmdir /s /q .\%zt%\lib
rmdir /s /q .\%zt%\tools

mkdir .\%zt%\build\net45
mkdir .\%zt%\content
mkdir .\%zt%\lib\net45
mkdir .\%zt%\tools

copy %base%\DynamoServices.dll .\%zt%\lib\%framework%\DynamoServices.dll
copy %base%\DynamoServices.xml .\%zt%\build\%framework%\DynamoServices.xml

nuget pack .\%zt%\DynamoServices.nuspec