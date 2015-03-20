@echo off
set cwd=%~dp0
echo %cwd%

set OPT_CONFIGURATION=Release
IF /I "%1"=="Debug" set OPT_CONFIGURATION=Debug

set OPT_Platform=AnyCPU
IF /I "%2"=="x64" set OPT_Platform=x64
IF /I "%2"=="x86" set OPT_Platform=x86

set binroot=%cwd%..\..\bin\%OPT_Platform%\%OPT_CONFIGURATION%\
set wwlroot=%binroot%wwl\

echo .
echo -------------------------------------------------------------------------------
echo BIN folder: %binroot%
echo WWL folder: %wwlroot%
echo -------------------------------------------------------------------------------
echo .
rd /s/q %wwlroot%
mkdir %wwlroot%

robocopy "%binroot%\en-US"                    "%wwlroot%\en-US"                  *.resources.dll *.xml
robocopy "%binroot%\libg_locale\en_US"        "%wwlroot%\libg_locale\en_US"      /e
robocopy "%binroot%\nodes\en-US"              "%wwlroot%\nodes\en-US"            *.resources.dll *.xml
robocopy "%binroot%\samples\en-US"            "%wwlroot%\samples\en-US"          /e

IF EXIST "%binroot%\Revit_2014" (
robocopy "%binroot%\Revit_2014\en-US"         "%wwlroot%\Revit_2014\en-US"       *.resources.dll *.xml
robocopy "%binroot%\Revit_2014\nodes\en-US"   "%wwlroot%\Revit_2014\nodes\en-US" *.resources.dll *.xml
)

IF EXIST "%binroot%\Revit_2015" (
robocopy "%binroot%\Revit_2015\en-US"         "%wwlroot%\Revit_2015\en-US"       *.resources.dll *.xml
robocopy "%binroot%\Revit_2015\nodes\en-US"   "%wwlroot%\Revit_2015\nodes\en-US" *.resources.dll *.xml
)

IF EXIST "%binroot%\Revit_2016" (
robocopy "%binroot%\Revit_2016\en-US"         "%wwlroot%\Revit_2016\en-US"       *.resources.dll *.xml
robocopy "%binroot%\Revit_2016\nodes\en-US"   "%wwlroot%\Revit_2016\nodes\en-US" *.resources.dll *.xml
)
