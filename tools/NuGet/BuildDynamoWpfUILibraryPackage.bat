SET base=..\..\bin\AnyCPU\Release
SET framework=net45

SET ui=DynamoVisualProgramming.WpfUILibrary

rmdir /s /q .\%ui%\build
rmdir /s /q .\%ui%\content
rmdir /s /q .\%ui%\lib
rmdir /s /q .\%ui%\tools

mkdir .\%ui%\build\net45
mkdir .\%ui%\content
mkdir .\%ui%\lib\net45
mkdir .\%ui%\tools

copy %base%\DynamoCoreWpf.dll .\%ui%\lib\%framework%\DynamoCoreWpf.dll
copy %base%\DynamoCoreWpf.xml .\%ui%\build\%framework%\DynamoCoreWpf.xml

copy %base%\nodes\CoreNodeModels.dll .\%ui%\lib\%framework%\CoreNodeModels.dll
copy %base%\nodes\CoreNodeModels.xml .\%ui%\build\%framework%\CoreNodeModels.xml

copy %base%\nodes\CoreNodeModelsWpf.dll .\%ui%\lib\%framework%\CoreNodeModelsWpf.dll
copy %base%\nodes\CoreNodeModelsWpf.xml .\%ui%\build\%framework%\CoreNodeModelsWpf.xml

nuget pack .\%ui%\WpfUILibrary.nuspec