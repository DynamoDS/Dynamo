# Quick Test Runner - Small subset for verification
# This runs just a few tests to verify everything is working

# Fix date/time localization issues
# Use a more targeted approach that doesn't break culture-dependent tests
$env:DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = 0
$env:DOTNET_SYSTEM_GLOBALIZATION_PREDEFINED_CULTURES_ONLY = 1

Write-Host "=== Quick Test Runner ===" -ForegroundColor Cyan
Write-Host "Running small subset of tests to verify setup..." -ForegroundColor Yellow

# Find test DLLs
$baseDir = if (Test-Path "bin\AnyCPU\Debug") { 
    (Get-Location).Path + "\bin\AnyCPU\Debug" 
}
else { 
    "C:\Users\$env:USERNAME\Documents\GitHub\Dynamo\bin\AnyCPU\Debug" 
}

if (-not (Test-Path $baseDir)) {
    Write-Error "Test directory not found: $baseDir"
    Write-Host "Please build the project first." -ForegroundColor Red
    exit 1
}

# Clean up previous results
if (Test-Path "TestResults") { 
    Remove-Item "TestResults" -Recurse -Force 
}
New-Item -ItemType Directory -Path "TestResults" -Force | Out-Null

Write-Host "`n1. Testing DateTimeWrappers (your failing test)..." -ForegroundColor Yellow
$cmd1 = "dotnet test `"$baseDir\DynamoCoreTests.dll`" --filter `"FullyQualifiedName~DateTimeWrappers`" --logger:`"junit;LogFilePath=TestResults\quick-test-1.xml`""
Invoke-Expression $cmd1

Write-Host "`n2. Testing a few basic tests..." -ForegroundColor Yellow
$cmd2 = "dotnet test `"$baseDir\DynamoCoreTests.dll`" --filter `"FullyQualifiedName~BasicTests`" --logger:`"junit;LogFilePath=TestResults\quick-test-2.xml`""
Invoke-Expression $cmd2

Write-Host "`n3. Testing utilities..." -ForegroundColor Yellow
$cmd3 = "dotnet test `"$baseDir\DynamoUtilitiesTests.dll`" --filter `"TestCategory!=Failure`" --logger:`"junit;LogFilePath=TestResults\quick-test-3.xml`""
Invoke-Expression $cmd3

Write-Host "`n=== Quick Test Summary ===" -ForegroundColor Cyan

# Summarize results
$xmlFiles = Get-ChildItem "TestResults\*.xml" -ErrorAction SilentlyContinue
if ($xmlFiles) {
    $totalTests = 0
    $passedTests = 0
    $failedTests = 0
    
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
    
    if ($failedTests -eq 0) {
        Write-Host "`n✅ Quick test successful! Your setup is working." -ForegroundColor Green
        Write-Host "You can now run the full test suite." -ForegroundColor Cyan
    }
    else {
        Write-Host "`n❌ Some tests failed. Check the results before running full suite." -ForegroundColor Red
    }
}
else {
    Write-Host "No test results found." -ForegroundColor Yellow
}

Write-Host "`nCheck TestResults folder for detailed XML results." -ForegroundColor Gray
