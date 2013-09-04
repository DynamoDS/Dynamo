@echo off 

call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat" x86

set OPT_CONFIGURATION=Debug
IF /I "%1"=="Release" set OPT_CONFIGURATION=Release
set OPT_Platform=%2

REM ################################################################################
REM Begin Machine specific configurations

set GIT_ROOT=%cd%\..\..
set NunitPath=%GIT_ROOT%\extern\NUnit
set OutputPath=%GIT_ROOT%\src\bin\%2\%OPT_CONFIGURATION%
set REVITAPI=C:\Program Files\Autodesk\Revit Architecture 2014

REM End Machine specific configurations
REM ################################################################################
REM build code
cd %GIT_ROOT%/src
msbuild Dynamo.sln /p:Platform="Any CPU" /p:Configuration=Debug
REM ################################################################################


