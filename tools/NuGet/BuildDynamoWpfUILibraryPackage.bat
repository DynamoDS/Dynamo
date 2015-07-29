SET base=..\..\bin\AnyCPU\Debug
SET framework=net40

SET ui=DynamoVisualProgramming.WpfUILibrary

rmdir /s /q .\%ui%\build
rmdir /s /q .\%ui%\content
rmdir /s /q .\%ui%\lib
rmdir /s /q .\%ui%\tools

mkdir .\%ui%\build
mkdir .\%ui%\content
mkdir .\%ui%\lib\net40
mkdir .\%ui%\tools

copy %base%\DynamoCore.dll .\%ui%\lib\%framework%\DynamoCore.dll
copy %base%\DynamoCoreWpf.dll .\%ui%\lib\%framework%\DynamoCoreWpf.dll
copy %base%\ProtoCore.dll .\%ui%\lib\%framework%\ProtoCore.dll
copy %base%\nodes\DSCoreNodesUI.dll .\%ui%\lib\%framework%\DSCoreNodesUI.dll
copy %base%\nodes\CoreNodeModelsWpf.dll .\%ui%\lib\%framework%\CoreNodeModelsWpf.dll

nuget pack .\%ui%\WpfUILibrary.nuspec