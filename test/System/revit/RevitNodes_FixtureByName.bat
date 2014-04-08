set arg1=%1
set arg2=%2
set arg3=%3
set arg1=%arg1:"=%
set arg2=%arg2:"=%
set arg3=%arg3:"=%

start /wait DynamoTestFrameworkRunner.exe -a=%arg1% -r=%arg2%Result.xml -f=DSRevitNodesTests.%arg2% -d+ -dir=%arg3%
start %arg2%Result.xml
