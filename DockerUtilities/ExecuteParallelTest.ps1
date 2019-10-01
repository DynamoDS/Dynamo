<#
   Date: 07/04/2019
   Purpose: Parallel test
#>
$ErrorActionPreference = "Stop"

$NUnitDir = "C:\Program Files (x86)\NUnit.org\nunit-console\"
Set-Location -Path $NUnitDir

New-Item -Path "$env:WORKSPACE" -Name "TestResults" -ItemType "directory"

workflow RunTests_Parallel {
    param($Tests)

    # Dynamo's location
    $DynamoRoot = "$env:WORKSPACE"
    
    # Location of the .dll's
    $ProjectDir = "$DynamoRoot\bin\AnyCPU\Release"

    # Location of the NUnit Console
    $NunitTool = "nunit3-console.exe"

    foreach -parallel ($Test in $Tests){
        $AssemblyLocation = $ProjectDir + '\' + $Test.TestAssembly

        $ParallelExecutionArguments = $AssemblyLocation + ' --where="test=="' + $Test.TestNamespace + '" and cat != Failure and cat != BackwardIncompatible and cat != ExcelTest" --labels=Before --result="' + $DynamoRoot + '\TestResults\TestResult-' + $Test.TestClass + '.xml";format=nunit2'

        #Start an NUnit console instance for the current test
        Start-Process -FilePath $NunitTool -ArgumentList $ParallelExecutionArguments -Wait
    }
}

workflow _Wkf_StartCommands {

    $SlowPath = "$env:WORKSPACE\\src\\Tools\\TransformXMLToCSVTool\\Result\\textFileWithFiltersSlowTests.txt" 
    $FastPath = "$env:WORKSPACE\\src\\Tools\\TransformXMLToCSVTool\\Result\\textFileWithFiltersFastTests.txt"

    # Map the .csv file into an object we can work with
    $SlowTests = Import-Csv $SlowPath -Header 'TestClass', 'TestNamespace', 'TestAssembly'
    $FastTests = Import-Csv $FastPath -Header 'TestClass', 'TestNamespace', 'TestAssembly'

    parallel {
        RunTests_Parallel -Tests $FastTests
        RunTests_Parallel -Tests $SlowTests
    }    
}

$StopWatch = [System.Diagnostics.Stopwatch]::StartNew()
#Wait a bit
_Wkf_StartCommands
$StopWatch.Stop()
$StopWatch.Elapsed
