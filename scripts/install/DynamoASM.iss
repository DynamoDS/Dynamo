[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{C2FB73FE-0312-468C-AFE6-EBB92F9B3D7E}
AppName=ASM for Dynamo
AppPublisher=Autodesk, Inc.
AppVersion=1.0
AppVerName=ASM for Dynamo 1.0
AppPublisherURL=http://www.autodesk.com/
DefaultDirName=.\
OutputDir=Extra
OutputBaseFilename=InstallASMForDynamo
Compression=lzma
SolidCompression=yes
ShowLanguageDialog=auto
LicenseFile=.\extra\DynamoASMLicense.txt
DirExistsWarning=no

[Files]
Source: "..\..\extern\DynamoASM\*"; DestDir: {app}\dll; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

