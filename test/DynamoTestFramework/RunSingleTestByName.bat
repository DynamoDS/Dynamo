set arg1=%1
set arg2=%2
set arg1=%arg1:"=%
set arg2=%arg2:"=%

start /wait python ..\..\DynamoTestFramework\RunRevitTests.py -i %arg1% -r %arg2%Result.xml -n %arg2% -d true
start %arg2%Result.xml
