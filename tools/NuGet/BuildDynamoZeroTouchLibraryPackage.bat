SET base=..\..\bin\AnyCPU\Release
SET framework=net45

SET zt=DynamoVisualProgramming.ZeroTouchLibrary

rmdir /s /q .\%zt%\build
rmdir /s /q .\%zt%\content
rmdir /s /q .\%zt%\lib
rmdir /s /q .\%zt%\tools

mkdir .\%zt%\build\net45
mkdir .\%zt%\content
mkdir .\%zt%\lib\net45
mkdir .\%zt%\tools

copy %base%\ProtoGeometry.dll .\%zt%\lib\%framework%\ProtoGeometry.dll
copy %base%\ProtoGeometry.xml .\%zt%\build\%framework%\ProtoGeometry.xml
copy %base%\DynamoUnits.dll .\%zt%\lib\%framework%\DynamoUnits.dll

nuget pack .\%zt%\ZeroTouchLibrary.nuspec