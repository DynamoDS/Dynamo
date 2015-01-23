SET branchName=%1
SET finalDir=%2

SET repo=%UserProfile%\Documents\GitHub\Dynamo
SET git=%UserProfile%\AppData\Local\GitHub\PORTAB~1\bin\git.exe
SET msbuild=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe
SET innosetup="C:\Program Files (x86)\Inno Setup 5\ISCC.exe"
SET oldDir= %cd%
SET installersDir=%repo%\tools\install\Installers

ECHO %oldDir%

chdir %repo%

REM Delete the bin folder
REM del %repo%\bin

REM Checkout and build Dynamo from the branch
%git% checkout %branchName%

REM Build Dynamo
%msbuild% .\src\Dynamo.All.2012.sln /t:clean,build /p:platform="Any CPU" /p:Configuration=Debug

REM Clear the installers folder
chdir %installersDir%
del *.exe

REM Create the installers
call %repo%\tools\install\CreateInstallers-Debug.bat

REM Rename the installer to include the branch name
chdir %installersDir%
rename InstallDynamo*.exe DynamoInstaller_Debug_%branchName%.exe

REM copy the installer to the output directory
xcopy /s %installersDir% %finalDir%

chdir %olddir%