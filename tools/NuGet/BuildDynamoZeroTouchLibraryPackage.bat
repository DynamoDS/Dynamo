SET base=..\..\bin\AnyCPU\Debug
SET framework=net40

SET zt=DynamoVisualProgramming.ZeroTouchLibrary

rmdir /s /q .\%zt%\build
rmdir /s /q .\%zt%\content
rmdir /s /q .\%zt%\lib
rmdir /s /q .\%zt%\tools

mkdir .\%zt%\build
mkdir .\%zt%\content
mkdir .\%zt%\lib\net40
mkdir .\%zt%\tools

copy %base%\ProtoGeometry.dll .\%zt%\lib\%framework%\ProtoGeometry.dll
copy %base%\ProtoInterface.dll .\%zt%\lib\%framework%\ProtoInterface.dll
copy %base%\DynamoServices.dll .\%zt%\lib\%framework%\DynamoServices.dll

nuget pack .\%zt%\ZeroTouchLibrary.nuspec