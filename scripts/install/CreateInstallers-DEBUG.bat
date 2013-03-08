robocopy ..\bin\Debug bin *.dll
robocopy ..\definitions definitions
robocopy ..\Samples Samples
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" "..\DynamoWIPInstall\DynamoForVasari_WIP.iss"
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" "..\DynamoWIPInstall\DynamoForRevit_2013_WIP.iss"
