[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{C2FB73FE-0312-468C-AFE6-EBB92F9B3D7E}
AppName=Autodesk ASM for Dynamo
AppPublisher=Autodesk, Inc.
AppVersion=1.0
AppVerName=Autodesk ASM for Dynamo 1.0
AppPublisherURL=http://www.autodesk.com/
DefaultDirName={src}
OutputDir=Extra
OutputBaseFilename=InstallASMForDynamo
Compression=lzma
SolidCompression=yes
ShowLanguageDialog=auto
LicenseFile=.\extra\DynamoASMLicense.txt
DirExistsWarning=no
UsePreviousAppDir=no
Uninstallable=no

[Files]
Source: "..\..\extern\ASM\*"; DestDir: {src}\dll; Flags: ignoreversion 
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
