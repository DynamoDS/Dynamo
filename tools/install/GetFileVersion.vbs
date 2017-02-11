Set arguments = WScript.Arguments
Set fileSysObj = CreateObject("Scripting.FileSystemObject")
fileVersion = fileSysObj.GetFileVersion(arguments(0))
versions = Split(fileVersion, ".", -1, 1)
Wscript.Echo versions(0)
Wscript.Echo versions(1)
Wscript.Echo versions(2)
Wscript.Echo versions(3)
Wscript.Quit