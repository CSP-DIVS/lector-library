param(
    [Parameter(Mandatory=$true)][string]$ResultsDir,
    [Parameter(Mandatory=$true)][string]$ProjectDir
)

try {
    $ErrorActionPreference = 'Stop'

    $timestamp = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
    $reportPath = Join-Path $ProjectDir 'TestReports/integration_test_report.html'
    $timestampedReportPath = Join-Path $ProjectDir ("TestReports/IntegrationTestReport_{0}.html" -f $timestamp)

    $trxFiles = Get-ChildItem -Path (Join-Path $ResultsDir '*.trx') -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending
    if (-not $trxFiles -or $trxFiles.Count -eq 0) {
        Write-Host "No TRX files found in $ResultsDir" -ForegroundColor Yellow
        exit 0
    }

    $xmlPath = $trxFiles[0].FullName
    [xml]$testResults = Get-Content -LiteralPath $xmlPath

    $counters = $testResults.TestRun.ResultSummary.Counters
    $total = [int]$counters.total
    $passed = [int]$counters.passed
    $failed = [int]$counters.failed
    $skipped = if ($counters.skipped) { [int]$counters.skipped } else { 0 }
    $passRate = if ($total -gt 0) { [math]::Round(($passed / $total) * 100, 2) } else { 0 }
    $duration = if ($testResults.TestRun.Times.finished -and $testResults.TestRun.Times.start) { 
        $testResults.TestRun.Times.finished - $testResults.TestRun.Times.start 
    } else { 'N/A' }

    $coverageIndex = Join-Path $ProjectDir 'Coverage/Html/index.html'
    $coverageLink = if (Test-Path $coverageIndex) { "file:///$($coverageIndex -replace '\\','/')" } else { '#' }
    $reportsFolder = "file:///$($ProjectDir -replace '\\','/')/TestReports"

    $html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>Book Management Integration Test Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        .header { text-align: center; color: #2c3e50; border-bottom: 3px solid #e74c3c; padding-bottom: 10px; margin-bottom: 20px; }
        .summary { display: flex; justify-content: space-around; margin: 20px 0; flex-wrap: wrap; }
        .summary-card { background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%); color: white; padding: 15px; border-radius: 8px; text-align: center; min-width: 120px; margin: 5px; }
        .summary-card.passed { background: linear-gradient(135deg, #27ae60 0%, #229954 100%); }
        .summary-card.failed { background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%); }
        .summary-card.skipped { background: linear-gradient(135deg, #f39c12 0%, #e67e22 100%); }
        .summary-card.total { background: linear-gradient(135deg, #3498db 0%, #2980b9 100%); }
        .summary-card.rate { background: linear-gradient(135deg, #9b59b6 0%, #8e44ad 100%); }
        .summary-card h3 { margin: 0 0 10px 0; font-size: 14px; }
        .summary-card h2 { margin: 0; font-size: 24px; }
        .test-details { margin-top: 30px; }
        .test-class { margin: 20px 0; border: 1px solid #ddd; border-radius: 5px; overflow: hidden; }
        .test-class-header { background-color: #e74c3c; color: white; padding: 15px; font-weight: bold; font-size: 16px; }
        .test-method { padding: 12px 15px; border-bottom: 1px solid #eee; }
        .test-method:last-child { border-bottom: none; }
        .test-method.passed { background-color: #d5f4e6; border-left: 4px solid #27ae60; }
        .test-method.failed { background-color: #fadbd8; border-left: 4px solid #e74c3c; }
        .test-method.skipped { background-color: #fdeaa7; border-left: 4px solid #f39c12; }
        .test-name { font-weight: bold; color: #2c3e50; }
        .test-duration { float: right; color: #7f8c8d; font-size: 12px; }
        .error-message { color: #e74c3c; font-size: 12px; margin-top: 5px; font-family: monospace; background-color: #f8f9fa; padding: 5px; border-radius: 3px; }
        .timestamp { text-align: center; color: #666; margin-top: 30px; font-style: italic; }
        .info-section { background-color: #fdf2e9; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #e67e22; }
        .links { text-align: center; margin: 20px 0; }
        .links a { color: #e74c3c; text-decoration: none; margin: 0 15px; padding: 8px 16px; border: 1px solid #e74c3c; border-radius: 4px; }
        .links a:hover { background-color: #e74c3c; color: white; text-decoration: none; }
        .integration-badge { background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%); color: white; padding: 5px 10px; border-radius: 15px; font-size: 12px; display: inline-block; margin-left: 10px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔗 Book Management Integration Test Report</h1>
            <span class='integration-badge'>INTEGRATION TESTS</span>
            <h2>End-to-End Functionality Validation</h2>
        </div>
        <div class='summary'>
            <div class='summary-card total'><h3>Total Tests</h3><h2>$total</h2></div>
            <div class='summary-card passed'><h3>✅ Passed</h3><h2>$passed</h2></div>
            <div class='summary-card failed'><h3>❌ Failed</h3><h2>$failed</h2></div>
            <div class='summary-card skipped'><h3>⏭️ Skipped</h3><h2>$skipped</h2></div>
            <div class='summary-card rate'><h3>Pass Rate</h3><h2>$passRate%</h2></div>
        </div>
        <div class='info-section'>
            <h3>📊 Test Execution Summary</h3>
            <p><strong>Execution Time:</strong> $duration</p>
            <p><strong>Test Framework:</strong> xUnit.net with ASP.NET Core TestHost</p>
            <p><strong>Database:</strong> MySQL Test Containers (with local fallback)</p>
            <p><strong>Authentication:</strong> JWT Token-based testing</p>
            <p><strong>Coverage Report:</strong> <a href='$coverageLink'>Open Coverage</a></p>
        </div>
        <div class='links'>
            <a href='$reportsFolder'>📁 Test Reports Folder</a>
        </div>
        <div class='timestamp'>Report generated on $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</div>
    </div>
</body>
</html>
"@

    New-Item -ItemType Directory -Force -Path (Join-Path $ProjectDir 'TestReports') | Out-Null
    $html | Out-File -FilePath $reportPath -Encoding UTF8
    $html | Out-File -FilePath $timestampedReportPath -Encoding UTF8

    Write-Host "Integration test report generated: $reportPath" -ForegroundColor Green
    Write-Host "Timestamped report: $timestampedReportPath" -ForegroundColor Green
    if (Test-Path $coverageIndex) {
        Write-Host "Coverage report: $coverageIndex" -ForegroundColor Green
    }
}
catch {
    Write-Warning $_
    exit 0
}
