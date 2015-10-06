SET base=..\..\bin\AnyCPU\Debug
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

copy %base%\ProtoInterface.dll .\%zt%\lib\%framework%\ProtoInterface.dll
copy %base%\ProtoInterface.xml .\%zt%\build\%framework%\ProtoInterface.xml

copy %base%\DynamoServices.dll .\%zt%\lib\%framework%\DynamoServices.dll
copy %base%\DynamoServices.xml .\%zt%\build\%framework%\DynamoServices.xml

nuget pack .\%zt%\ZeroTouchLibrary.nuspec