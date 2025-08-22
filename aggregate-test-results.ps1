# Test Results Aggregator
# Aggregates results from all Dynamo test XML files into a single summary

param(
    [string]$TestResultsPath = "TestResults"
)

Write-Host "=== Dynamo Test Results Summary ===" -ForegroundColor Cyan
Write-Host "Aggregating results from: $TestResultsPath" -ForegroundColor Yellow
Write-Host ""

# Initialize summary data
$allResults = @{}
$totalTests = 0
$totalFailures = 0
$totalTime = 0

# Get all XML files in TestResults folder
$xmlFiles = Get-ChildItem -Path $TestResultsPath -Filter "*.xml" | Sort-Object Name

if ($xmlFiles.Count -eq 0) {
    Write-Host "No XML files found in $TestResultsPath" -ForegroundColor Red
    exit 1
}

Write-Host "Found $($xmlFiles.Count) result files:" -ForegroundColor Green
$xmlFiles | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Gray }
Write-Host ""

# Process each XML file
foreach ($xmlFile in $xmlFiles) {
    Write-Host "Processing $($xmlFile.Name)..." -ForegroundColor Yellow
    
    try {
        [xml]$xml = Get-Content $xmlFile.FullName -Encoding UTF8
        
        # Process each testsuite in the XML
        foreach ($testsuite in $xml.testsuites.testsuite) {
            $dllName = $testsuite.name
            $tests = [int]$testsuite.tests
            $failures = [int]$testsuite.failures
            $time = [double]$testsuite.time
            
            # Initialize DLL entry if it doesn't exist
            if (-not $allResults.ContainsKey($dllName)) {
                $allResults[$dllName] = @{
                    Tests = 0
                    Failures = 0
                    Time = 0.0
                    Files = @()
                }
            }
            
            # Add results to the DLL entry
            $allResults[$dllName].Tests += $tests
            $allResults[$dllName].Failures += $failures
            $allResults[$dllName].Time += $time
            $allResults[$dllName].Files += $xmlFile.Name
            
            # Update totals
            $totalTests += $tests
            $totalFailures += $failures
            $totalTime += $time
        }
    }
    catch {
        Write-Host "Error processing $($xmlFile.Name): $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Display results
Write-Host "=== Test Results by DLL ===" -ForegroundColor Cyan
Write-Host ""

# Sort by DLL name for consistent output
$sortedDlls = $allResults.Keys | Sort-Object

foreach ($dllName in $sortedDlls) {
    $result = $allResults[$dllName]
    $successRate = if ($result.Tests -gt 0) { 
        [math]::Round((($result.Tests - $result.Failures) / $result.Tests) * 100, 1) 
    } else { 0 }
    
    $statusColor = if ($result.Failures -eq 0) { "Green" } else { "Red" }
    $statusIcon = if ($result.Failures -eq 0) { "✅" } else { "❌" }
    
    Write-Host "$statusIcon $dllName" -ForegroundColor $statusColor
    Write-Host "  Tests: $($result.Tests), Failures: $($result.Failures), Success Rate: $successRate%, Time: $([math]::Round($result.Time, 2))s" -ForegroundColor Gray
    Write-Host "  Files: $($result.Files -join ', ')" -ForegroundColor DarkGray
    Write-Host ""
}

# Summary
Write-Host "=== Summary ===" -ForegroundColor Cyan
$overallSuccessRate = if ($totalTests -gt 0) { 
    [math]::Round((($totalTests - $totalFailures) / $totalTests) * 100, 1) 
} else { 0 }

Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Total Failures: $totalFailures" -ForegroundColor $(if ($totalFailures -eq 0) { "Green" } else { "Red" })
Write-Host "Success Rate: $overallSuccessRate%" -ForegroundColor $(if ($overallSuccessRate -eq 100) { "Green" } else { "Yellow" })
Write-Host "Total Time: $([math]::Round($totalTime, 2))s" -ForegroundColor White
Write-Host "DLLs Tested: $($allResults.Count)" -ForegroundColor White

# Save to file
$summaryFile = "test-results-summary.txt"
$summaryContent = @"
=== Dynamo Test Results Summary ===
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Total Tests: $totalTests
Total Failures: $totalFailures
Success Rate: $overallSuccessRate%
Total Time: $([math]::Round($totalTime, 2))s
DLLs Tested: $($allResults.Count)

=== Results by DLL ===
"@

foreach ($dllName in $sortedDlls) {
    $result = $allResults[$dllName]
    $successRate = if ($result.Tests -gt 0) { 
        [math]::Round((($result.Tests - $result.Failures) / $result.Tests) * 100, 1) 
    } else { 0 }
    
    $summaryContent += "`n$dllName"
    $summaryContent += "  Tests: $($result.Tests), Failures: $($result.Failures), Success Rate: $successRate%, Time: $([math]::Round($result.Time, 2))s"
    $summaryContent += "  Files: $($result.Files -join ', ')"
}

$summaryContent | Out-File -FilePath $summaryFile -Encoding UTF8
Write-Host "`nSummary saved to: $summaryFile" -ForegroundColor Green
