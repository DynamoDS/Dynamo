call .\BuildPackages.bat "template-artifactory"

set Artifactory=https://art-bobcat.autodesk.com/artifactory/api/nuget/team-dynamo-nuget

:: Use credentials in config file for pushing package to Artifactory
set configPath=%~dp0..\..\dynamo-nuget.config
nuget push *.nupkg -source %Artifactory% -configfile %configPath%
