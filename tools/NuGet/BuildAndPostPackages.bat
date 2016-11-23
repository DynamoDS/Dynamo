call .\BuildPackages.bat

nuget push -apikey %1 *.nupkg