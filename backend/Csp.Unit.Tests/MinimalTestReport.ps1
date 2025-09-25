# Minimal Test Report Generator
param(
    [string]$TrxPath = "",
    [string]$OutputPath = ""
)

# Generate timestamped filename if no specific output path provided
if ([string]::IsNullOrEmpty($OutputPath)) {
    $timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $OutputPath = "TestReport\TestReport_$timestamp.html"
}

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
} elseif (Test-Path $TrxPath -PathType Container) {
    $trxFiles = Get-ChildItem "$TrxPath\*.trx" | Sort-Object LastWriteTime -Descending
    if ($trxFiles.Count -eq 0) {
        Write-Host "No TRX files found in $TrxPath" -ForegroundColor Red
        exit 1
    }
    $xmlPath = $trxFiles[0].FullName
} elseif (Test-Path $TrxPath -PathType Leaf) {
    $xmlPath = $TrxPath
} else {
    Write-Host "TRX path not found: $TrxPath" -ForegroundColor Red
    exit 1
}

Write-Host "Processing TRX file: $xmlPath" -ForegroundColor Green

# Load test results
[xml]$testResults = Get-Content $xmlPath

# Define test priority mapping based on test categories and names
$testPriorities = @{
    # Authentication & Security - Critical Priority
    'ChangePassword' = 'P1 - CRITICAL'
    'UpdateUserStatus_ReturnsFalse_WhenActorTriesToChangeOwnStatus' = 'P1 - CRITICAL'
    'CreateMember_BadRequest_When_Duplicate' = 'P2 - HIGH'
    'UpdateMember_BadRequest_When_Duplicate' = 'P2 - HIGH'
    
    # Book Management Core Functions - High Priority
    'CreateBookRequest_ValidationRules' = 'P2 - HIGH'
    'CreateBook_DuplicateIsbn' = 'P2 - HIGH'
    'BookInventory_ValidateBusinessRules' = 'P2 - HIGH'
    'CalculateAvailableCopies' = 'P2 - HIGH'
    
    # API Controllers - Medium Priority  
    'BooksController' = 'P3 - MEDIUM'
    'UsersController' = 'P3 - MEDIUM'
    'CreateBook_ValidRequest' = 'P3 - MEDIUM'
    'UpdateBook_ValidRequest' = 'P3 - MEDIUM'
    
    # Data Validation - Medium Priority
    'BookResponse_StateValidation' = 'P3 - MEDIUM'
    'PagedBooksResponse_ConsistencyValidation' = 'P3 - MEDIUM'
    'BookSearchRequest_PaginationValidation' = 'P3 - MEDIUM'
    
    # Model Properties & Basic Validation - Low Priority
    'DefaultValues_ShouldBeSetCorrectly' = 'P4 - LOW'
    'SetProperties_ShouldRetainValues' = 'P4 - LOW'
    'BookDto_StatusProperty' = 'P4 - LOW'
}

function Get-TestPriority {
    param([string]$testName)
    
    foreach ($key in $testPriorities.Keys) {
        if ($testName -like "*$key*") {
            return $testPriorities[$key]
        }
    }
    
    # Default priority assignment based on test class
    if ($testName -like "*Authentication*" -or $testName -like "*Security*" -or $testName -like "*Password*") {
        return 'P1 - CRITICAL'
    } elseif ($testName -like "*Controller*" -or $testName -like "*Service*") {
        return 'P3 - MEDIUM'
    } elseif ($testName -like "*Validation*" -or $testName -like "*Model*") {
        return 'P4 - LOW'
    } else {
        return 'P3 - MEDIUM'
    }
}

function Get-PriorityColor {
    param([string]$priority)
    
    switch ($priority) {
        'P1 - CRITICAL' { return '#d32f2f' }
        'P2 - HIGH' { return '#f57c00' }
        'P3 - MEDIUM' { return '#1976d2' }
        'P4 - LOW' { return '#388e3c' }
        default { return '#666' }
    }
}

# Extract test data
$counters = $testResults.TestRun.ResultSummary.Counters
$total = [int]$counters.total
$passed = [int]$counters.passed
$failed = [int]$counters.failed
$skipped = if($counters.skipped) { [int]$counters.skipped } else { 0 }
$passRate = if($total -gt 0) { [math]::Round(($passed / $total) * 100, 2) } else { 0 }

# Calculate duration
try {
    $startTime = [DateTime]::Parse($testResults.TestRun.Times.start)
    $finishTime = [DateTime]::Parse($testResults.TestRun.Times.finish)
    $duration = ($finishTime - $startTime).ToString("mm\:ss\.fff")
} catch {
    $duration = "N/A"
}

