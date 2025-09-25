# Auto-detect TRX file and generate enhanced report
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
    # Authentication & Security - High Priority
    'ChangePassword' = 'P1 - CRITICAL'
    'UpdateUserStatus_ReturnsFalse_WhenActorTriesToChangeOwnStatus' = 'P2 - HIGH'
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
        'P1 - CRITICAL' { return '#dc3545' }
        'P2 - HIGH' { return '#fd7e14' }
        'P3 - MEDIUM' { return '#ffc107' }
        'P4 - LOW' { return '#28a745' }
        'P5 - INFO' { return '#17a2b8' }
        default { return '#6c757d' }
    }
}

# Extract test data
$counters = $testResults.TestRun.ResultSummary.Counters
$total = [int]$counters.total
$passed = [int]$counters.passed
$failed = [int]$counters.failed
$skipped = if($counters.skipped) { [int]$counters.skipped } else { 0 }
$passRate = if($total -gt 0) { [math]::Round(($passed / $total) * 100, 2) } else { 0 }

# Calculate duration properly
try {
    $startTime = [DateTime]::Parse($testResults.TestRun.Times.start)
    $finishTime = [DateTime]::Parse($testResults.TestRun.Times.finished)
    $duration = ($finishTime - $startTime).ToString("mm\:ss")
} catch {
    $duration = "N/A"
}

