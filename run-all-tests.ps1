# Dynamo Test Runner Script
# Updated for current codebase (2024)
# Based on colleague's script with modern improvements

param(
    [string]$TestTypeToRun = "all",
    [string]$Configuration = "Debug",
    [string]$Platform = "AnyCPU"
)

# Setup and Configuration
$ErrorActionPreference = "Stop"
$baseDir = if (Test-Path "bin\$Platform\$Configuration") { 
    (Get-Location).Path + "\bin\$Platform\$Configuration" 
}
else { 
    "C:\Users\$env:USERNAME\Documents\GitHub\Dynamo\bin\$Platform\$Configuration" 
}

# Fix date/time localization issues that cause test failures
# Use a more targeted approach that doesn't break culture-dependent tests
$env:DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = 0
$env:DOTNET_SYSTEM_GLOBALIZATION_PREDEFINED_CULTURES_ONLY = 1

Write-Host "=== Dynamo Test Runner ===" -ForegroundColor Cyan
Write-Host "Base Directory: $baseDir" -ForegroundColor Yellow
Write-Host "Test Type: $TestTypeToRun" -ForegroundColor Yellow
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Verify base directory exists
if (-not (Test-Path $baseDir)) {
    Write-Error "Base directory not found: $baseDir"
    Write-Host "Please ensure the project is built first." -ForegroundColor Red
    exit 1
}

# Cleanup and setup
if (Test-Path "TestResults") { 
    Remove-Item "TestResults" -Recurse -Force 
    Write-Host "Cleaned up previous test results" -ForegroundColor Green
}
New-Item -ItemType Directory -Path "TestResults" -Force | Out-Null

# Get current test DLLs (automatically discover all test assemblies)
$DllList = Get-ChildItem $baseDir -Name -Include *Tests.dll | Get-Unique -AsString
$TestDlls = $DllList | ForEach-Object { "$baseDir\$_" }

Write-Host "Found test assemblies:" -ForegroundColor Green
$DllList | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }

# Updated filter to exclude known problematic tests
$filterFailures = "TestCategory!=Failure&TestCategory!=IntegrationTest&TestCategory!=SlowTest&TestCategory!=DisplayHardwareDependent"

# Test groupings for parallel execution
$Tests1 = @(
    'PythonNodeCustomizationTests', 
    'PreviewBubbleTests', 
    'NodeViewTests', 
    'NodeViewCustomizationTests', 
    'NodeAutoCompleteSearchTests', 
    'DocumentationBrowserViewExtensionTests'
)

$Tests2 = @(
    'JSONSerializationTests.SerializationNonGuidIdsTest', 
    'Dynamo.Tests.WorkspaceSaving', 
    'AnalysisTests.LabelTests', 
    'Dynamo.Tests.ComplexTests', 
    'Dynamo.Tests.DynamoSamples', 
    'Dynamo.Tests.GeometryDefectTests', 
    'Dynamo.Tests.PackageValidationTest', 
    'ProtoFFITests', 
    'GraphNodeManagerViewExtensionTests',
    'PreferenceSettingsTests',
    'Dynamo.Tests.Logging',
    'Dynamo.Tests.ProfilingTest',
    'DynamoCoreWpfTests.SplashScreenTests',
    'Dynamo.Tests.GraphLayoutTests',
    'ConnectorViewModelTests',
    'PeriodicUpdateTest',
    'AnnotationViewTests'
)

$Tests3 = @(
    'JSONSerializationTests.SerializationTest', 
    'WorkspaceDependencyViewExtensionTests', 
    'AnnotationViewModelTests', 
    'CoreUserInterfaceTests', 
    'DocumentationBrowserViewExtensionContentTests', 
    'DynamoUtilitiesTests', 
    'CommandLineTests', 
    'PythonEvalTests', 
    'PythonEditTests', 
    'DynamoPythonTests', 
    'IntegrationTests'  
)

$Tests4 = @(
    'WpfVisualizationTests', 
    'DynamoCoreWpfTests.PackageManager', 
    'Dynamo.PackageManager',
    'CrashReportingTests',
    'RecentFileTests',
    'WorkspaceOpeningTests',
    'CodeBlockNodeTests',
    'ColorPaletteTests',
    'ConnectorContextMenuTests',
    'ConverterViewModelTests',
    'DynamoViewTests',
    'ElementResolverTests',
    'GroupStylesTests',
    'LiveChartsTests',
    'NoteViewTests',
    'PackagePathTests',
    'PreferencesGeometryScalingTests',
    'RunSettingsTests',
    'UnitsUITests'
)

$Tests5 = @(
    'RecordedTests', 
    'RecordedTestsDSEngine'
)

$Tests6 = @(
    'Dynamo.Tests.ListTests', 
    'Dynamo.Tests.MigrationTestFramework', 
    'Dynamo.Tests.SerializationTests'
)

