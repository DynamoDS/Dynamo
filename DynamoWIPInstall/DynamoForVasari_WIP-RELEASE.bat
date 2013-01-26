RMDIR "..\DynamoRevit\bin\Release\definitions" /S /Q
RMDIR "..\DynamoRevit\bin\Debug\Samples" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_VasBeta1_2013\definitions\*.*" "..\DynamoRevit\bin\Debug\definitions" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_VasBeta1_2013\Samples\*.*" "C:\xfer\dev\dynamo_VasBeta1_2013f\DynamoRevit\bin\Debug\Samples" /S /I /Y /R

copy "..\DynamoRevit\bin\Debug\DynamoRevit.dll" "..\DynamoWIPInstall\DynamoRevit.dll"
copy "..\DynamoRevit\bin\Debug\DynamoElements.dll" ..\DynamoWIPInstall\DynamoElements.dll"
copy "..\DynamoRevit\bin\Debug\DragCanvas.dll" "..\DynamoWIPInstall\DragCanvas.dll"
copy "..\DynamoRevit\bin\Debug\FScheme.dll" "..\DynamoWIPInstall\FScheme.dll"
copy "..\DynamoRevit\bin\Debug\FSchemeInterop.dll" "..\DynamoWIPInstall\FSchemeInterop.dll"
copy "..\packages\miconvexhull_3e3d8e61febb\lib\MIConvexHullPlugin.dll" "..\DynamoWIPInstall\MIConvexHullPlugin.dll"
copy "..\packages\Helix3D\NET40\HelixToolkit.Wpf.dll" "..\DynamoWIPInstall\HelixToolkit.Wpf.dll"
copy "..\packages\kinect\Microsoft.Kinect.dll" "..\DynamoWIPInstall\Microsoft.Kinect.dll"


xcopy "..\definitions\*.*" "..\DynamoWIPInstall\definitions" /S /I /Y /R
xcopy "..\Samples\*.*" "..\DynamoWIPInstall\Samples" /S /I /Y /R
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" "..\DynamoWIPInstall\DynamoForVasari_WIP.iss"
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" "..\DynamoWIPInstall\DynamoForRevit_2013_WIP.iss"
