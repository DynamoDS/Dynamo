set arg=%1
set arg=%arg:"=%
start /wait python ..\..\src\DynamoTestFramework\RunRevitTests.py -i DSRevitNodesTests.xml -r %arg%Result.xml -n %arg%
start %arg%Result.xml