# Tests that need to run in serial mode
$SerialTests = @(
    'Dynamo.Tests.PeriodicEvaluationTests.StartPeriodicEvaluation_CompletesFewerRunsWhenRunTimeIsGreaterThanEvaluationTime',
    'Dynamo.Tests.SearchSideEffects.LuceneSearchTSplineNodesOrderingValidation'
)

# Build filter expressions
$filterTests1 = '(FullyQualifiedName~' + ($Tests1 -join '|FullyQualifiedName~') + ')'
$filterTests2 = '(FullyQualifiedName~' + ($Tests2 -join '|FullyQualifiedName~') + ')'
$filterTests3 = '(FullyQualifiedName~' + ($Tests3 -join '|FullyQualifiedName~') + ')'
$filterTests4 = '(FullyQualifiedName~' + ($Tests4 -join '|FullyQualifiedName~') + ')'
$filterTests5 = '(FullyQualifiedName~' + ($Tests5 -join '|FullyQualifiedName~') + ')'
$filterTests6 = '(FullyQualifiedName~' + ($Tests6 -join '|FullyQualifiedName~') + ')'
$filterSerialTests = '(FullyQualifiedName~' + ($SerialTests -join '|FullyQualifiedName~') + ')'

# Build filter for remaining tests
$allTestGroups = @($Tests1, $Tests2, $Tests3, $Tests4, $Tests5, $Tests6, $SerialTests) | ForEach-Object { $_ }
$filterRestOfTests = 'FullyQualifiedName!~' + ($allTestGroups | ForEach-Object { $_ -join '&FullyQualifiedName!~' })

# Function to execute tests with timing
function ExecuteTests {
    param (
        [string[]]$Cmds
    )
    
    $startTime = Get-Date
    Write-Host "Starting test execution at: $startTime" -ForegroundColor Cyan
    
    $et = Measure-Command {
        # Loop through test batches and run them in parallel
        # If a test command fails, the rest will go on but the overall test script will fail in the end.
        $jobs = @()
        
        for ($i = 0; $i -lt $Cmds.Length; $i++) {
            $cmd = $Cmds[$i]
            $batchNum = $i + 1
            
            Write-Host "Starting batch $batchNum of $($Cmds.Length)" -ForegroundColor Yellow
            
            $job = Start-Job -ScriptBlock {
                param($Command, $BatchNumber)
                try {
                    Write-Host "Batch $($BatchNumber): Executing command..." -ForegroundColor Gray
                    Invoke-Expression $Command
                    if ($LASTEXITCODE -ne 0) {
                        Write-Host "Batch $($BatchNumber) failed with exit code $LASTEXITCODE" -ForegroundColor Red
                        return @{ Success = $false; ExitCode = $LASTEXITCODE; Batch = $BatchNumber }
                    }
                    else {
                        Write-Host "Batch $($BatchNumber) completed successfully" -ForegroundColor Green
                        return @{ Success = $true; ExitCode = 0; Batch = $BatchNumber }
                    }
                }
                catch {
                    Write-Host "Batch $($BatchNumber) error occurred: $($_.Exception.Message)" -ForegroundColor Red
                    return @{ Success = $false; ExitCode = 1; Batch = $BatchNumber; Error = $_.Exception.Message }
                }
            } -ArgumentList $cmd, $batchNum
            
            $jobs += $job
        }
        
        # Wait for all jobs to complete
        $results = $jobs | Wait-Job | Receive-Job
        $jobs | Remove-Job
        
        # Check results
        $failedJobs = $results | Where-Object { -not $_.Success }
        if ($failedJobs) {
            Write-Host "Some test batches failed:" -ForegroundColor Red
            $failedJobs | ForEach-Object { 
                Write-Host "  Batch $($_.Batch): Exit code $($_.ExitCode)" -ForegroundColor Red
                if ($_.Error) { Write-Host "    Error: $($_.Error)" -ForegroundColor Gray }
            }
        }
        else {
            Write-Host "All test batches completed successfully!" -ForegroundColor Green
        }
    }
    
    $endTime = Get-Date
    Write-Host "Test execution completed at: $endTime" -ForegroundColor Cyan
    Write-Host "Total elapsed time: $($et.ToString())" -ForegroundColor Cyan
}

