set arg1=%1
set arg2=%2
set arg3=%3
set arg4=%4
set arg1=%arg1:"=%
set arg2=%arg2:"=%
set arg3=%arg3:"=%
set arg4=%arg4:"=%

start /wait DynamoTestFrameworkRunner.exe -a=%arg1% -t=%arg2% -r=%arg2%Result.xml -f=DSRevitNodesTests.%arg3% -d+ -dir=%arg4%
start %arg2%Result.xml
