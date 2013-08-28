SET cwd=%0\..
echo %cwd%
robocopy %cwd%\..\..\bin\Release %cwd%\temp\bin *.dll -XF *Tests.dll
robocopy %cwd%\..\..\bin\Release %cwd%\temp\bin *.exe
robocopy %cwd%\..\..\ %cwd%\Extra README.md
cd %cwd%\Extra
del README.txt
rename README.md README.txt
cd ..
robocopy %cwd%\..\..\doc\distrib\definitions %cwd%\temp\definitions /s
robocopy %cwd%\..\..\doc\distrib\Samples %cwd%\temp\Samples /s
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" %cwd%\DynamoInstaller.iss
rmdir /Q /S %cwd%\temp