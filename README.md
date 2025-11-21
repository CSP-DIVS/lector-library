# Lector Library Management System

A full-stack library management system with book cataloging, lending, reservations, and fines management. Built with ASP.NET Core 8 and React 18.

## Live Demo

**Production**: https://lector-lms-bpb2b4hzeqaqdth5.southindia-01.azurewebsites.net

**Test Accounts**:
- Admin: `admin` / `admin123`
- Librarian: `librarian` / `librarian123`
- Member: `member` / `member123`

## Features

- JWT authentication with role-based access (Admin, Librarian, Member)
- Book catalog with search, filtering, and stock management
- Lending system with due dates, renewals, and return processing
- Book reservations with queue management
- Automatic fine calculation (Rs. 20/day) and payment tracking
- User management with profile updates
- Role-specific dashboards with statistics
- PDF report generation for fines

## Tech Stack

**Backend**: .NET 8, ASP.NET Core, ADO.NET, MySQL, JWT  
**Frontend**: React 18, Vite, Axios  
**Database**: Azure MySQL Flexible Server  
**Hosting**: Azure App Services with GitHub Actions CI/CD

## Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- MySQL 8.0

### Setup

1. **Clone and setup database**
```bash
git clone https://github.com/CSP-DIVS/lector-library.git
cd lector-library

# Create MySQL database
mysql -u root -p
CREATE DATABASE lector_lms_dev;
```

2. **Configure backend** (`backend/Csp.Api/appsettings.Development.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=lector_lms_dev;User Id=root;Password=your_password;SslMode=None"
  },
  "Jwt": {
    "Secret": "your-32-character-secret-key-here",
    "Issuer": "LectorLibrary",
    "Audience": "LectorLibraryUsers"
  }
}
```

3. **Run backend**
```bash
cd backend/Csp.Api
dotnet restore
dotnet run
# Runs on http://localhost:5192
```

4. **Run frontend**
```bash
cd frontend/csp-web
echo "VITE_API_URL=http://localhost:5192" > .env
npm install
npm run dev
# Runs on http://localhost:5173
```

## Testing

```bash
# Unit tests
cd backend/Csp.Api.Tests
dotnet test

# Integration tests
cd backend/Csp.Integration.Tests
dotnet test

# E2E tests (requires running application)
cd backend/Csp.E2E.Tests
dotnet test
```

## Deployment

See [DEPLOYMENT.md](DEPLOYMENT.md) for complete Azure deployment instructions including:
- Azure App Service setup
- MySQL Flexible Server configuration
- GitHub Actions CI/CD pipeline
- Environment variables and security





