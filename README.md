# Lector Library – Full‑Stack Library Management System

Modern web application for library user management with role‑based access, built on ASP.NET Core (C#) and React (Vite).

## 🚀 Deployment

**Live Application**: https://lms-cyf6d5f2fqhvf7b3.southindia-01.azurewebsites.net

This application is deployed on Azure App Services with automated CI/CD through GitHub Actions.

For deployment instructions, see [DEPLOYMENT.md](DEPLOYMENT.md).

## Features
- JWT authentication with role claims and CORS‑enabled API
- Role‑based dashboards: Member, Librarian, Administrator
- Admin member management: register, list/search, edit, activate/deactivate
- Member self‑service: profile update and password change
- Audit log for administrative actions

## Tech Stack
- Backend: .NET 8, ASP.NET Core, ADO.NET (MySQL), JWT (HS256)
- Database: MySQL
- Frontend: React 18, Vite, Axios
- Testing: xUnit (unit/integration), Selenium WebDriver (E2E)

## Repository Layout
```
backend/
  Csp.Api/               # ASP.NET Core API
  Csp.Api.Tests/         # Unit + integration tests (WebApplicationFactory)
  Csp.E2E.Tests/         # Selenium end‑to‑end tests
frontend/
  csp-web/               # React app (Vite)
infra/
  docker-compose.yml     # Dev compose for API + DB + web
```

## Backend
### Configuration
Set environment variables or appsettings for database and JWT:
```
ConnectionStrings:DefaultConnection=Server=localhost;Port=3306;Database=lector;User Id=root;Password=pass;SslMode=None
Jwt:Secret=0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef
Jwt:Issuer=csp-api
Jwt:Audience=csp-web
```

Notes:
- Secret must be ≥ 256 bits for HS256. The app pads a short secret in dev, but set a strong key in prod.
- CORS allows http://localhost:5173 by default.

### Run API
```
cd backend/Csp.Api
dotnet run
```
On start, the API will:
- Ensure tables exist (`users`, `audit_log`)
- Seed default users: `admin`, `librarian`, `member`, `testuser`

### Key Endpoints
- POST `/api/auth/login` – username/email + password → JWT
- POST `/api/auth/register` – basic registration (Member)
- GET `/api/users/my-profile` – current user profile
- PUT `/api/users/my-profile` – update profile
- PUT `/api/users/my-password` – change password (current required)
- Admin only:
  - POST `/api/users/members` – create member
  - GET `/api/users/members` – list/search/paginate
  - PUT `/api/users/members/{id}` – edit member
  - PUT `/api/users/{id}/status` – activate/deactivate with self‑guard

### Authentication
JWT includes claims: `sub` (id), `unique_name` (username), `nameidentifier` (id), `name` (username), `role`. API policies:
- `RequireAdmin`: Administrator
- `RequireLibrarian`: Librarian or Administrator

## Frontend
### Setup & Run
```
cd frontend/csp-web
npm install
npm run dev
```
Configure API base URL with `VITE_API_BASE` (defaults to http://localhost:5192).

### UX
- Modern login with spinner, toasts, and transitions
- Role‑aware dashboards with sidebar and stat cards
- Member management with search, pagination, inline edit, status badges, confirm modal
- My Profile with editable info and password change

## Testing
### Unit/Integration
```
cd backend/Csp.Api.Tests
dotnet test
```
Includes hashing/verification, JWT generation/validation, validators, and API slice tests with a test auth scheme.

### End‑to‑End (Selenium)
Requires Chrome installed.
```
cd backend/Csp.E2E.Tests
set E2E_BASE_URL=http://localhost:5173
dotnet test
```

## Docker (optional)
Use `infra/docker-compose.yml` to spin up API + MySQL. Ensure environment variables match your local setup.

## Security & Production
- Provide a strong `Jwt:Secret` via secure configuration
- Restrict CORS to trusted origins
- Use HTTPS and secure cookies if applicable
- Add rate limiting on auth endpoints

## Azure Deployment
The application is configured for deployment to Azure App Services:
- **App Service**: lms-cyf6d5f2fqhvf7b3.southindia-01.azurewebsites.net
- **Database**: Azure Database for MySQL
- **CI/CD**: GitHub Actions with automated builds and deployments
- **Static Files**: Frontend build integrated with backend for single deployment

See [DEPLOYMENT.md](DEPLOYMENT.md) for complete setup instructions.

## Troubleshooting
- Login 500 with HS256: set a 32+ char `Jwt:Secret`
- 404 on `/api/users/my-profile`: ensure request has `Authorization: Bearer <token>` and API has restarted after updates
- Vite Node version: Node 20.19+ or 22.12+
