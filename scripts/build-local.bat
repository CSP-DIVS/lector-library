@echo off
echo 🚀 Building and testing local deployment...

REM Clean previous builds
echo 🧹 Cleaning previous builds...
if exist "backend\Csp.Api\wwwroot" rmdir /s /q "backend\Csp.Api\wwwroot"
if exist "backend\Csp.Api\bin\Release" rmdir /s /q "backend\Csp.Api\bin\Release"
if exist "frontend\csp-web\dist" rmdir /s /q "frontend\csp-web\dist"

REM Build frontend
echo 📦 Building frontend...
cd frontend\csp-web
call npm install
call npm run build

if %errorlevel% neq 0 (
    echo ❌ Frontend build failed
    exit /b 1
)

cd ..\..

REM Copy frontend build to backend wwwroot
echo 📂 Copying frontend to backend...
mkdir "backend\Csp.Api\wwwroot"
xcopy "frontend\csp-web\dist\*" "backend\Csp.Api\wwwroot\" /E /Y

REM Build backend
echo 🔨 Building backend...
cd backend\Csp.Api
dotnet restore
dotnet build --configuration Release

if %errorlevel% neq 0 (
    echo ❌ Backend build failed
    exit /b 1
)

REM Run tests
echo 🧪 Running tests...
cd ..\Csp.Api.Tests
dotnet test --configuration Release

echo ✅ Build completed successfully!
echo 📋 Next steps:
echo    1. Set up GitHub secrets with your Azure publish profile
echo    2. Configure JWT_SECRET_KEY in Azure App Service settings
echo    3. Push to main or deploy/usermanagement branch to trigger deployment

pause
