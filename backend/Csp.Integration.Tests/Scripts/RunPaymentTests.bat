@echo off
echo === Payment System Test Execution ===
echo.

REM Set environment variables
set CSP_WEB_URL=http://localhost:5173
set CSP_E2E_HEADLESS=false

REM Create output directory
if not exist "TestResults" mkdir TestResults

REM Get the directory where this batch file is located
set SCRIPT_DIR=%~dp0
set ROOT_DIR=%SCRIPT_DIR%..\..

echo Running Unit Tests...
cd /d "%ROOT_DIR%\Csp.Unit.Tests"
dotnet test --logger "trx;LogFileName=TestResults\PaymentUnitTests.trx" --logger "html;LogFileName=TestResults\PaymentUnitTests.html" --verbosity normal
if %ERRORLEVEL% neq 0 (
    echo Unit tests failed!
    pause
    exit /b 1
)

echo.
echo Running Integration Tests...
cd /d "%ROOT_DIR%\Csp.Integration.Tests"
dotnet test --logger "trx;LogFileName=TestResults\PaymentIntegrationTests.trx" --logger "html;LogFileName=TestResults\PaymentIntegrationTests.html" --verbosity normal
if %ERRORLEVEL% neq 0 (
    echo Integration tests failed!
    pause
    exit /b 1
)

echo.
echo Running E2E Tests...
cd /d "%ROOT_DIR%\Csp.E2E.Tests"
dotnet test --logger "trx;LogFileName=TestResults\PaymentE2ETests.trx" --logger "html;LogFileName=TestResults\PaymentE2ETests.html" --verbosity normal
if %ERRORLEVEL% neq 0 (
    echo E2E tests failed!
    pause
    exit /b 1
)

echo.
echo === All tests completed successfully! ===
echo Results saved in: TestResults folder
pause

