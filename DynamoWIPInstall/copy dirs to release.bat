RMDIR "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Release\definitions" /S /Q
RMDIR ""C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Release\Samples" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_tatlin4\definitions\*.*" "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Release\definitions" /S /I /Y /R
xcopy "C:\xfer\dev\dynamo_tatlin4\Samples\*.*" "C:\xfer\dev\dynamo_tatlin4\DynamoRevit\bin\Release\Samples" /S /I /Y /R