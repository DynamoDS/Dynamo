RMDIR "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Release\definitions" /S /Q
RMDIR ""C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Release\Samples" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_VasBeta1_2013\definitions\*.*" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Release\definitions" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_VasBeta1_2013\Samples\*.*" "C:\xfer\dev\dynamo_VasBeta1_2013f\DynamoRevit\bin\Release\Samples" /S /I /Y /R

copy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Release\DynamoRevit.dll" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\DynamoRevit.dll"
copy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Release\DynamoElements.dll" C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\DynamoElements.dll"
copy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Release\DragCanvas.dll" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\DragCanvas.dll"
copy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Release\FScheme.dll" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\FScheme.dll"
copy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Release\FSchemeInterop.dll" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\FSchemeInterop.dll"

xcopy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\definitions\*.*" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\definitions" /S /I /Y /R
xcopy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\Samples\*.*" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\Samples" /S /I /Y /R
iscc.exe "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\DynamoForVasari_3-0_WIP.iss"
iscc.exe "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\DynamoForRevit_2013_WIP.iss"
