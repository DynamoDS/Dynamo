:: Argument %1: API key to upload packages to www.nuget.org
::

call .\BuildPackages.bat "template-nuget"

:: Push these packages to www.nuget.org
nuget push *.nupkg -apikey %1
