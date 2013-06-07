[Setup]
AppName=Dynamo
AppVerName=Dynamo WIP 0.4.0 for Vasari Beta 2 
AppPublisher=Autodesk, Inc.
AppID={{5F61EC1C-39EF-4E21-80DA-55621BB20B2A}
AppCopyright=
AppPublisherURL=http://labs.autodesk.com/utilities/vasari
AppSupportURL=
AppUpdatesURL=
AppVersion=0.3.4
VersionInfoVersion=0.3.4
VersionInfoCompany=Autodesk
VersionInfoDescription=Dynamo WIP for Vasari Beta 2
VersionInfoTextVersion=Dynamo WIP for Vasari Beta 2
VersionInfoCopyright=
DefaultDirName=C:/Autodesk/Vasari/DynamoWIP
DefaultGroupName=
OutputDir=Installers
OutputBaseFilename=DynamoWIPForVasari
SetupIconFile=Extra\Nodes_32_32.ico
Compression=lzma
SolidCompression=true
RestartIfNeededByRun=false
FlatComponentsList=false
ShowLanguageDialog=auto
DirExistsWarning=no
UninstallFilesDir={app}\Uninstall
UninstallDisplayIcon={app}\Nodes_32_32.ico
UninstallDisplayName=Dynamo WIP 0.4.0 for Vasari Beta 2
PrivilegesRequired=none
UsePreviousAppDir=no

[Types]
Name: "full"; Description: "Full installation"
Name: "compact"; Description: "Compact installation"
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Dirs]
Name: "{app}\definitions"
Name: "{app}\samples"

[Components]
Name: "DynamoForVasariWIP"; Description: "Dynamo WIP for Vasari Beta 2"; Types: full compact custom; Flags: fixed
Name: "DynamoTrainingFiles"; Description: "Dynamo Training Files"; Types: full

[Files]
;Core Files
Source: temp\bin\*; DestDir: {app}; Flags: ignoreversion overwritereadonly; Components: DynamoForVasariWIP
Source: Extra\Nodes_32_32.ico; DestDir: {app}; Flags: ignoreversion overwritereadonly; Components: DynamoForVasariWIP
Source: Extra\README.txt; DestDir: {app}; Flags: isreadme ignoreversion overwritereadonly; Components: DynamoForVasariWIP
Source: Extra\fsharp_redist.exe; DestDir: {app}; Flags: ignoreversion overwritereadonly; Components: DynamoForVasariWIP
Source: Extra\IronPython-2.7.3.msi; DestDir: {tmp}; Flags: deleteafterinstall;

;Training Files
Source: temp\samples\*.*; DestDir: {app}\samples; Flags: ignoreversion overwritereadonly recursesubdirs; Components: DynamoTrainingFiles
Source: temp\definitions\*.dyf; DestDir: {app}\definitions; Flags: ignoreversion overwritereadonly recursesubdirs; Components: DynamoTrainingFiles

[UninstallDelete]
Type: files; Name: "{userappdata}\Autodesk\Vasari\Addins\2013\Dynamo.addin"

[Run]
Filename: "{app}\fsharp_redist.exe"; Parameters: "/q"; Flags: runascurrentuser
Filename: "msiexec.exe"; Parameters: "/i ""{tmp}\IronPython-2.7.3.msi"" /qb"; WorkingDir: {tmp};

[Code]
{ HANDLE INSTALL PROCESS STEPS }

// added custom uninstall trigger based on http://stackoverflow.com/questions/2000296/innosetup-how-to-automatically-uninstall-previous-installed-version
/////////////////////////////////////////////////////////////////////
function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;


/////////////////////////////////////////////////////////////////////
function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
end;


/////////////////////////////////////////////////////////////////////
function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
// Return Values:
// 1 - uninstall string is empty
// 2 - error executing the UnInstallString
// 3 - successfully executed the UnInstallString

  // default return value
  Result := 0;

  // get the uninstall string of the old app
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  AddInFilePath: String;
  AddInFileContents: String;
begin
  if (CurStep=ssInstall) then
  begin
    if (IsUpgrade()) then
    begin
      UnInstallOldVersion();
    end;
  end;

  if CurStep = ssPostInstall then
  begin

	{ GET LOCATION OF USER AppData (Roaming) }
	AddInFilePath := ExpandConstant('{userappdata}\Autodesk\Vasari\Addins\2013\Dynamo.addin');

	{ CREATE NEW ADDIN FILE }
	AddInFileContents := '<?xml version="1.0" encoding="utf-8" standalone="no"?>' + #13#10;
	AddInFileContents := AddInFileContents + '<RevitAddIns>' + #13#10;
	AddInFileContents := AddInFileContents + '  <AddIn Type="Application">' + #13#10;
    AddInFileContents := AddInFileContents + '    <Name>Dynamo For Vasari</Name>' + #13#10;
	AddInFileContents := AddInFileContents + '    <Assembly>'  + ExpandConstant('{app}') + '\DynamoRevit.dll</Assembly>' + #13#10;
	AddInFileContents := AddInFileContents + '    <AddInId>188B9080-EEBE-40C3-865A-8FC31DEEC12F</AddInId>' + #13#10;
	AddInFileContents := AddInFileContents + '    <FullClassName>Dynamo.Applications.DynamoRevitApp</FullClassName>' + #13#10;
	AddInFileContents := AddInFileContents + '  <VendorId>ADSK</VendorId>' + #13#10;
	AddInFileContents := AddInFileContents + '  <VendorDescription>Autodesk, github.com/ikeough/dynamo</VendorDescription>' + #13#10;
	AddInFileContents := AddInFileContents + '  </AddIn>' + #13#10;
	AddInFileContents := AddInFileContents + '  <AddIn Type="Command">' + #13#10;
	AddInFileContents := AddInFileContents + '    <Assembly>'  + ExpandConstant('{app}') + '\DynamoRevit.dll</Assembly>' + #13#10;
	AddInFileContents := AddInFileContents + '    <AddInId>dc09be67-aa31-4ea7-86c9-d06c080cd3e9</AddInId>' + #13#10;
	AddInFileContents := AddInFileContents + '    <FullClassName>Dynamo.Applications.DynamoRevit</FullClassName>' + #13#10;
	AddInFileContents := AddInFileContents + '  <VendorId>ADSK</VendorId>' + #13#10;
	AddInFileContents := AddInFileContents + '  <VendorDescription>Autodesk, github.com/ikeough/dynamo</VendorDescription>' + #13#10;
    AddInFileContents := AddInFileContents + '    <Text>Dynamo For Vasari</Text>' + #13#10;
	AddInFileContents := AddInFileContents + '    <Description>Visual programming for Vasari.</Description>' + #13#10;
	AddInFileContents := AddInFileContents + '  </AddIn>' + #13#10;
    AddInFileContents := AddInFileContents + '</RevitAddIns>' + #13#10;
	SaveStringToFile(AddInFilePath, AddInFileContents, False);

  end;
end;




