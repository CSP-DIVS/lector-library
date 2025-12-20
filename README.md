# Lector Library – Full-Stack Library Management System

![License: MIT](https://img.shields.io/badge/License-MIT-green.svg) ![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4) ![React 18](https://img.shields.io/badge/React-18-149ECA) ![Node 20+](https://img.shields.io/badge/Node-20%2F22-43853D)

Modern, role-aware library management platform with cataloging, lending, reservations, fines, and admin tooling. Built with ASP.NET Core and React (Vite), deployed to Azure with GitHub Actions.

## Table of Contents
- [Live Demo](#live-demo)
- [Features](#features)
- [Architecture](#architecture)
- [Requirements](#requirements)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Project Layout](#project-layout)
- [Testing](#testing)
- [Docker](#docker)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)
- [Docs](#docs)

## Live Demo
Production: https://lector-lms-bpb2b4hzeqaqdth5.southindia-01.azurewebsites.net

Test accounts:
- Admin: admin / admin123
- Librarian: librarian / librarian123
- Member: member / member123

## Features
- JWT auth with role policies (Admin, Librarian, Member)
- Book catalog: search, filter, stock management
- Lending: due dates, renewals, returns
- Reservations with queue management
- Automatic fines (Rs. 20/day) and payment tracking
- User management, profile updates, password changes
- Role-specific dashboards and stats
- PDF report generation for fines

## Architecture
- Backend: ASP.NET Core 8, ADO.NET (MySQL), JWT (HS256)
- Frontend: React 18, Vite, Axios
- Database: MySQL (Azure Flexible Server in prod)
- Hosting/CI: Azure App Service with GitHub Actions

## Requirements
- .NET 8 SDK
- Node 20.19+ or 22.12+
- MySQL 8.0 (local or container)
- Git

## Quick Start
1) Backend API
```
cd backend/Csp.Api
dotnet restore
dotnet run
# Default: http://localhost:5192
```
The API ensures tables exist and seeds default users on first run.

2) Frontend
```
cd frontend/csp-web
echo "VITE_API_URL=http://localhost:5192" > .env
npm install
npm run dev
# Default: http://localhost:5173
```

3) VS Code tasks (optional)
- Start both: Start Full Stack (Frontend + Backend)
- Individually: Backend: Start Development Server, Frontend: Start Development Server

## Configuration
- Backend (appsettings or env vars)
  - ConnectionStrings:DefaultConnection=Server=localhost;Port=3306;Database=lector;User Id=root;Password=pass;SslMode=None
  - Jwt:Secret (32+ chars), Jwt:Issuer, Jwt:Audience
  - CORS allows http://localhost:5173 by default
- Frontend env
  - VITE_API_URL (defaults to http://localhost:5192)

Security: use a strong Jwt:Secret, lock CORS to trusted origins, enable HTTPS in production.

## Project Layout
```
backend/
  Csp.Api/               API (ASP.NET Core)
  Csp.Api.Tests/         Unit + integration tests
  Csp.Integration.Tests/ Integration tests (Testcontainers)
  Csp.E2E.Tests/         Selenium end-to-end tests
frontend/
  csp-web/               React app (Vite)
infra/
  docker-compose.yml     Dev compose for API + DB + web
documentation/           Deployment, fixes, quick guides
```

## Testing
- Unit: dotnet test backend/Csp.Api.Tests/Csp.Api.Tests.csproj
- Integration: dotnet test backend/Csp.Integration.Tests/Csp.Integration.Tests.csproj (Docker Desktop required)
- E2E (Selenium):
  - cd backend/Csp.E2E.Tests
  - set E2E_BASE_URL=http://localhost:5173
  - dotnet test
- VS Code tasks: Run All Tests, Backend: Run Tests, Frontend: Run Tests

## Docker
- Local stack: docker compose -f infra/docker-compose.yml up --build
- Images: infra/backend.Dockerfile, infra/frontend.Dockerfile
- Ensure env values match your DB/JWT settings.

## Deployment
Configured for Azure App Service with GitHub Actions CI/CD.
- App Service: lector-lms-bpb2b4hzeqaqdth5.southindia-01.azurewebsites.net
- Database: Azure Database for MySQL
- Static files: frontend build can be served from backend deployment

See documentation/DEPLOYMENT.md for full steps.

## Troubleshooting
- 500 on login (HS256): set Jwt:Secret to 32+ chars
- 404 on /api/users/my-profile: include Authorization: Bearer <token> and restart API after config changes
- Frontend build errors: use Node 20.19+ or 22.12+

## Docs
- Deployment: documentation/DEPLOYMENT.md
- Quick references: documentation/TASK_QUICK_REFERENCE.md
- Auth fix: documentation/FRONTEND_AUTH_FIX.md
- Fines: documentation/QUICK_START_FINES.md
- Receipts testing: documentation/RECEIPT_TESTING_GUIDE.md
- More: documentation/





