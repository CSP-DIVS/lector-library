# Azure Deployment Guide

## Prerequisites

1. **Azure Account**: Ensure you have an active Azure subscription
2. **Azure App Service**: Web app named "lector-lms" should be created
3. **Azure Database for MySQL**: Server "lector-lms-server.mysql.database.azure.com" with database "lector-lms-database"
4. **GitHub Repository**: This code should be in a GitHub repository

## Setup Steps

### 1. Configure Azure App Service

1. Go to Azure Portal → App Services → lector-lms-bpb2b4hzeqaqdth5
2. Under **Configuration** → **Application settings**, add:
   - `JWT_SECRET_KEY`: A secure random string (e.g., generate a 256-bit key)
   - `ASPNETCORE_ENVIRONMENT`: `Production`

### 2. Get Publish Profile

1. In Azure Portal → App Services → lector-lms-bpb2b4hzeqaqdth5
2. Click **Get publish profile** to download the .publishsettings file
3. Copy the entire content of this file

### 3. Configure GitHub Secrets

1. Go to your GitHub repository → Settings → Secrets and variables → Actions
2. Add a new repository secret:
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: Paste the entire content of the publish profile file

### 4. Database Configuration

The application is configured to connect to:
- **Server**: lector-lms-server.mysql.database.azure.com
- **Database**: lector-lms-database
- **User**: bwqwxjamnf
- **Password**: WiIPxDf$$vKWvowD

Make sure your MySQL server allows connections from Azure App Services.

### 5. Deploy

The deployment will happen automatically when you push to the `main` or `deploy/usermanagement` branch.

You can also trigger a manual deployment:
1. Go to GitHub repository → Actions
2. Select "Deploy to Azure App Service" workflow
3. Click "Run workflow"

## Local Development

### Backend
```bash
cd backend/Csp.Api
dotnet run
```

### Frontend
```bash
cd frontend/csp-web
npm install
npm run dev
```

## Production URLs

- **App URL**: https://lector-lms-bpb2b4hzeqaqdth5.southindia-01.azurewebsites.net
- **API Health Check**: https://lector-lms-bpb2b4hzeqaqdth5.southindia-01.azurewebsites.net/api/health/db
- **Swagger** (in development): https://lector-lms-bpb2b4hzeqaqdth5.southindia-01.azurewebsites.net/swagger

## Troubleshooting

### Common Issues

1. **Database Connection Fails**:
   - Verify MySQL server allows Azure App Service connections
   - Check if SSL is properly configured
   - Verify connection string in Azure App Service configuration

2. **JWT Token Issues**:
   - Ensure `JWT_SECRET_KEY` is set in Azure App Service application settings
   - Key should be at least 256 bits (32 characters)

3. **CORS Issues**:
   - Verify that your domain is included in the CORS policy
   - Check `AllowedOrigins` in appsettings.Production.json

4. **Static Files Not Loading**:
   - Ensure the build process copies frontend files to wwwroot
   - Check that `UseStaticFiles()` and `MapFallbackToFile("index.html")` are configured

### Logs

View application logs in Azure Portal:
1. App Services → lector-lms → Log stream
2. Or download logs from App Services → lector-lms → Advanced Tools → Kudu

## Environment Variables

The application uses these environment variables:

- `JWT_SECRET_KEY`: Secret key for JWT token generation
- `ASPNETCORE_ENVIRONMENT`: Set to "Production" for production deployment
- `ConnectionStrings__DefaultConnection`: MySQL connection string (configured in appsettings)
