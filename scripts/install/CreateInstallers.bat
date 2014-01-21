SET cwd=%0\..
echo %cwd%

set OPT_CONFIGURATION=Release
IF /I "%1"=="Debug" set OPT_CONFIGURATION=Debug

set OPT_Platform=AnyCPU
IF /I "%2"=="x64" set OPT_Platform=x64
IF /I "%2"=="x86" set OPT_Platform=x86

robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION% %cwd%\temp\bin *.dll *.config -XF *Tests.dll nunit*.dll
robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION% %cwd%\temp\bin *.exe
robocopy %cwd%\..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\dll %cwd%\temp\bin\dll
robocopy %cwd%\..\..\ %cwd%\Extra README.md
cd %cwd%\Extra
del README.txt
rename README.md README.txt
cd ..
robocopy %cwd%\..\..\doc\distrib\dynamo_packages %cwd%\temp\dynamo_packages /e
robocopy %cwd%\..\..\doc\distrib\Samples %cwd%\temp\Samples /s
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" %cwd%\DynamoInstaller.iss
rmdir /Q /S %cwd%\temp