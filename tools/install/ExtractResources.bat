@echo off
set cwd=%~dp0
echo %cwd%

set OPT_CONFIGURATION=Release
IF /I "%1"=="Debug" set OPT_CONFIGURATION=Debug

set OPT_Platform=AnyCPU
IF /I "%2"=="x64" set OPT_Platform=x64
IF /I "%2"=="x86" set OPT_Platform=x86

set binroot=%cwd%..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%
set wwlroot=%binroot%\wwl

echo .
echo -------------------------------------------------------------------------------
echo BIN folder: %binroot%
echo WWL folder: %wwlroot%
echo -------------------------------------------------------------------------------
echo .
rd /s/q %wwlroot%
mkdir %wwlroot%

rem Copy localizable resources
robocopy "%binroot%\en-US"                    "%wwlroot%\en-US"                     *.resources.dll *.xml
robocopy "%binroot%\libg_locale\en_US"        "%wwlroot%\libg_locale\en_US"         /e
robocopy "%binroot%\nodes\en-US"              "%wwlroot%\nodes\en-US"               *.resources.dll *.xml
robocopy "%binroot%\samples\en-US"            "%wwlroot%\samples\en-US"             /e

rem Copy all accompanying main assemblies (due to build time dependencies, these are 
rem required for translation tool to even open up localizable resource assemblies above).
robocopy "%binroot%"                          "%wwlroot%"                           *.dll *.exe -XF *tests.dll *.customization.dll 
robocopy "%binroot%\nodes"                    "%wwlroot%\nodes"                     *.dll

IF EXIST "%binroot%\Revit_2014" (
robocopy "%binroot%\Revit_2014"               "%wwlroot%\Revit_2014"                *.dll *.exe -XF *tests.dll *.customization.dll 
robocopy "%binroot%\Revit_2014\en-US"         "%wwlroot%\Revit_2014\en-US"          *.resources.dll *.xml
robocopy "%binroot%\Revit_2014\nodes"         "%wwlroot%\Revit_2014\nodes"          *.dll
robocopy "%binroot%\Revit_2014\nodes\en-US"   "%wwlroot%\Revit_2014\nodes\en-US"    *.resources.dll *.xml
)

IF EXIST "%binroot%\Revit_2015" (
robocopy "%binroot%\Revit_2015"               "%wwlroot%\Revit_2015"                *.dll *.exe -XF *tests.dll *.customization.dll 
robocopy "%binroot%\Revit_2015\en-US"         "%wwlroot%\Revit_2015\en-US"          *.resources.dll *.xml
robocopy "%binroot%\Revit_2015\nodes"         "%wwlroot%\Revit_2015\nodes"          *.dll
robocopy "%binroot%\Revit_2015\nodes\en-US"   "%wwlroot%\Revit_2015\nodes\en-US"    *.resources.dll *.xml
)

IF EXIST "%binroot%\Revit_2016" (
robocopy "%binroot%\Revit_2016"               "%wwlroot%\Revit_2016"                *.dll *.exe -XF *tests.dll *.customization.dll 
robocopy "%binroot%\Revit_2016\en-US"         "%wwlroot%\Revit_2016\en-US"          *.resources.dll *.xml
robocopy "%binroot%\Revit_2016\nodes"         "%wwlroot%\Revit_2016\nodes"          *.dll
robocopy "%binroot%\Revit_2016\nodes\en-US"   "%wwlroot%\Revit_2016\nodes\en-US"    *.resources.dll *.xml
)
