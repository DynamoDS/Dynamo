# Dynamo Test Runner Script - Simple Version
# Updated for current codebase (2024)
# Simplified to avoid PowerShell parallel execution issues

param(
    [string]$TestTypeToRun = "simple",
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

Write-Host "=== Dynamo Test Runner (Simple) ===" -ForegroundColor Cyan
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
            
            # Handle JUnit XML structure (testsuites)
            if ($xml.DocumentElement.Name -eq "testsuites") {
                $xml.testsuites.testsuite | ForEach-Object {
                    $totalTests += [int]$_.tests
                    $passedTests += [int]$_.tests - [int]$_.failures - [int]$_.skipped
                    $failedTests += [int]$_.failures
                    $skippedTests += [int]$_.skipped
                }
            }
            # Handle NUnit XML structure (test-results)
            elseif ($xml.DocumentElement.Name -eq "test-results") {
                $totalTests += [int]$xml.'test-results'.total
                $passedTests += [int]$xml.'test-results'.passed
                $failedTests += [int]$xml.'test-results'.failures
                $skippedTests += [int]$xml.'test-results'.skipped
            }
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
try {
    $startTime = Get-Date
    Write-Host "`nStarting test execution at: $startTime" -ForegroundColor Cyan
    
    if ($TestTypeToRun -eq "all") {
        Write-Host "Executing ALL tests in parallel batches..." -ForegroundColor Green
        
        # Run tests in batches for better performance
        $testCmd = "dotnet test --arch x64 -v n " + ($TestDlls -join " ")
        
        # Batch 1: Core tests
        Write-Host "`nExecuting Batch 1: Core tests..." -ForegroundColor Yellow
        $cmd1 = $testCmd + " --filter `"${filterFailures}&FullyQualifiedName~DynamoCoreTests`" --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.1.xml`""
        Invoke-Expression $cmd1
        
        # Batch 2: WPF tests
        Write-Host "`nExecuting Batch 2: WPF tests..." -ForegroundColor Yellow
        $cmd2 = $testCmd + " --filter `"${filterFailures}&FullyQualifiedName~DynamoCoreWpfTests`" --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.2.xml`""
        Invoke-Expression $cmd2
        
        # Batch 3: Library tests
        Write-Host "`nExecuting Batch 3: Library tests..." -ForegroundColor Yellow
        $cmd3 = $testCmd + " --filter `"${filterFailures}&FullyQualifiedName~DynamoUtilitiesTests|FullyQualifiedName~AnalysisTests|FullyQualifiedName~CommandLineTests`" --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.3.xml`""
        Invoke-Expression $cmd3
        
        # Batch 4: Remaining tests
        Write-Host "`nExecuting Batch 4: Remaining tests..." -ForegroundColor Yellow
        $cmd4 = $testCmd + " --filter `"${filterFailures}&FullyQualifiedName!~DynamoCoreTests&FullyQualifiedName!~DynamoCoreWpfTests&FullyQualifiedName!~DynamoUtilitiesTests&FullyQualifiedName!~AnalysisTests&FullyQualifiedName!~CommandLineTests`" --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.4.xml`""
        Invoke-Expression $cmd4
        
    }
    elseif ($TestTypeToRun -eq "coreonly") {
        Write-Host "Executing CORE tests only..." -ForegroundColor Green
        
        $filterCoreFailures = "${filterFailures}&TestCategory!=FailureDynCore"
        $coreDlls = @('DynamoCoreTests.dll', 'DisplayTests.dll', 'AnalysisTests.dll', 'DynamoUtilitiesTests.dll')
        $coreTestDlls = $coreDlls | ForEach-Object { "$baseDir\$_" }
        
        $testCmd = "dotnet test " + ($coreTestDlls -join " ") + " --arch x64 -v q --filter `"${filterCoreFailures}`" --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.xml`""
        
        Write-Host "Executing core tests..." -ForegroundColor Yellow
        Invoke-Expression $testCmd
    }
    elseif ($TestTypeToRun -eq "simple") {
        Write-Host "Executing simple test run (all tests in one batch)..." -ForegroundColor Green
        
        $testCmd = "dotnet test --arch x64 -v n " + ($TestDlls -join " ") + " --filter `"${filterFailures}`" --logger:`"junit;LogFilePath=TestResults\DynamoTestsResult.xml`""
        
        Write-Host "Executing all tests in single batch..." -ForegroundColor Yellow
        Invoke-Expression $testCmd
    }
    else {
        Write-Error "Invalid TestTypeToRun: $TestTypeToRun. Use 'all', 'coreonly', or 'simple'"
        exit 1
    }
    
    $endTime = Get-Date
    Write-Host "`nTest execution completed at: $endTime" -ForegroundColor Cyan
    $duration = $endTime - $startTime
    Write-Host "Total elapsed time: $($duration.ToString('hh\:mm\:ss'))" -ForegroundColor Cyan
    
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
