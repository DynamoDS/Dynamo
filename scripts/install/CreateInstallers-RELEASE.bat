robocopy ..\..\bin\Release temp\bin *.dll -XF *Tests.dll
robocopy ..\..\ Extra README.md
cd Extra
del README.txt
rename README.md README.txt
cd ..
robocopy ..\..\doc\distrib\definitions temp\definitions /s
robocopy ..\..\doc\distrib\Samples temp\Samples /s
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" "DynamoInstaller.iss"
rmdir /Q /S temp