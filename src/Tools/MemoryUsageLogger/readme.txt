MemoryUsageLogger
================================================================================
    
This simple tool starts a .net managed application, does sampling at 100ms 
interval and prints out the total managed heap size of remote application. 

Note the total managed heap size doesn't accurately reflect the real memory 
usage because this value is updated when CLR triggers garbage collection on 
the target application. 

Usage:
    memlog.exe remote-application [argument-to-remote-application]

For example,

    memlog.exe c:\project\dynamo\bin\DynamoSandbox.exe > memorylog.txt

    memlog.exe c:\nunit\bin\nunit-console.exe c:\project\dynamo\bin\ProtoTest.dll
