
if NOT EXIST Nunitresults mkdir Nunitresults

REM first argument - dll to run Nunit with 
REM second arg - name of the xml to store 
REM third arg -  name of the jenkins project 
REM fourth arg - project Description
REM fifth arg -  %BUILD_NUMBER% - build number from jenkins master 
REM sixth arg - %ARCHIVE_BIN% - the path from where to get the test dll
REM seven arg - %DEFAULT_RECIPIENTS% - 
REM get the build number of last succesful build for job DesignScriptMaster

set  buildnumber=%5


REM read the build number 
set DEFAULT_RECIPIENTS=%DEFAULT_RECIPIENTS%
set ARCHIVE_BIN=%6
set /p buildversion=<%ARCHIVE_BIN%\Bundle\DesignScript.bundle\Contents\Win64\appversion.txt
echo %buildversion% >buildversion.txt



REM delete old file

REM run nunit 
if not exist %RESULT_PATH% mkdir %RESULT_PATH%
if not exist %RESULT_PATH%\%buildnumber% mkdir %RESULT_PATH%\%buildnumber%


set Scripts=E:\DesignScript\Workspace\DesignScript\Prototype\Scripts
set linkScript=E:\Archive\DesignScriptMaster\binaries\%buildnumber%\Scripts
set testfiles=E:\DesignScript\Workspace\DesignScript\Studio
set linkfiles=E:\Archive\DesignScriptMaster\binaries\Studio


if "%4" == "ProtoScript" (
	echo %linkScript%
	echo %Scripts%
 	if not exist %linkScript% mklink /D %linkScript% %Scripts%
)

if "%4" == "DesignScriptStudio" (
	echo %testfiles%
	echo %linkfiles%
	if not exist %linkfiles% mklink /D %linkfiles% %testfiles%
)



%NUNITEXE_PATH% %ARCHIVE_BIN%\%1 /xml=%2

copy %2 %RESULT_PATH%\%buildnumber%
copy buildversion.txt %RESULT_PATH%\%5

E:\DesignScript\BuildResources\curl-7.23.1-win64-ssl-sspi\curl http://10.35.235.87:8080/job/%3/lastSuccessfulBuild/buildNumber >build.txt

set /p prebuild=<build.txt

echo %prebuild%
REM gather nunit summary report and send email



E:\DesignScript\buildresources\NUnitCI.exe "%RESULT_PATH%\%buildnumber%\buildversion.txt" "C:\Program Files (x86)\Jenkins\jobs\%3\builds\%prebuild%\archive\buildversion.txt" "E:\DesignScript\Workspace\DesignScript" "%4" "%RESULT_PATH%\%buildnumber%\%2" "C:\Program Files (x86)\Jenkins\jobs\%3\builds\%prebuild%\archive\%2" "%DEFAULT_RECIPIENTS%"
