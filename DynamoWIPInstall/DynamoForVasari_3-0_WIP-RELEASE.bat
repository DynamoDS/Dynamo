RMDIR "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoRevit\bin\Release\definitions" /S /Q
RMDIR ""C:\xfer\dev\dynamo_VasBeta1_2013\DynamoRevit\bin\Release\Samples" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_VasBeta1_2013\definitions\*.*" "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoRevit\bin\Release\definitions" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_VasBeta1_2013\Samples\*.*" "C:\xfer\dev\dynamo_VasBeta1_2013f\DynamoRevit\bin\Release\Samples" /S /I /Y /R

copy "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoRevit\bin\Release\DynamoRevit.dll" "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoWIPInstall\DynamoRevit.dll"
copy "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoRevit\bin\Release\DynamoElements.dll" C:\xfer\dev\dynamo_VasBeta1_2013\DynamoWIPInstall\DynamoElements.dll"
copy "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoRevit\bin\Release\DragCanvas.dll" "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoWIPInstall\DragCanvas.dll"
copy "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoRevit\bin\Release\FScheme.dll" "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoWIPInstall\FScheme.dll"
copy "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoRevit\bin\Release\FSchemeInterop.dll" "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoWIPInstall\FSchemeInterop.dll"

xcopy "C:\xfer\dev\dynamo_VasBeta1_2013\definitions\*.*" "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoWIPInstall\definitions" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_VasBeta1_2013\Samples\*.*" "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoWIPInstall\Samples" /S /I /Y /R
iscc.exe "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoWIPInstall\DynamoForVasari_3-0_WIP.iss"
iscc.exe "C:\xfer\dev\dynamo_VasBeta1_2013\DynamoWIPInstall\DynamoForRevit_2013_WIP.iss"
