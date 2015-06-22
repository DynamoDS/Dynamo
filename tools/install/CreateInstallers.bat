SET cwd=%0\..
echo %cwd%

set OPT_CONFIGURATION=Release
IF /I "%1"=="Debug" set OPT_CONFIGURATION=Debug

set OPT_Platform=AnyCPU
IF /I "%2"=="x64" set OPT_Platform=x64
IF /I "%2"=="x86" set OPT_Platform=x86

robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION% %cwd%\temp\bin *.rtf README.txt
robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION% %cwd%\temp\bin *.exe *.dll *.xml *.config *.cer *.ds -XF *Tests.dll

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
copy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\DSCoreNodes_DynamoCustomization.xml %cwd%\temp\bin\DSCoreNodes_DynamoCustomization.xml
copy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\ProtoGeometry_DynamoCustomization.xml %cwd%\temp\bin\ProtoGeometry_DynamoCustomization.xml

robocopy %cwd%\..\..\extern\LibG_219 %cwd%\temp\bin\LibG_219
robocopy %cwd%\..\..\extern\LibG_220 %cwd%\temp\bin\LibG_220
robocopy %cwd%\..\..\extern\LibG_221 %cwd%\temp\bin\LibG_221
robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\LibG_locale %cwd%\temp\bin\LibG_locale /e *.po *.mo

SET PATH=%PATH%;%cwd%\..\..\src\Tools\XmlDocumentationsUtility\bin\%OPT_CONFIGURATION%
echo %cwd%
XmlDocumentationsUtility.exe %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\

REM Localized resource assemblies
for %%L in (cs-CZ, de-DE, en-US, es-ES, fr-FR, it-IT, ja-JP, ko-KR, pl-PL, pt-BR, ru-RU, zh-CN, zh-TW) do (
    robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\%%L %cwd%\temp\bin\lang\%%L *.dll *.xml /e
)

robocopy %cwd%\..\..\doc\distrib\migration_nodes %cwd%\temp\definitions /e
robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\samples %cwd%\temp\samples /s

robocopy %cwd%\Extra\DirectX %cwd%\temp\DirectX

robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\gallery %cwd%\temp\gallery /s

"C:\Program Files (x86)\Inno Setup 5\iscc.exe" %cwd%\DynamoInstaller.iss
rmdir /Q /S %cwd%\temp