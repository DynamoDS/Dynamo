Set arguments = WScript.Arguments
Set xmlDoc = CreateObject("Msxml2.DOMDocument")
xmlDoc.load(arguments(0))
Set dotnet = xmlDoc.selectSingleNode("/Project/PropertyGroup/DotNet")
Wscript.Echo dotnet.text
Wscript.Quit
