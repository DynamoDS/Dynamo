SET base=..\..\bin\AnyCPU\Debug
SET framework=net45

SET core=DynamoVisualProgramming.Core

rmdir /s /q .\%core%\build
rmdir /s /q .\%core%\content
rmdir /s /q .\%core%\lib
rmdir /s /q .\%core%\tools

mkdir .\%core%\build\net45
mkdir .\%core%\content
mkdir .\%core%\lib\net45
mkdir .\%core%\tools

copy %base%\DynamoCore.dll .\%core%\lib\%framework%\DynamoCore.dll
copy %base%\DynamoCore.xml .\%core%\build\%framework%\DynamoCore.xml

copy %base%\ProtoGeometry.dll .\%core%\lib\%framework%\ProtoGeometry.dll
copy %base%\ProtoGeometry.xml .\%core%\build\%framework%\ProtoGeometry.xml

copy %base%\ProtoInterface.dll .\%core%\lib\%framework%\ProtoInterface.dll
copy %base%\ProtoInterface.xml .\%core%\build\%framework%\ProtoInterface.xml

copy %base%\ProtoCore.dll .\%core%\lib\%framework%\ProtoCore.dll
copy %base%\DynamoShapeManager.dll .\%core%\lib\%framework%\DynamoShapeManager.dll
copy %base%\DynamoUtilities.dll .\%core%\lib\%framework%\DynamoUtilities.dll

nuget pack .\%core%\Core.nuspec