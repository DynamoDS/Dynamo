robocopy ..\..\bin\Release temp\bin *.dll
robocopy ..\..\doc\distrib\definitions temp\definitions /s
robocopy ..\..\doc\distrib\Samples temp\Samples /s
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" "DynamoForVasari_WIP.iss"
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" "DynamoForRevit_2013_WIP.iss"
rmdir /Q /S temp