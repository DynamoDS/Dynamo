RMDIR "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Debug\definitions" /S /Q
RMDIR ""C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Debug\Samples" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_tatlin4\definitions\*.*" "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Debug\definitions" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_tatlin4\Samples\*.*" "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Debug\Samples" /S /I /Y /R