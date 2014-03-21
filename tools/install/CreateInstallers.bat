SET cwd=%0\..
echo %cwd%

set OPT_CONFIGURATION=Release
IF /I "%1"=="Debug" set OPT_CONFIGURATION=Debug

set OPT_Platform=AnyCPU
IF /I "%2"=="x64" set OPT_Platform=x64
IF /I "%2"=="x86" set OPT_Platform=x86

robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION% %cwd%\temp\bin *.dll *.xml *.config -XF *Tests.dll
robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION% %cwd%\temp\bin License.rtf
robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION% %cwd%\temp\bin *.exe
robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\nodes %cwd%\temp\bin\nodes
copy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\FunctionObject.ds %cwd%\temp\bin\FunctionObject.ds
copy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\DSCoreNodes_DynamoCustomization.xml %cwd%\temp\bin\DSCoreNodes_DynamoCustomization.xml
copy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\ProtoGeometry_DynamoCustomization.xml %cwd%\temp\bin\ProtoGeometry_DynamoCustomization.xml
robocopy %cwd%\..\..\extern\LibG %cwd%\temp\bin\LibG
robocopy %cwd%\..\..\ %cwd%\Extra README.md
cd %cwd%\Extra
del README.txt
rename README.md README.txt
cd ..
robocopy %cwd%\..\..\doc\distrib\dynamo_packages %cwd%\temp\dynamo_packages /e
robocopy %cwd%\..\..\doc\distrib\Samples %cwd%\temp\Samples /s
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" %cwd%\DynamoASM.iss
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" %cwd%\DynamoInstaller.iss
rmdir /Q /S %cwd%\temp