# Quick script to check what's in the test result XML files

Write-Host "=== Checking Test Results ===" -ForegroundColor Cyan

$xmlFiles = Get-ChildItem "TestResults\*.xml" -ErrorAction SilentlyContinue

if (-not $xmlFiles) {
    Write-Host "No XML files found in TestResults folder" -ForegroundColor Red
    exit 1
}

foreach ($file in $xmlFiles) {
    Write-Host "`n=== $($file.Name) ===" -ForegroundColor Yellow
    
    try {
        $content = Get-Content $file -Raw
        Write-Host "File size: $($content.Length) characters" -ForegroundColor Gray
        
        # Try to parse as XML
        $xml = [xml]$content
        
        # Check what nodes exist
        Write-Host "Root element: $($xml.DocumentElement.Name)" -ForegroundColor Green
        
        # List all child elements
        Write-Host "Child elements:" -ForegroundColor Green
        $xml.DocumentElement.ChildNodes | ForEach-Object {
            Write-Host "  - $($_.Name): $($_.InnerText)" -ForegroundColor Gray
        }
        
        # Try different possible structures
        if ($xml.DocumentElement.Name -eq "test-results") {
            Write-Host "Found test-results structure:" -ForegroundColor Green
            Write-Host "  Total: $($xml.DocumentElement.total)" -ForegroundColor White
            Write-Host "  Passed: $($xml.DocumentElement.passed)" -ForegroundColor Green
            Write-Host "  Failed: $($xml.DocumentElement.failures)" -ForegroundColor Red
            Write-Host "  Skipped: $($xml.DocumentElement.skipped)" -ForegroundColor Yellow
        }
        elseif ($xml.DocumentElement.Name -eq "testsuites") {
            Write-Host "Found testsuites structure:" -ForegroundColor Green
            $xml.DocumentElement.testsuite | ForEach-Object {
                Write-Host "  Tests: $($_.tests), Failures: $($_.failures), Skipped: $($_.skipped)" -ForegroundColor White
            }
        }
        else {
            Write-Host "Unknown XML structure. First 500 characters:" -ForegroundColor Yellow
            Write-Host $content.Substring(0, [Math]::Min(500, $content.Length)) -ForegroundColor Gray
        }
        
    }
    catch {
        Write-Host "Error parsing $($file.Name): $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "First 200 characters of file:" -ForegroundColor Yellow
        $content = Get-Content $file -Raw -ErrorAction SilentlyContinue
        if ($content) {
            Write-Host $content.Substring(0, [Math]::Min(200, $content.Length)) -ForegroundColor Gray
        }
    }
}

Write-Host "`n=== End of Results Check ===" -ForegroundColor Cyan
