@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

:: set the working dir (default to current dir)
set wdir=%cd%
if not (%1)==() set wdir=%1

:: set the file extension (default to vb)
set extension=cs
if not (%2)==() set extension=%2

echo executing transform_all from %wdir%
pushd %wdir%
:: create a list of all the T4 templates in the working dir
dir *.tt /b > t4list.txt

echo the following T4 templates will be transformed:
type t4list.txt

:: Use texttransform.exe from the IDE if it is present, otherwise, use legacy locations. 

:: clear text transform path to undefine it
set TEXTTRANSFORMPATH=

:: use latest vswhere utility to locate in IDE, where it resides now. 
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" (
   for /f "usebackq tokens=1* delims=: " %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -all`) do (
      if /i "%%i" == "installationPath" set TEXTTRANSFORMPATH="%%j\Common7\IDE\TextTransform.exe"
   )
)

:: not found in IDE, use legacy
IF NOT DEFINED TEXTTRANSFORMPATH (
   set TEXTTRANSFORMPATH="%COMMONPROGRAMFILES(x86)%\microsoft shared\TextTemplating\%VisualStudioVersion%\TextTransform.exe"
)

:: transform all the templates
for /f %%d in (t4list.txt) do (
set file_name=%%d
set file_name=!file_name:~0,-3!.%extension%
echo:  \--^> !file_name!
echo %TEXTTRANSFORMPATH% -out !file_name! %%d
    %TEXTTRANSFORMPATH% -out !file_name! %%d
)

:: delete T4 list and return to previous directory
del t4list.txt
popd

echo transformation complete