# Process test results and add priority information
$results = $testResults.TestRun.Results.UnitTestResult
$groupedResults = $results | Group-Object { ($_.testName -split '\.')[0] }

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
    <title>Unit Test Report with Priority Classification</title>
    <meta charset="utf-8">
    <style>
        body { font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 20px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); min-height: 100vh; }
        .container { max-width: 1400px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 12px; box-shadow: 0 8px 32px rgba(0,0,0,0.15); }
        .header { text-align: center; color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 20px; margin-bottom: 30px; }
        .header h1 { font-size: 2.5em; margin-bottom: 10px; }
        .header h2 { font-size: 1.3em; color: #7f8c8d; margin: 0; }
        
        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin: 30px 0; }
        .summary-card { padding: 20px; border-radius: 10px; text-align: center; color: white; box-shadow: 0 4px 15px rgba(0,0,0,0.1); transition: transform 0.2s; }
        .summary-card:hover { transform: translateY(-2px); }
        .summary-card h3 { margin: 0 0 15px 0; font-size: 16px; opacity: 0.9; }
        .summary-card h2 { margin: 0; font-size: 36px; font-weight: bold; }
        .summary-card.total { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }
        .summary-card.passed { background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%); }
        .summary-card.failed { background: linear-gradient(135deg, #f44336 0%, #da190b 100%); }
        .summary-card.skipped { background: linear-gradient(135deg, #ff9800 0%, #f57c00 100%); }
        .summary-card.rate { background: linear-gradient(135deg, #17a2b8 0%, #138496 100%); }
        
        .priority-section { margin: 30px 0; padding: 20px; background: #f8f9fa; border-radius: 8px; }
        .priority-section h3 { color: #495057; margin-bottom: 20px; }
        .priority-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; }
        .priority-card { padding: 15px; background: white; border-radius: 8px; border-left: 5px solid; box-shadow: 0 2px 5px rgba(0,0,0,0.1); }
        .priority-card h4 { margin: 0 0 10px 0; font-size: 14px; }
        .priority-card h3 { margin: 0; font-size: 24px; }
        
        .test-details { margin-top: 40px; }
        .test-class { margin: 20px 0; border: 1px solid #dee2e6; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 5px rgba(0,0,0,0.05); }
        .test-class-header { background: linear-gradient(135deg, #3498db 0%, #2980b9 100%); color: white; padding: 20px; font-weight: bold; font-size: 18px; }
        .test-method { padding: 15px 20px; border-bottom: 1px solid #eee; display: flex; justify-content: space-between; align-items: center; }
        .test-method:last-child { border-bottom: none; }
        .test-method:hover { background-color: #f8f9fa; }
        
        .test-info { flex-grow: 1; }
        .test-name { font-weight: 500; margin-bottom: 5px; }
        .test-details-small { font-size: 0.9em; color: #6c757d; }
        
        .priority-badge { 
            padding: 6px 12px; 
            border-radius: 20px; 
            font-size: 12px; 
            font-weight: bold; 
            color: white; 
            margin-left: 10px;
            white-space: nowrap;
        }
        
        .test-method.passed { background-color: #d4edda; border-left: 5px solid #28a745; }
        .test-method.failed { background-color: #f8d7da; border-left: 5px solid #dc3545; }
        .test-method.skipped { background-color: #fff3cd; border-left: 5px solid #ffc107; }
        
        .status-icon { font-size: 18px; margin-right: 10px; }
        
        .info-section { background: linear-gradient(135deg, #e8f4fd 0%, #d6eef7 100%); padding: 25px; border-radius: 8px; margin: 30px 0; }
        .methodology { background-color: #f8f9fa; padding: 25px; border-radius: 8px; margin: 30px 0; }
        .methodology h3 { color: #495057; margin-top: 0; }
        
        .timestamp { text-align: center; color: #6c757d; margin-top: 40px; font-style: italic; padding-top: 20px; border-top: 1px solid #dee2e6; }
        
        .legend { margin: 20px 0; padding: 15px; background: #f8f9fa; border-radius: 8px; }
        .legend h4 { margin-top: 0; color: #495057; }
        .legend-item { display: inline-block; margin: 5px 15px 5px 0; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>&#128218; Unit Test Report with Priority Classification</h1>
            <h2>Lector Library Backend - Test Results with Priority Levels</h2>
        </div>

        <div class="summary">
            <div class="summary-card total">
                <h3>Total Tests</h3>
                <h2>$total</h2>
            </div>
            <div class="summary-card passed">
                <h3>&#10004; Passed</h3>
                <h2>$passed</h2>
            </div>
            <div class="summary-card failed">
                <h3>&#10060; Failed</h3>
                <h2>$failed</h2>
            </div>
            <div class="summary-card skipped">
                <h3>&#10145;&#65039; Skipped</h3>
                <h2>$skipped</h2>
            </div>
            <div class="summary-card rate">
                <h3>Pass Rate</h3>
                <h2>$passRate%</h2>
            </div>
        </div>

        <div class="priority-section">
            <h3>&#127991;&#65039; Test Distribution by Priority Level</h3>
            <div class="priority-grid">
"@

# Add priority distribution cards
foreach ($priority in @('P1 - CRITICAL', 'P2 - HIGH', 'P3 - MEDIUM', 'P4 - LOW', 'P5 - INFO')) {
    $count = if ($priorityCounts.ContainsKey($priority)) { $priorityCounts[$priority] } else { 0 }
    $color = Get-PriorityColor $priority
    $icon = switch ($priority) {
        'P1 - CRITICAL' { '&#128308;' }
        'P2 - HIGH' { '&#128992;' }
        'P3 - MEDIUM' { '&#128993;' }
        'P4 - LOW' { '&#128994;' }
        'P5 - INFO' { '&#128309;' }
        default { '⚪' }
    }
    
    $htmlContent += @"
                <div class="priority-card" style="border-left-color: $color;">
                    <h4>$icon $priority</h4>
                    <h3>$count</h3>
                </div>
"@
}

$htmlContent += @"
            </div>
        </div>

        <div class="legend">
            <h4>&#127991;&#65039; Priority Level Legend</h4>
            <div class="legend-item">&#128308; P1 - CRITICAL: System crashes, security issues</div>
            <div class="legend-item">&#128992; P2 - HIGH: Core functionality broken</div>
            <div class="legend-item">&#128993; P3 - MEDIUM: Feature limitations, API issues</div>
            <div class="legend-item">&#128994; P4 - LOW: Minor issues, validation problems</div>
            <div class="legend-item">&#128309; P5 - INFO: Enhancements, code quality</div>
        </div>

        <div class="info-section">
            <h3>&#128202; Test Execution Information</h3>
            <p><strong>Execution Time:</strong> $duration</p>
            <p><strong>Report Generated:</strong> $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</p>
            <p><strong>Test Framework:</strong> xUnit with .NET 8.0</p>
            <p><strong>Priority Classification:</strong> Automatic assignment based on test category and business impact</p>
        </div>

        <div class="test-details">
            <h2>&#128203; Detailed Test Results with Priority Classification</h2>
"@

# Add grouped test results
foreach ($group in $groupedResults) {
    $className = $group.Name
    $classTests = $group.Group
    $classPassed = ($classTests | Where-Object { $_.outcome -eq 'Passed' }).Count
    $classFailed = ($classTests | Where-Object { $_.outcome -eq 'Failed' }).Count
    $classTotal = $classTests.Count

    $htmlContent += @"
            <div class="test-class">
                <div class="test-class-header">
                    $className ($classPassed/$classTotal passed)
                </div>
"@

    foreach ($test in $classTests) {
        $methodName = ($test.testName -split '\.')[-1]
        $outcome = $test.outcome.ToLower()
        $testDuration = if($test.duration) { $test.duration } else { 'N/A' }
        $priority = Get-TestPriority $test.testName
        $priorityColor = Get-PriorityColor $priority
                $statusIcon = if($outcome -eq 'passed') { '&#10004;' } elseif($outcome -eq 'failed') { '&#10060;' } else { '&#10145;' }

        $htmlContent += @"
                <div class="test-method $outcome">
                    <div class="test-info">
                        <div class="test-name">
                            <span class="status-icon">$statusIcon</span>
                            $methodName
                        </div>
                        <div class="test-details-small">
                            Duration: $testDuration
                        </div>
                    </div>
                    <div class="priority-badge" style="background-color: $priorityColor;">
                        $priority
                    </div>
                </div>
"@
    }
    
    $htmlContent += "            </div>"
}

$htmlContent += @"
        </div>

        <div class="methodology">
            <h3>🧪 Testing Methodology and Priority System</h3>
            <ul>
                <li><strong>Framework:</strong> xUnit with .NET 8.0 for comprehensive unit testing</li>
                <li><strong>Mocking:</strong> Moq library for dependency isolation and controlled testing</li>
                <li><strong>Priority Assignment:</strong> Automatic classification based on test category and business impact</li>
                <li><strong>Classification Logic:</strong> Authentication/Security (P1), Core Business Logic (P2), API/Controllers (P3), Models/Validation (P4)</li>
                <li><strong>Reporting:</strong> Enhanced HTML reports with priority visualization and actionable insights</li>
            </ul>
        </div>

        <div class="timestamp">
            Report automatically generated on: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') | Enhanced with Priority Classification System
        </div>
    </div>
</body>
</html>
"@

# Write the HTML to file
# Write the HTML content to file with proper encoding
$htmlContent | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "Enhanced HTML report with priority levels generated: $OutputPath" -ForegroundColor Green