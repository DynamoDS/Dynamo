xcopy "..\bin\Debug\*.dll" "..\DynamoWIPInstall\bin" /Y
xcopy "..\definitions\*.*" "..\DynamoWIPInstall\definitions" /S /I /Y /R
xcopy "..\Samples\*.*" "..\DynamoWIPInstall\Samples" /S /I /Y /R
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" "..\DynamoWIPInstall\DynamoForVasari_WIP.iss"
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" "..\DynamoWIPInstall\DynamoForRevit_2013_WIP.iss"
