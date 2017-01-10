call .\BuildPackages.bat

SET Artifactory=https://art-bobcat.autodesk.com/artifactory/api/nuget/team-dynamo-nuget

:: Push these packages to www.nuget.org
nuget push DynamoVisualProgramming.Core.*.nupkg -apikey %1
nuget push DynamoVisualProgramming.DynamoCoreNodes.*.nupkg -apikey %1
nuget push DynamoVisualProgramming.DynamoServices.*.nupkg -apikey %1
nuget push DynamoVisualProgramming.Tests.*.nupkg -apikey %1
nuget push DynamoVisualProgramming.WpfUILibrary.*.nupkg -apikey %1
nuget push DynamoVisualProgramming.ZeroTouchLibrary.*.nupkg -apikey %1

:: Push Runtime package to Artifactory
nuget push DynamoVisualProgramming.DynamoCoreRuntime.*.nupkg -source %Artifactory% -apikey %2