# Process test results
$results = $testResults.TestRun.Results.UnitTestResult
$groupedResults = $results | Group-Object { 
    $parts = $_.testName -split '\.'
    # Extract class name from full test name (e.g., "Csp.Unit.Tests.BooksControllerTests.MethodName" -> "BooksControllerTests")
    if ($parts.Length -ge 4) { $parts[3] } else { $parts[-2] }
}

# Count tests by priority
$priorityCounts = @{}
foreach ($result in $results) {
    $priority = Get-TestPriority $result.testName
    if ($priorityCounts.ContainsKey($priority)) {
        $priorityCounts[$priority]++
    } else {
        $priorityCounts[$priority] = 1
    }
}

# Generate HTML content
$htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <title>Unit Test Report</title>
    <meta charset="utf-8">
    <style>
        body { font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background-color: white; padding: 30px; border: 1px solid #ddd; }
        .header { text-align: center; border-bottom: 2px solid #333; padding-bottom: 20px; margin-bottom: 30px; }
        .header h1 { font-size: 24px; margin: 0; color: #333; }
        .header p { font-size: 14px; color: #666; margin: 5px 0 0 0; }
        
        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 15px; margin: 30px 0; }
        .summary-card { padding: 15px; text-align: center; border: 1px solid #ddd; background-color: #f9f9f9; }
        .summary-card h3 { margin: 0 0 10px 0; font-size: 14px; color: #555; }
        .summary-card h2 { margin: 0; font-size: 28px; color: #333; }
        .summary-card.failed h2 { color: #d32f2f; }
        .summary-card.passed h2 { color: #388e3c; }
        .summary-card.skipped h2 { color: #f57c00; }
        
        .failed-tests { margin: 30px 0; }
        .failed-tests h2 { color: #d32f2f; border-bottom: 2px solid #d32f2f; padding-bottom: 10px; }
        .failed-test-item { 
            background-color: #ffebee; 
            border: 2px solid #ffcdd2; 
            margin: 15px 0; 
            padding: 20px; 
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(211, 47, 47, 0.1);
        }
        .failed-test-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 10px;
        }
        .failed-test-name { font-weight: bold; color: #c62828; margin-bottom: 5px; font-size: 16px; }
        .failed-test-class { font-size: 12px; color: #666; margin-bottom: 5px; }
        .failed-test-duration { font-size: 12px; color: #666; }
        
        .priority-badge { 
            padding: 5px 12px; 
            font-size: 12px; 
            font-weight: bold;
            color: white;
            border-radius: 15px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        .priority-badge.p1-critical { background-color: #d32f2f; }
        .priority-badge.p2-high { background-color: #f57c00; }
        .priority-badge.p3-medium { background-color: #1976d2; }
        .priority-badge.p4-low { background-color: #388e3c; }
        
        .priority-section { margin: 30px 0; }
        .priority-section h2 { color: #333; border-bottom: 2px solid #333; padding-bottom: 10px; }
        .priority-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 15px; margin: 20px 0; }
        .priority-card { padding: 15px; text-align: center; border: 1px solid #ddd; background-color: #f9f9f9; }
        .priority-card h3 { margin: 0 0 10px 0; font-size: 14px; color: #555; }
        .priority-card h2 { margin: 0; font-size: 24px; }
        .priority-card.critical h2 { color: #d32f2f; }
        .priority-card.high h2 { color: #f57c00; }
        .priority-card.medium h2 { color: #1976d2; }
        .priority-card.low h2 { color: #388e3c; }
        
        .test-details { margin-top: 40px; }
        .test-class { margin: 20px 0; border: 1px solid #ddd; }
        .test-class-header { background-color: #f0f0f0; padding: 15px; font-weight: bold; color: #333; border-bottom: 1px solid #ddd; }
        .test-method { padding: 10px 15px; border-bottom: 1px solid #eee; display: flex; justify-content: space-between; align-items: center; }
        .test-method:last-child { border-bottom: none; }
        
        .test-info { flex-grow: 1; }
        .test-name { margin-bottom: 3px; }
        .test-details-small { font-size: 12px; color: #666; }
        
        .test-method.passed { background-color: #e8f5e8; }
        .test-method.failed { background-color: #ffebee; }
        .test-method.skipped { background-color: #fff3e0; }
        
        .status { width: 60px; text-align: center; font-size: 12px; font-weight: bold; }
        .status.passed { color: #388e3c; }
        .status.failed { color: #d32f2f; }
        .status.skipped { color: #f57c00; }
        
        .priority-badge { 
            padding: 3px 8px; 
            font-size: 11px; 
            color: white;
            border-radius: 3px;
            margin-left: 10px;
            min-width: 70px;
            text-align: center;
        }
        
        .timestamp { text-align: center; color: #666; margin-top: 30px; font-size: 12px; padding-top: 20px; border-top: 1px solid #ddd; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>Unit Test Report</h1>
            <p>Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</p>
            <p>Duration: $duration</p>
        </div>

        <div class="summary">
            <div class="summary-card">
                <h3>Total Tests</h3>
                <h2>$total</h2>
            </div>
            <div class="summary-card passed">
                <h3>Passed</h3>
                <h2>$passed</h2>
            </div>
            <div class="summary-card failed">
                <h3>Failed</h3>
                <h2>$failed</h2>
            </div>
            <div class="summary-card skipped">
                <h3>Skipped</h3>
                <h2>$skipped</h2>
            </div>
            <div class="summary-card">
                <h3>Pass Rate</h3>
                <h2>$passRate%</h2>
            </div>
        </div>

        <div class="priority-section">
            <h2>Test Priority Distribution</h2>
            <div class="priority-grid">
"@

# Add priority distribution cards
foreach ($priority in @('P1 - CRITICAL', 'P2 - HIGH', 'P3 - MEDIUM', 'P4 - LOW')) {
    $count = if ($priorityCounts.ContainsKey($priority)) { $priorityCounts[$priority] } else { 0 }
    $cssClass = switch ($priority) {
        'P1 - CRITICAL' { 'critical' }
        'P2 - HIGH' { 'high' }
        'P3 - MEDIUM' { 'medium' }
        'P4 - LOW' { 'low' }
    }
    
    $htmlContent += @"
                <div class="priority-card $cssClass">
                    <h3>$priority</h3>
                    <h2>$count</h2>
                </div>
"@
}

$htmlContent += @"
            </div>
        </div>
"@

# Add failed tests section if there are any failures
if ($failed -gt 0) {
    $failedTests = $results | Where-Object { $_.outcome -eq 'Failed' }
    $htmlContent += @"

        <div class="failed-tests">
            <h2>Failed Tests ($failed)</h2>
"@
    
    foreach ($failedTest in $failedTests) {
        $parts = $failedTest.testName -split '\.'
        $testClass = if ($parts.Length -ge 4) { $parts[3] } else { $parts[-2] }
        $testMethodName = $parts[-1]
        $testDuration = if ($failedTest.duration) { $failedTest.duration } else { "N/A" }
        $priority = Get-TestPriority $failedTest.testName
        
        # Get priority badge class
        $priorityClass = switch ($priority) {
            "P1-CRITICAL" { "p1-critical" }
            "P2-HIGH" { "p2-high" }
            "P3-MEDIUM" { "p3-medium" }
            "P4-LOW" { "p4-low" }
            default { "p3-medium" }
        }
        
        $htmlContent += @"
            <div class="failed-test-item">
                <div class="failed-test-header">
                    <div class="failed-test-name">$testMethodName</div>
                    <div class="priority-badge $priorityClass">$priority</div>
                </div>
                <div class="failed-test-class">Class: $testClass</div>
                <div class="failed-test-duration">Duration: $testDuration</div>
            </div>
"@
    }
    
    $htmlContent += @"
        </div>
"@
}

$htmlContent += @"

        <div class="test-details">
            <h2>All Test Results</h2>
"@

# Generate test results by class
foreach ($group in $groupedResults) {
    $className = $group.Name
    $tests = $group.Group
    
    $htmlContent += @"
            <div class="test-class">
                <div class="test-class-header">$className</div>
"@
    
    foreach ($test in $tests) {
        $outcome = $test.outcome.ToLower()
        $parts = $test.testName -split '\.'
        $testMethodName = $parts[-1]
        $testDuration = if ($test.duration) { $test.duration } else { "N/A" }
        $statusText = $outcome.ToUpper()
        $priority = Get-TestPriority $test.testName
        $priorityColor = Get-PriorityColor $priority
        
        $htmlContent += @"
                <div class="test-method $outcome">
                    <div class="test-info">
                        <div class="test-name">$testMethodName</div>
                        <div class="test-details-small">Duration: $testDuration</div>
                    </div>
                    <div class="priority-badge" style="background-color: $priorityColor;">$priority</div>
                    <div class="status $outcome">$statusText</div>
                </div>
"@
    }
    
    $htmlContent += @"
            </div>
"@
}

$htmlContent += @"
        </div>
        
        <div class="timestamp">
            Report generated on $(Get-Date -Format 'yyyy-MM-dd') at $(Get-Date -Format 'HH:mm:ss')
        </div>
    </div>
</body>
</html>
"@

# Write the HTML file
$outputFullPath = Join-Path $PSScriptRoot $OutputPath
$outputDir = Split-Path $outputFullPath -Parent
if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force
}

$htmlContent | Out-File -FilePath $outputFullPath -Encoding UTF8

Write-Host "Minimal HTML report generated: $OutputPath" -ForegroundColor Green