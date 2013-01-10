RMDIR "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Debug\definitions" /S /Q
RMDIR ""C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Debug\Samples" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_VasBeta1_2013\definitions\*.*" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Debug\definitions" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_VasBeta1_2013\Samples\*.*" "C:\xfer\dev\dynamo_VasBeta1_2013f\DynamoRevit\bin\Debug\Samples" /S /I /Y /R

copy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Debug\DynamoRevit.dll" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\DynamoRevit.dll"
copy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Debug\DynamoElements.dll" C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\DynamoElements.dll"
copy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Debug\DragCanvas.dll" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\DragCanvas.dll"
copy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Debug\FScheme.dll" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\FScheme.dll"
copy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Debug\FSchemeInterop.dll" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\FSchemeInterop.dll"
copy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\packages\miconvexhull_3e3d8e61febb\lib\MIConvexHullPlugin.dll" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\MIConvexHullPlugin.dll"
copy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoRevit\bin\Debug\HelixToolkit.Wpf.dll" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\HelixToolkit.Wpf.dll"


xcopy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\definitions\*.*" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\definitions" /S /I /Y /R
xcopy "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\Samples\*.*" "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\Samples" /S /I /Y /R
iscc.exe "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\DynamoForVasari_3-0_WIP.iss"
iscc.exe "C:\xfer\Github\3\4\dynamo_VasBeta1_2013_GHforW\DynamoWIPInstall\DynamoForRevit_2013_WIP.iss"