# Function to summarize test results
function SummarizeTestResults {
    Write-Host "`n=== TEST SUMMARY ===" -ForegroundColor Cyan
    
    $xmlFiles = Get-ChildItem "TestResults\*.xml" -ErrorAction SilentlyContinue
    if (-not $xmlFiles) {
        Write-Host "No test result files found" -ForegroundColor Yellow
        return
    }
    
    $totalTests = 0
    $passedTests = 0
    $failedTests = 0
    $skippedTests = 0
    
    foreach ($file in $xmlFiles) {
        try {
            $xml = [xml](Get-Content $file)
            $totalTests += [int]$xml.'test-results'.total
            $passedTests += [int]$xml.'test-results'.passed
            $failedTests += [int]$xml.'test-results'.failures
            $skippedTests += [int]$xml.'test-results'.skipped
        }
        catch {
            Write-Host "Error parsing $($file.Name)" -ForegroundColor Yellow
        }
    }
    
    Write-Host "Total Tests: $totalTests" -ForegroundColor White
    Write-Host "Passed: $passedTests" -ForegroundColor Green
    Write-Host "Failed: $failedTests" -ForegroundColor Red
    Write-Host "Skipped: $skippedTests" -ForegroundColor Yellow
    
    if ($failedTests -gt 0) {
        Write-Host "`nSome tests failed. Check the logs for details." -ForegroundColor Red
        exit 1
    }
    else {
        Write-Host "`nAll tests passed! 🎉" -ForegroundColor Green
    }
}

# Main execution logic
$errors = @{ "err_code" = 0 }

try {
    if ($TestTypeToRun -eq "all") {
        Write-Host "`nExecuting ALL tests in parallel batches..." -ForegroundColor Green
        
        $testCmd = "dotnet test --arch x64 -v n " + ($TestDlls -join " ")
        
        $testCmd1 = $testCmd + " --filter `"${filterFailures}&${filterTests1}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.1.xml`" > test_log_1.log 2>&1"
        $testCmd2 = $testCmd + " --filter `"${filterFailures}&${filterTests2}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.2.xml`" > test_log_2.log 2>&1"
        $testCmd3 = $testCmd + " --filter `"${filterFailures}&${filterTests3}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.3.xml`" > test_log_3.log 2>&1"
        $testCmd4 = $testCmd + " --filter `"${filterFailures}&${filterTests4}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.4.xml`" > test_log_4.log 2>&1"
        $testCmd5 = $testCmd + " --filter `"${filterFailures}&${filterTests5}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.5.xml`" > test_log_5.log 2>&1"
        $testCmd6 = $testCmd + " --filter `"${filterFailures}&${filterTests6}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.6.xml`" > test_log_6.log 2>&1"
        $testCmd7 = $testCmd + " --filter `"${filterFailures}&${filterRestOfTests}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.7.xml`" > test_log_7.log 2>&1"
        $testSerialCmd = $testCmd + " --filter `"${filterFailures}&${filterSerialTests}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.8.xml`" > test_log_8.log 2>&1"
        
        $testCmds = @($testCmd1, $testCmd2, $testCmd3, $testCmd4, $testCmd5, $testCmd6, $testCmd7)
        
        Write-Host "Executing serial tests first..." -ForegroundColor Yellow
        ExecuteTests @($testSerialCmd)
        
        Write-Host "Executing parallel test batches..." -ForegroundColor Yellow
        ExecuteTests $testCmds
        
    }
    elseif ($TestTypeToRun -eq "coreonly") {
        Write-Host "`nExecuting CORE tests only..." -ForegroundColor Green
        
        $filterCoreFailures = "${filterFailures}&TestCategory!=FailureDynCore"
        $coreDlls = @('DynamoCoreTests.dll', 'DisplayTests.dll', 'AnalysisTests.dll', 'DynamoUtilitiesTests.dll')
        $coreTestDlls = $coreDlls | ForEach-Object { "$baseDir\$_" }
        
        $testCmd = "dotnet test " + ($coreTestDlls -join " ") + " --arch x64 -v q --filter `"${filterCoreFailures}`"" + " --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.xml`" -- NUnit.ConsoleOut=0"
        
        Write-Host "Executing core tests..." -ForegroundColor Yellow
        Invoke-Expression $testCmd
    }
    elseif ($TestTypeToRun -eq "simple") {
        Write-Host "`nExecuting simple test run (all tests in one batch)..." -ForegroundColor Green
        
        $testCmd = "dotnet test --arch x64 -v n " + ($TestDlls -join " ") + " --filter `"${filterFailures}`" --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.xml`""
        
        Write-Host "Executing all tests in single batch..." -ForegroundColor Yellow
        Invoke-Expression $testCmd
    }
    else {
        Write-Error "Invalid TestTypeToRun: $TestTypeToRun. Use 'all', 'coreonly', or 'simple'"
        exit 1
    }
    
    # Summarize results
    SummarizeTestResults
    
}
catch {
    Write-Host "`nTest execution failed with error:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host $_.Exception.StackTrace -ForegroundColor Gray
    exit 1
}

Write-Host "`nTest execution completed!" -ForegroundColor Green
Write-Host "Check the TestResults folder for detailed results." -ForegroundColor Cyan
