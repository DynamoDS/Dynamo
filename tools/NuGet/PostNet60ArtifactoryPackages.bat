:: Use credentials in config file for pushing package to Artifactory
set configPath=%~dp0..\..\dynamo-nuget.config
nuget push *.nupkg -source team-dynamo-nuget -configfile %configPath%
