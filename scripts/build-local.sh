#!/bin/bash

echo "🚀 Building and testing local deployment..."

# Clean previous builds
echo "🧹 Cleaning previous builds..."
rm -rf backend/Csp.Api/wwwroot
rm -rf backend/Csp.Api/bin/Release
rm -rf frontend/csp-web/dist

# Build frontend
echo "📦 Building frontend..."
cd frontend/csp-web
npm install
npm run build

if [ $? -ne 0 ]; then
    echo "❌ Frontend build failed"
    exit 1
fi

cd ../..

# Copy frontend build to backend wwwroot
echo "📂 Copying frontend to backend..."
mkdir -p backend/Csp.Api/wwwroot
cp -r frontend/csp-web/dist/* backend/Csp.Api/wwwroot/

# Build backend
echo "🔨 Building backend..."
cd backend/Csp.Api
dotnet restore
dotnet build --configuration Release

if [ $? -ne 0 ]; then
    echo "❌ Backend build failed"
    exit 1
fi

# Run tests
echo "🧪 Running tests..."
cd ../Csp.Api.Tests
dotnet test --configuration Release

echo "✅ Build completed successfully!"
echo "📋 Next steps:"
echo "   1. Set up GitHub secrets with your Azure publish profile"
echo "   2. Configure JWT_SECRET_KEY in Azure App Service settings"
echo "   3. Push to main or deploy/usermanagement branch to trigger deployment"
echo "   4. Your app will be available at: https://lms-cyf6d5f2fqhvf7b3.southindia-01.azurewebsites.net"
