SET cwd=%0\..
echo %cwd%

set OPT_CONFIGURATION=Release
IF /I "%1"=="Debug" set OPT_CONFIGURATION=Debug

set OPT_Platform=AnyCPU
IF /I "%2"=="x64" set OPT_Platform=x64
IF /I "%2"=="x86" set OPT_Platform=x86

robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION% %cwd%\temp\bin *.exe *.dll *.xml *.config *.cer -XF *Tests.dll

IF EXIST %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\Revit_2014 (
	robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\Revit_2014 %cwd%\temp\bin\Revit_2014 *.dll *.xml *.config -XF *Tests.dll -XD int /e
)

IF EXIST %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\Revit_2015 (
	robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\Revit_2015 %cwd%\temp\bin\Revit_2015 *.dll *.xml *.config -XF *Tests.dll -XD int /e
)
IF EXIST %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\Revit_2016 (
	robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\Revit_2016 %cwd%\temp\bin\Revit_2016 *.dll *.xml *.config -XF *Tests.dll -XD int /e
)


robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\nodes %cwd%\temp\bin\nodes *.dll *.xml /e
robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\UI %cwd%\temp\bin\UI /E
copy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\FunctionObject.ds %cwd%\temp\bin\FunctionObject.ds
copy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\DSCoreNodes_DynamoCustomization.xml %cwd%\temp\bin\DSCoreNodes_DynamoCustomization.xml
copy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\ProtoGeometry_DynamoCustomization.xml %cwd%\temp\bin\ProtoGeometry_DynamoCustomization.xml

robocopy %cwd%\..\..\extern\LibG_219 %cwd%\temp\bin\LibG_219
robocopy %cwd%\..\..\extern\LibG_220 %cwd%\temp\bin\LibG_220
robocopy %cwd%\..\..\extern\LibG_221 %cwd%\temp\bin\LibG_221

SET PATH=%PATH%;%cwd%\..\..\src\Tools\XmlDocumentationsUtility\bin\%OPT_CONFIGURATION%
echo %cwd%
XmlDocumentationsUtility.exe %cwd%\..\..

REM Localized resource assemblies
for %%L in (en-US, de-DE, ja-JP) do (
    robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\%%L %cwd%\temp\bin\%%L License.rtf
    robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\%%L %cwd%\temp\bin\%%L *.dll *.xml
)

set OPT_Language=en-US
robocopy %cwd%\..\..\ %cwd%\temp\bin\%OPT_Language% README.md
pushd %cwd%\temp\bin\%OPT_Language%\
rename README.md README.txt
popd 

robocopy %cwd%\..\..\doc\distrib\migration_nodes %cwd%\temp\definitions /e
robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\samples %cwd%\temp\samples /s

"C:\Program Files (x86)\Inno Setup 5\iscc.exe" %cwd%\DynamoInstaller.iss
rmdir /Q /S %cwd%\temp

