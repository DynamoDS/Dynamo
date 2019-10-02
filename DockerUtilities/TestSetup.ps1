<#
   Date: 06/18/2019
   Purpose: Run the .dll tests using the NUnit Console Runner
#>
$ErrorActionPreference = "Stop"

# Dynamo's location
$DynamoRoot = "$env:WORKSPACE"

# Location of the .dll's
$ProjectDir = "$DynamoRoot\bin\AnyCPU\Release"

$NUnitDir = "C:\Program Files (x86)\NUnit.org\nunit-console\"
Set-Location -Path $NUnitDir

# Location of the NUnit Console
$NunitTool = "nunit3-console.exe"


# .dll's to test (*Test*.dll ?)
$DllList = Get-ChildItem $ProjectDir -Name -Include *Tests.dll | Get-Unique -AsString

$TestDlls = ""

foreach($Dll in $DllList)
{
    #$TestDlls += $ProjectDir + '\' + $Dll.Name + ' '
    $TestDlls += $ProjectDir + '\' + $Dll + ' '
}

# .xml file where NUnit will dump the test results
$OutputPath =  "$DynamoRoot\src\Tools\TransformXMLToCSVTool\XML\TestResults.xml"

# Arguments for NUnit to run in 'explore' mode
$NunitExploreArguments = $TestDlls + ' --where="cat != Failure and cat != BackwardIncompatible and cat != ExcelTest" --labels=Before --explore="' + $OutputPath + ';format=nunit2"'

#Run NUnit in explore mode for the configured .dll's and export the results to the file specified in $OutputPath
Start-Process -FilePath $NunitTool -ArgumentList $NunitExploreArguments -Wait

# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

# Location of the executable file which will convert the TestResults.xml file to a .csv containing specific information about the tests
$CSVToolPath = "$DynamoRoot\src\Tools\TransformXMLToCSVTool\TransformXMLToCSVTool.exe"

# Run the converter tool
Start-Process -FilePath $CSVToolPath -ArgumentList $OutputPath -Wait

# - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 