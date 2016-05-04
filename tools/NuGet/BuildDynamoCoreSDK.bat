SET base=..\..\bin\AnyCPU\Release
SET framework=net45

SET zt=DynamoVisualProgramming.DynamoCoreSDK

rmdir /s /q .\%zt%\build
rmdir /s /q .\%zt%\content
rmdir /s /q .\%zt%\lib
rmdir /s /q .\%zt%\tools

mkdir .\%zt%\build\net45
mkdir .\%zt%\content
mkdir .\%zt%\lib\net45
mkdir .\%zt%\tools

copy %base%\Analysis.dll .\%zt%\lib\%framework%\Analysis.dll
copy %base%\Analysis.xml .\%zt%\build\%framework%\Analysis.xml

copy %base%\DSCoreNodes.dll .\%zt%\lib\%framework%\DSCoreNodes.dll
copy %base%\DSCoreNodes.xml .\%zt%\build\%framework%\DSCoreNodes.xml

copy %base%\DynamoInstallDetective.dll .\%zt%\lib\%framework%\DynamoInstallDetective.dll

nuget pack .\%zt%\DynamoCoreSDK.nuspec