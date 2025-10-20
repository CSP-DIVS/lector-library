# Simple PowerShell script to execute Payment System tests
param(
    [string]$TestType = "All",  # All, Unit, Integration, E2E
    [string]$OutputPath = "TestResults"
)

Write-Host "=== Payment System Test Execution ===" -ForegroundColor Green
Write-Host "Test Type: $TestType" -ForegroundColor Yellow
Write-Host "Output Path: $OutputPath" -ForegroundColor Yellow
Write-Host ""

# Create output directory if it doesn't exist
if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force
    Write-Host "Created output directory: $OutputPath" -ForegroundColor Green
}

# Set environment variables for testing
$env:CSP_WEB_URL = "http://localhost:5173"
$env:CSP_E2E_HEADLESS = "false"

$totalTests = 0
$passedTests = 0
$failedTests = 0

# Function to run tests
function Run-TestSuite {
    param(
        [string]$ProjectPath,
        [string]$TestName
    )
    
    Write-Host "Running $TestName tests..." -ForegroundColor Cyan
    
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $resultFile = Join-Path $OutputPath "$TestName`_$timestamp.trx"
    $htmlFile = Join-Path $OutputPath "$TestName`_$timestamp.html"
    
    try {
        $testCommand = "dotnet test `"$ProjectPath`" --logger `"trx;LogFileName=$resultFile`" --logger `"html;LogFileName=$htmlFile`" --verbosity normal"
        Write-Host "Executing: $testCommand" -ForegroundColor Gray
        
        $result = Invoke-Expression $testCommand 2>&1
        
        # Parse test results
        $testSummary = $result | Select-String "Tests run: (\d+), Passed: (\d+), Failed: (\d+), Skipped: (\d+)"
        
        if ($testSummary) {
            $total = [int]$testSummary.Matches[0].Groups[1].Value
            $passed = [int]$testSummary.Matches[0].Groups[2].Value
            $failed = [int]$testSummary.Matches[0].Groups[3].Value
            $skipped = [int]$testSummary.Matches[0].Groups[4].Value
            
            $script:totalTests += $total
            $script:passedTests += $passed
            $script:failedTests += $failed
            
            $successRate = if ($total -gt 0) { [math]::Round(($passed / $total) * 100, 2) } else { 0 }
            Write-Host "✓ $TestName completed: $passed/$total passed ($successRate`%)" -ForegroundColor Green
        } else {
            Write-Host "⚠ Could not parse test results for $TestName" -ForegroundColor Yellow
        }
        
        return $true
    }
    catch {
        Write-Host "✗ Error running $TestName tests: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Get project paths
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
        Run-TestSuite -ProjectPath $unitTestPath -TestName "PaymentUnitTests"
    }
    "integration" {
        Run-TestSuite -ProjectPath $integrationTestPath -TestName "PaymentIntegrationTests"
    }
    "e2e" {
        Run-TestSuite -ProjectPath $e2eTestPath -TestName "PaymentE2ETests"
    }
    "all" {
        Write-Host "Running all test suites..." -ForegroundColor Cyan
        Write-Host ""
        
        Write-Host "=== UNIT TESTS ===" -ForegroundColor Magenta
        Run-TestSuite -ProjectPath $unitTestPath -TestName "PaymentUnitTests"
        Write-Host ""
        
        Write-Host "=== INTEGRATION TESTS ===" -ForegroundColor Magenta
        Run-TestSuite -ProjectPath $integrationTestPath -TestName "PaymentIntegrationTests"
        Write-Host ""
        
        Write-Host "=== E2E TESTS ===" -ForegroundColor Magenta
        Run-TestSuite -ProjectPath $e2eTestPath -TestName "PaymentE2ETests"
        Write-Host ""
    }
    default {
        Write-Host "Error: Invalid TestType. Use 'Unit', 'Integration', 'E2E', or 'All'" -ForegroundColor Red
        exit 1
    }
}

# Final summary
Write-Host ""
Write-Host "=== TEST EXECUTION SUMMARY ===" -ForegroundColor Green
Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $failedTests" -ForegroundColor Red
$overallSuccessRate = if ($totalTests -gt 0) { [math]::Round(($passedTests / $totalTests) * 100, 2) } else { 0 }
Write-Host "Success Rate: $overallSuccessRate`%" -ForegroundColor Yellow
Write-Host ""

if ($failedTests -gt 0) {
    Write-Host "⚠ Some tests failed. Check the detailed reports for more information." -ForegroundColor Yellow
    Write-Host "Results saved in: $OutputPath" -ForegroundColor Cyan
    exit 1
} else {
    Write-Host "✓ All tests passed successfully!" -ForegroundColor Green
    Write-Host "Results saved in: $OutputPath" -ForegroundColor Cyan
    exit 0
}
