# Simple Test Report Generator
param(
    [string]$TrxPath = "",
    [string]$OutputPath = "TestReport\EnhancedTestReport.html"
)

# Find the latest TRX file in TestReport directory
$testReportPath = Join-Path $PSScriptRoot "TestReport"
if ([string]::IsNullOrEmpty($TrxPath)) {
    if (Test-Path $testReportPath) {
        $trxFiles = Get-ChildItem "$testReportPath\*.trx" | Sort-Object LastWriteTime -Descending
        if ($trxFiles.Count -eq 0) {
            Write-Host "No TRX files found in $testReportPath" -ForegroundColor Red
            exit 1
        }
        $xmlPath = $trxFiles[0].FullName
    } else {
        Write-Host "TestReport directory not found: $testReportPath" -ForegroundColor Red
        exit 1
    }
} else {
    $xmlPath = $TrxPath
}

Write-Host "Processing TRX file: $xmlPath" -ForegroundColor Green

# Load test results
[xml]$testResults = Get-Content $xmlPath

# Define test priority mapping
function Get-TestPriority {
    param($testName)
    
    # Authentication & Security - High Priority
    if ($testName -match 'ChangePassword|UpdateUserStatus.*ChangeOwnStatus|CreateMember.*Duplicate|UpdateMember.*Duplicate') {
        return 'P1 - CRITICAL'
    }
    
    # Book Management Core Functions - High Priority
    if ($testName -match 'CreateBookRequest.*ValidationRules|CreateBook.*DuplicateIsbn|BookInventory.*ValidateBusinessRules|CalculateAvailableCopies') {
        return 'P2 - HIGH'
    }
    
    # API Controllers - Medium Priority  
    if ($testName -match 'BooksController|UsersController|CreateBook.*ValidRequest|UpdateBook.*ValidRequest') {
        return 'P3 - MEDIUM'
    }
    
    # Data Validation - Low Priority
    if ($testName -match 'BookResponse.*StateValidation|SetProperties|ValidationRules') {
        return 'P4 - LOW'
    }
    
    # Default to Medium priority
    return 'P3 - MEDIUM'
}

# Calculate totals
$totalTests = $testResults.TestRun.ResultSummary.Counters.total
$passedTests = $testResults.TestRun.ResultSummary.Counters.passed
$failedTests = $testResults.TestRun.ResultSummary.Counters.failed
$skippedTests = $testResults.TestRun.ResultSummary.Counters.skipped
$passRate = if($totalTests -gt 0) { [math]::Round(($passedTests / $totalTests) * 100, 1) } else { 0 }

# Calculate priority distribution
$priorityCount = @{}
$testResults.TestRun.Results.UnitTestResult | ForEach-Object {
    $priority = Get-TestPriority $_.testName
    if ($priorityCount.ContainsKey($priority)) {
        $priorityCount[$priority]++
    } else {
        $priorityCount[$priority] = 1
    }
}

# Generate minimal HTML report
$htmlContent = "<!DOCTYPE html>`n<html>`n<head>`n"
$htmlContent += "    <title>Unit Test Report</title>`n"
$htmlContent += "    <meta charset=`"utf-8`">`n"
$htmlContent += "    <style>`n"
$htmlContent += "        body { font-family: Arial, sans-serif; margin: 20px; }`n"
$htmlContent += "        .header { border-bottom: 2px solid #ccc; padding-bottom: 10px; margin-bottom: 20px; }`n"
$htmlContent += "        table { border-collapse: collapse; width: 100%; margin: 20px 0; }`n"
$htmlContent += "        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }`n"
$htmlContent += "        th { background-color: #f2f2f2; }`n"
$htmlContent += "        .passed { background-color: #d4edda; }`n"
$htmlContent += "        .failed { background-color: #f8d7da; }`n"
$htmlContent += "        .skipped { background-color: #fff3cd; }`n"
$htmlContent += "        .p1 { color: #dc3545; font-weight: bold; }`n"
$htmlContent += "        .p2 { color: #fd7e14; font-weight: bold; }`n"
$htmlContent += "        .p3 { color: #ffc107; font-weight: bold; }`n"
$htmlContent += "        .p4 { color: #28a745; font-weight: bold; }`n"
$htmlContent += "        .p5 { color: #17a2b8; font-weight: bold; }`n"
$htmlContent += "    </style>`n</head>`n<body>`n"
$htmlContent += "    <div class=`"header`">`n"
$htmlContent += "        <h1>Unit Test Report</h1>`n"
$htmlContent += "        <p>Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</p>`n"
$htmlContent += "    </div>`n`n"

$htmlContent += "    <h2>Test Summary</h2>`n"
$htmlContent += "    <table>`n"
$htmlContent += "        <tr><th>Total Tests</th><td>$totalTests</td></tr>`n"
$htmlContent += "        <tr><th>Passed</th><td>$passedTests</td></tr>`n"
$htmlContent += "        <tr><th>Failed</th><td>$failedTests</td></tr>`n"
$htmlContent += "        <tr><th>Skipped</th><td>$skippedTests</td></tr>`n"
$htmlContent += "        <tr><th>Pass Rate</th><td>$passRate%</td></tr>`n"
$htmlContent += "    </table>`n`n"

$htmlContent += "    <h2>Priority Distribution</h2>`n"
$htmlContent += "    <table>`n"
$htmlContent += "        <tr><th>Priority Level</th><th>Count</th><th>Description</th></tr>`n"

# Add priority distribution rows
@('P1 - CRITICAL', 'P2 - HIGH', 'P3 - MEDIUM', 'P4 - LOW', 'P5 - INFO') | ForEach-Object {
    $count = if ($priorityCount.ContainsKey($_)) { $priorityCount[$_] } else { 0 }
    $description = switch ($_) {
        'P1 - CRITICAL' { 'System crashes, security issues' }
        'P2 - HIGH' { 'Core functionality broken' }
        'P3 - MEDIUM' { 'Feature limitations, API issues' }
        'P4 - LOW' { 'Minor issues, validation problems' }
        'P5 - INFO' { 'Enhancements, code quality' }
    }
    $class = $_.Split(' ')[0].ToLower()
    $htmlContent += "        <tr><td class=`"$class`">$_</td><td>$count</td><td>$description</td></tr>`n"
}

$htmlContent += "    </table>`n`n"
$htmlContent += "    <h2>Test Results - All $totalTests Test Cases</h2>`n"
$htmlContent += "    <table>`n"
$htmlContent += "        <tr><th>Test Name</th><th>Result</th><th>Priority</th><th>Duration</th></tr>`n"

# Add all test results
$testResults.TestRun.Results.UnitTestResult | ForEach-Object {
    $testName = $_.testName
    $outcome = $_.outcome
    $duration = if ($_.duration) { $_.duration } else { "N/A" }
    $priority = Get-TestPriority $testName
    $statusClass = $outcome.ToLower()
    $priorityClass = $priority.Split(' ')[0].ToLower()
    
    $htmlContent += "        <tr class=`"$statusClass`"><td>$testName</td><td>$outcome</td><td class=`"$priorityClass`">$priority</td><td>$duration</td></tr>`n"
}

$htmlContent += "    </table>`n</body>`n</html>`n"

# Write the HTML content to file
$htmlContent | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "Simple HTML report generated: $OutputPath" -ForegroundColor Green