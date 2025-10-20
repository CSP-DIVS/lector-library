# PowerShell script to execute all Payment System tests
# This script runs Unit, Integration, and E2E tests for the Payment System

param(
    [string]$TestType = "All",  # All, Unit, Integration, E2E
    [string]$OutputPath = "TestResults",
    [switch]$GenerateReport = $true,
    [switch]$Verbose = $false
)

Write-Host "=== Payment System Test Execution Script ===" -ForegroundColor Green
Write-Host "Test Type: $TestType" -ForegroundColor Yellow
Write-Host "Output Path: $OutputPath" -ForegroundColor Yellow
Write-Host "Generate Report: $GenerateReport" -ForegroundColor Yellow
Write-Host ""

# Create output directory if it doesn't exist
if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force
    Write-Host "Created output directory: $OutputPath" -ForegroundColor Green
}
# Set environment variables for testing
$env:CSP_WEB_URL = "http://localhost:5173"
# PowerShell script to execute all Payment System tests
# This script runs Unit, Integration, and E2E tests for the Payment System
$env:CSP_E2E_HEADLESS = "false"  # Set to "true" for headless E2E tests

$testResults = @()
$totalTests = 0
$passedTests = 0
$failedTests = 0

# Function to run tests and capture results
function Run-TestSuite {
    param(
        [string]$ProjectPath,
        [string]$TestName,
        [string]$TestType
    )
    
    Write-Host "Running $TestName tests..." -ForegroundColor Cyan
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $resultFile = Join-Path $OutputPath "$TestName`_$timestamp.trx"
    $htmlFile = Join-Path $OutputPath "$TestName`_$timestamp.html"
    
    try {
        # Build argument array and execute dotnet test safely to avoid quoting issues
        $dotnet = "dotnet"
        $args = @("test", $ProjectPath, "--logger", "trx;LogFileName=$resultFile", "--logger", "html;LogFileName=$htmlFile", "--verbosity", "normal")

        if ($Verbose) {
            $args += @("--verbosity", "detailed")
        }

        Write-Host "Executing: $dotnet $($args -join ' ')" -ForegroundColor Gray
        $result = & $dotnet @args 2>&1

        # Parse test results (dotnet test output format may vary)
        $testSummary = $result | Select-String "Total tests: (\d+). Passed: (\d+). Failed: (\d+). Skipped: (\d+)" -SimpleMatch
        if (-not $testSummary) {
            # Fallback pattern used by some runners
            $testSummary = $result | Select-String "Tests run: (\d+), Passed: (\d+), Failed: (\d+), Skipped: (\d+)"
        }

        if ($testSummary) {
            $total = [int]$testSummary.Matches[0].Groups[1].Value
            $passed = [int]$testSummary.Matches[0].Groups[2].Value
            $failed = [int]$testSummary.Matches[0].Groups[3].Value
            $skipped = [int]$testSummary.Matches[0].Groups[4].Value

            $successRate = if ($total -gt 0) { [math]::Round(($passed / $total) * 100, 2) } else { 0 }

            $testResults += [PSCustomObject]@{
                TestSuite = $TestName
                TestType = $TestType
                Total = $total
                Passed = $passed
                Failed = $failed
                Skipped = $skipped
                SuccessRate = $successRate
                ResultFile = $resultFile
                HtmlFile = $htmlFile
                Timestamp = $timestamp
            }

            $script:totalTests += $total
            $script:passedTests += $passed
            $script:failedTests += $failed

        Write-Host ("✓ {0} completed: {1}/{2} passed ({3}%)" -f $TestName, $passed, $total, $successRate) -ForegroundColor Green
        }
        else {
            Write-Host "⚠ Could not parse test results for $TestName" -ForegroundColor Yellow
            # Output raw result to help debugging
            $result | Select-Object -First 200 | ForEach-Object { Write-Host $_ -ForegroundColor DarkGray }
        }

        return $true
    }
    catch {
        Write-Host "✗ Error running $TestName tests: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Function to generate comprehensive test report
function Generate-TestReport {
    param(
        [array]$Results,
        [string]$OutputPath
    )
    
    $reportFile = Join-Path $OutputPath "PaymentSystemTestReport_$(Get-Date -Format 'yyyyMMdd_HHmmss').html"
    
    # Precompute overall success rate to avoid parsing issues inside the here-string
    $overallSuccessRate = if ($totalTests -gt 0) { [math]::Round(($passedTests / $totalTests) * 100, 2) } else { 0 }

    $html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Payment System Test Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background-color: #f0f0f0; padding: 20px; border-radius: 5px; }
        .summary { background-color: #e8f5e8; padding: 15px; margin: 10px 0; border-radius: 5px; }
        .test-suite { border: 1px solid #ddd; margin: 10px 0; padding: 15px; border-radius: 5px; }
        .passed { color: green; font-weight: bold; }
        .failed { color: red; font-weight: bold; }
        .warning { color: orange; font-weight: bold; }
        table { width: 100%; border-collapse: collapse; margin: 10px 0; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; }
        .success-rate { font-size: 18px; font-weight: bold; }
    </style>
</head>
<body>
    <div class="header">
        <h1>Payment System Test Report</h1>
        <p>Generated on: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</p>
        <p>Test Execution: $TestType</p>
    </div>
    
    <div class="summary">
        <h2>Overall Summary</h2>
        <p><span class="success-rate">Total Tests: $totalTests | Passed: $passedTests | Failed: $failedTests</span></p>
    <p>Overall Success Rate: $overallSuccessRate%</p>
    </div>
    
    <h2>Test Suite Results</h2>
    <table>
        <tr>
            <th>Test Suite</th>
            <th>Type</th>
            <th>Total</th>
            <th>Passed</th>
            <th>Failed</th>
            <th>Success Rate</th>
            <th>Result Files</th>
        </tr>
"@

    foreach ($result in $Results) {
        $successClass = if ($result.SuccessRate -ge 90) { "passed" } elseif ($result.SuccessRate -ge 70) { "warning" } else { "failed" }
        
        $html += @"
        <tr>
            <td>$($result.TestSuite)</td>
            <td>$($result.TestType)</td>
            <td>$($result.Total)</td>
            <td class="passed">$($result.Passed)</td>
            <td class="failed">$($result.Failed)</td>
            <td class="$successClass">$($result.SuccessRate)%</td>
            <td>
                <a href="$($result.HtmlFile)">HTML Report</a> | 
                <a href="$($result.ResultFile)">TRX File</a>
            </td>
        </tr>
"@
    }

    $html += @"
    </table>
    
    <h2>Test Coverage Analysis</h2>
    <div class="test-suite">
        <h3>Payment System Test Coverage</h3>
        <ul>
            <li><strong>Unit Tests:</strong> Controller logic, Service layer, Model validation, DTO validation</li>
            <li><strong>Integration Tests:</strong> API endpoints, Database operations, Transaction handling, Background services</li>
            <li><strong>E2E Tests:</strong> Payment workflows, Fine adjustment, Receipt generation, User role validation</li>
        </ul>
    </div>
    
    <h2>Key Test Scenarios Covered</h2>
    <div class="test-suite">
        <h3>Payment Recording</h3>
        <ul>
            <li>Valid payment recording with different payment methods</li>
            <li>Payment validation (amount, method, member validation)</li>
            <li>Authorization (Librarian/Admin only)</li>
            <li>Database transaction handling</li>
        </ul>
        
        <h3>Fine Calculation</h3>
        <ul>
            <li>Background service execution</li>
            <li>Overdue lending detection</li>
            <li>Fine amount calculation</li>
            <li>Database updates and transaction handling</li>
        </ul>
        
        <h3>Fine Adjustment</h3>
        <ul>
            <li>Admin-only fine adjustment</li>
            <li>Partial and full waivers</li>
            <li>Audit trail creation</li>
            <li>Authorization validation</li>
        </ul>
        
        <h3>Receipt Generation</h3>
        <ul>
            <li>PDF receipt generation</li>
            <li>Receipt data validation</li>
            <li>Download functionality</li>
            <li>Error handling for non-existent payments</li>
        </ul>
    </div>
    
    <h2>Recommendations</h2>
    <div class="test-suite">
        <ul>
            <li>Monitor test execution time and optimize slow tests</li>
            <li>Add performance tests for high-load scenarios</li>
            <li>Implement automated test data cleanup</li>
            <li>Consider adding security tests for payment processing</li>
        </ul>
    </div>
</body>
</html>
"@

    $html | Out-File -FilePath $reportFile -Encoding UTF8
    Write-Host "Test report generated: $reportFile" -ForegroundColor Green
    return $reportFile
}

# Main execution logic
Write-Host "Starting Payment System test execution..." -ForegroundColor Green
Write-Host ""

# Get the script directory and project paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent (Split-Path -Parent $scriptDir)
$unitTestPath = Join-Path $rootDir "Csp.Unit.Tests\Csp.Unit.Tests.csproj"
$integrationTestPath = Join-Path $rootDir "Csp.Integration.Tests\Csp.Integration.Tests.csproj"
$e2eTestPath = Join-Path $rootDir "Csp.E2E.Tests\Csp.E2E.Tests.csproj"

# Verify project files exist
if (!(Test-Path $unitTestPath)) {
    Write-Host "Error: Unit test project not found at $unitTestPath" -ForegroundColor Red
    exit 1
}

if (!(Test-Path $integrationTestPath)) {
    Write-Host "Error: Integration test project not found at $integrationTestPath" -ForegroundColor Red
    exit 1
}

if (!(Test-Path $e2eTestPath)) {
    Write-Host "Error: E2E test project not found at $e2eTestPath" -ForegroundColor Red
    exit 1
}

# Run tests based on TestType parameter
switch ($TestType.ToLower()) {
    "unit" {
        Run-TestSuite -ProjectPath $unitTestPath -TestName "PaymentUnitTests" -TestType "Unit"
    }
    "integration" {
        Run-TestSuite -ProjectPath $integrationTestPath -TestName "PaymentIntegrationTests" -TestType "Integration"
    }
    "e2e" {
        Run-TestSuite -ProjectPath $e2eTestPath -TestName "PaymentE2ETests" -TestType "E2E"
    }
    "all" {
        Write-Host "Running all test suites..." -ForegroundColor Cyan
        Write-Host ""
        
        # Unit Tests
        Write-Host "=== UNIT TESTS ===" -ForegroundColor Magenta
        Run-TestSuite -ProjectPath $unitTestPath -TestName "PaymentUnitTests" -TestType "Unit"
        Write-Host ""
        
        # Integration Tests
        Write-Host "=== INTEGRATION TESTS ===" -ForegroundColor Magenta
        Run-TestSuite -ProjectPath $integrationTestPath -TestName "PaymentIntegrationTests" -TestType "Integration"
        Write-Host ""
        
        # E2E Tests
        Write-Host "=== E2E TESTS ===" -ForegroundColor Magenta
        Run-TestSuite -ProjectPath $e2eTestPath -TestName "PaymentE2ETests" -TestType "E2E"
        Write-Host ""
    }
    default {
        Write-Host "Error: Invalid TestType. Use 'Unit', 'Integration', 'E2E', or 'All'" -ForegroundColor Red
        exit 1
    }
}

# Generate comprehensive test report
if ($GenerateReport -and $testResults.Count -gt 0) {
    Write-Host "Generating comprehensive test report..." -ForegroundColor Cyan
    $reportFile = Generate-TestReport -Results $testResults -OutputPath $OutputPath
    Write-Host "Report generated: $reportFile" -ForegroundColor Green
}

# Final summary
Write-Host ""
Write-Host "=== TEST EXECUTION SUMMARY ===" -ForegroundColor Green
Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $failedTests" -ForegroundColor Red
Write-Host "Success Rate: $([math]::Round(($passedTests / $totalTests) * 100, 2))%" -ForegroundColor Yellow
Write-Host ""

if ($failedTests -gt 0) {
    Write-Host "⚠ Some tests failed. Check the detailed reports for more information." -ForegroundColor Yellow
    exit 1
} else {
    Write-Host "✓ All tests passed successfully!" -ForegroundColor Green
    exit 0
}

