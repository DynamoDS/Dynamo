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

set TextTransform=%COMMONPROGRAMFILES(x86)%\microsoft shared\TextTemplating\11.0\TextTransform.exe
IF NOT EXIST "%TextTransform%" set TextTransform=%COMMONPROGRAMFILES(x86)%\microsoft shared\TextTemplating\12.0\TextTransform.exe

set TextTransform=%COMMONPROGRAMFILES(x86)%\microsoft shared\TextTemplating\12.0\TextTransform.exe
IF NOT EXIST "%TextTransform%" set TextTransform=%COMMONPROGRAMFILES(x86)%\microsoft shared\TextTemplating\14.0\TextTransform.exe

:: transform all the templates
for /f %%d in (t4list.txt) do (
set file_name=%%d
set file_name=!file_name:~0,-3!.%extension%
echo:  \--^> !file_name!    
"%TextTransform%" -out !file_name! %%d
)

:: delete T4 list and return to previous directory
del t4list.txt
popd

echo transformation complete
