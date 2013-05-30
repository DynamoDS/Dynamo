robocopy ..\..\bin\Release temp\bin *.dll -XF *Tests.dll
robocopy ..\..\ Extra README.md
cd Extra
del README.txt
rename README.md README.txt
cd ..
robocopy ..\..\doc\distrib\definitions temp\definitions /s
robocopy ..\..\doc\distrib\Samples temp\Samples /s
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" "DynamoForVasari_WIP.iss"
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" "DynamoForRevit_2013_WIP.iss"
rmdir /Q /S temp