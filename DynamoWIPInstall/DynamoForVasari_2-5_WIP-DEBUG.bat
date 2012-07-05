RMDIR "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Debug\definitions" /S /Q
RMDIR ""C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Debug\Samples" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_tatlin4\definitions\*.*" "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Debug\definitions" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_tatlin4\Samples\*.*" "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Debug\Samples" /S /I /Y /R

copy "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Debug\DynamoRevit.dll" "C:\xfer\dev\dynamo_tatlin4\DynamoWIPInstall\DynamoRevit.dll"
copy "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Debug\DynamoElements.dll" C:\xfer\dev\dynamo_tatlin4\DynamoWIPInstall\DynamoElements.dll"
copy "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Debug\DragCanvas.dll" "C:\xfer\dev\dynamo_tatlin4\DynamoWIPInstall\DragCanvas.dll"
copy "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Debug\FScheme.dll" "C:\xfer\dev\dynamo_tatlin4\DynamoWIPInstall\FScheme.dll"
copy "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Debug\FSchemeInterop.dll" "C:\xfer\dev\dynamo_tatlin4\DynamoWIPInstall\FSchemeInterop.dll"

xcopy "C:\xfer\dev\dynamo_tatlin4\definitions\*.*" "C:\xfer\dev\dynamo_tatlin4\DynamoWIPInstall\definitions" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_tatlin4\Samples\*.*" "C:\xfer\dev\dynamo_tatlin4\DynamoWIPInstall\Samples" /S /I /Y /R
iscc.exe "C:\xfer\dev\dynamo_tatlin4\DynamoWIPInstall\DynamoForVasari_2-5_WIP.iss"
iscc.exe "C:\xfer\dev\dynamo_tatlin4\DynamoWIPInstall\Revit_2012_WIP_Dynamo_Add-In.iss"