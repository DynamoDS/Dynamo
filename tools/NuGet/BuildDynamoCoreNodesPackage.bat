SET base=..\..\bin\AnyCPU\Release
SET framework=net45

SET dcn=DynamoVisualProgramming.DynamoCoreNodes

rmdir /s /q .\%dcn%\build
rmdir /s /q .\%dcn%\content
rmdir /s /q .\%dcn%\lib
rmdir /s /q .\%dcn%\tools

mkdir .\%dcn%\build\net45
mkdir .\%dcn%\content
mkdir .\%dcn%\lib\net45
mkdir .\%dcn%\tools

copy %base%\Display.dll .\%dcn%\lib\%framework%\Display.dll
copy %base%\Display.xml .\%dcn%\build\%framework%\Display.xml
copy %base%\DSCoreNodes.dll .\%dcn%\lib\%framework%\DSCoreNodes.dll
copy %base%\DSCoreNodes.xml .\%dcn%\build\%framework%\DSCoreNodes.xml

nuget pack .\%dcn%\DynamoVisualProgramming.DynamoCoreNodes.nuspec