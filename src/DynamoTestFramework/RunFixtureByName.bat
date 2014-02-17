set arg=%1
set arg=%arg:"=%
start /wait python ..\..\src\DynamoTestFramework\RunRevitTests.py -i DynamoRevitTests.xml -r %arg%Results.xml -f %arg%
start %arg%Results.xml
