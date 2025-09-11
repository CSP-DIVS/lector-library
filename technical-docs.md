# Technical Documentation for Lector Library Management System

## 1. Project Overview
Lector is a web-based library management application that supports user roles (Administrator, Librarian, Member) for managing books, users, loans, reservations, fines, and profiles. Key features include:
- User authentication and role-based access control (RBAC) using JWT.
- CRUD operations for users and members.
- Lending/reservation management.
- Profile management and password changes.
- Dashboard for role-specific actions.

The system emphasizes security (e.g., hashed passwords, JWT validation), testing (unit, integration, E2E), and containerization for easy deployment.

## 2. Architecture
### High-Level Design
- **Monolithic Backend with Microservices Potential**: The backend is a single ASP.NET Core Web API, but modular services (e.g., `IUserService`, `IJwtTokenService`) allow future splitting.
- **Layered Structure**:
  - **Presentation Layer**: API controllers (e.g., `AuthController`, `UsersController`).
  - **Business Layer**: Services (e.g., `UserService` for user operations, password hashing/verification).
  - **Data Layer**: ADO.NET with MySQL for database interactions (e.g., in `UserService.InitializeDatabaseAsync` for schema setup and seeding).
- **Frontend**: Single-Page Application (SPA) using React, with components for dashboards, forms, and UI elements (e.g., `Sidebar`, `Header`, `Modal`).
- **Database**: MySQL 8.0, with tables for Users (including roles, status, hashed passwords).
- **Infrastructure**: Docker Compose orchestrates MySQL DB, .NET backend, and NGINX-served React frontend.
- **Authentication**: JWT Bearer tokens, with role-based policies (e.g., "RequireAdmin").
- **Communication**: Frontend uses Axios for API calls (e.g., to `/api/auth/login`, `/api/users`).
- **Deployment Model**: Containerized for local/dev; suitable for cloud (e.g., AWS ECS, Kubernetes).

### Data Flow
1. User logs in via frontend → API (`/api/auth/login`) validates credentials → Issues JWT.
2. Authenticated requests use JWT in headers → Backend authorizes via policies.
3. Database operations use parameterized queries to prevent SQL injection.

### Dependencies
- **Backend**: .NET 8, MySql.Data, Microsoft.AspNetCore.Authentication.JwtBearer, Swashbuckle (Swagger).
- **Frontend**: React 19, Axios, ESLint, Vite.
- **Database**: MySQL 8.0.
- **Infra**: Docker, NGINX, Node 20.
- **Testing**: xUnit, Selenium WebDriver.
- **CI**: GitHub Actions.

## 3. Technologies Used
- **Backend**: C#/.NET 8 (ASP.NET Core Web API), ADO.NET for MySQL.
- **Frontend**: JavaScript/React 19, CSS (custom styles), Vite for build/dev.
- **Database**: MySQL 8.0.
- **Containerization**: Docker, Docker Compose.
- **Authentication**: JWT (symmetric key signing).
- **Testing**: xUnit (unit/integration), Selenium (E2E).
- **CI/CD**: GitHub Actions (build, test).
- **Other**: DotNetEnv for env vars, ESLint for JS linting.

## 4. Setup and Installation
### Prerequisites
- .NET SDK 8.0+
- Node.js 20+
- Docker & Docker Compose
- MySQL client (optional for manual DB access)
- Git

### Local Development Setup
1. **Clone Repository**:
   ```
   git clone <repo-url>
   cd lector-library
   ```

2. **Backend Setup**:
   - Navigate to `backend/Csp.Api`.
   - Restore packages: `dotnet restore`.
   - Update `appsettings.Development.json` with DB connection (e.g., `Server=localhost;Database=LibraryManagement;User=root;Port=3306`).
   - Run: `dotnet run` (starts on http://localhost:5250 with Swagger UI).

3. **Frontend Setup**:
   - Navigate to `frontend/csp-web`.
   - Install deps: `npm ci` or `npm install`.
   - Run: `npm run dev` (starts on http://localhost:5173).
   - CORS is configured for localhost:5173/3000.

4. **Database Setup**:
   - Use Docker Compose (preferred) or manual MySQL.
   - Manual: Create DB `LibraryManagement`, run schema from `UserService.InitializeDatabaseAsync` (tables: Users).

### Docker Setup
1. Set environment variables in `.env` (e.g., `MYSQL_ROOT_PASSWORD=root`, `MYSQL_DATABASE=LibraryManagement`, `DB_PORT=3306`, `API_PORT=8080`, `FRONTEND_PORT=80`).
2. Run: `docker-compose up -d` from `infra/`.
3. Access:
   - Backend: http://localhost:<API_PORT>/swagger
   - Frontend: http://localhost:<FRONTEND_PORT>
   - DB: mysql://root:<MYSQL_ROOT_PASSWORD>@localhost:<DB_PORT>/<MYSQL_DATABASE>

### Configuration
- **Env Vars** (via DotNetEnv or appsettings.json):
  - `ConnectionStrings__DefaultConnection`: DB string.
  - `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience`: For token signing/validation.
- **appsettings.json**: Prod config with placeholders (e.g., `${DB_HOST}`).
- **CORS**: Allows origins like localhost:5173.

### Running the Application
- Local: Run backend and frontend separately.
- Docker: `docker-compose up`.
- Seed Data: On startup, `Program.cs` calls `InitializeDatabaseAsync` to create tables and seed users (e.g., admin, member).

## 5. Database Schema
- **Table: Users**
  - Id (INT, PK, Auto-Increment)
  - Username (VARCHAR(50), Unique)
  - Email (VARCHAR(100), Unique)
  - PasswordHash (VARCHAR(255))
  - Role (VARCHAR(50)): 'Administrator', 'Librarian', 'Member'
  - IsActive (BOOL, Default: true)
  - CreatedAt (DATETIME)
  - UpdatedAt (DATETIME)
  - CreatedBy (INT, FK to Users.Id)
  - UpdatedBy (INT, FK to Users.Id)

- **Initialization**: Handled in `UserService.InitializeDatabaseAsync` (creates table if not exists, seeds default users like 'admin' with hashed password).

- **Queries**: Use parameterized ADO.NET (e.g., prevent SQL injection).

## 6. API Documentation
Swagger is enabled at `/swagger`. Key endpoints:

### Auth
- `POST /api/auth/login`: Login (body: {Username, Password}) → JWT token.
- `POST /api/auth/register`: (body: {Username, Email, Password}).

### Users (Requires Auth)
- `POST /api/users/members`: Create member (Admin only).
- `GET /api/users/members`: List members (paginated, searchable).
- `PUT /api/users/members/{id}`: Update member.
- `PUT /api/users/{id}/status?isActive={bool}`: Toggle status.
- `GET /api/users/my-profile`: Get own profile.
- `PUT /api/users/my-profile`: Update own profile.
- `PUT /api/users/my-password`: Change password.

- **Validation**: Emails via regex, passwords (min 8 chars, hashed with PBKDF2).
- **Error Handling**: Returns 400/401/403 with messages.

## 7. Frontend Components
- **Structure**: Vite + React, ESLint for linting.
- **Key Components**:
  - `App.jsx`: Main router, auth handling.
  - `Header.jsx`, `Sidebar.jsx`: Navigation.
  - `Login.jsx`: Form with validation.
  - `Dashboard.jsx`: Role-based sections.
  - `MemberManagement.jsx`: CRUD for members (forms, tables, modals).
  - `MyProfile.jsx`: Profile edit and password change.
  - `LendingReservation.jsx`: Tabs for loans/reservations (mock data; integrate API).
- **Routing**: Implicit via section changes in `Dashboard`.
- **State Management**: Local state (useState/useEffect); no Redux.
- **Styles**: Custom CSS with variables (e.g., `--color-primary`).
- **Build**: `npm run build` → Static files in `dist/`.

## 8. Testing
### Backend
- **Unit Tests**: xUnit for services (e.g., `UserServiceHashTests` for password hashing).
- **Integration Tests**: WebApplicationFactory for API endpoints (e.g., `LoginIntegrationTests`, mocks `IUserService`).
- **Run**: `dotnet test`.

### Frontend
- No explicit tests; ESLint for linting (`npm run lint`).

### E2E
- Selenium with ChromeDriver (headless).
- Tests: Login, member CRUD, profile updates (e.g., `Story1LoginTests`).
- Run: `dotnet test backend/Csp.E2E.Tests`.

## 9. Deployment and CI/CD
- **CI**: GitHub Actions (`dotnet.yml` for backend build/test, `node.yml` for frontend).
- **Deployment**:
  - Build Docker images: Use `infra/*.Dockerfile`.
  - Prod: Replace dev configs; use secrets for JWT/DB.
  - Hosting: NGINX for frontend, ASP.NET for backend; scale with orchestrators.
- **Monitoring**: Add logging (appsettings.json has levels); health checks in Docker Compose.

## 10. Security Considerations
- **Auth**: JWT with validation parameters (issuer, audience, lifetime).
- **Passwords**: Hashed (PBKDF2 with salt/iterations).
- **RBAC**: Policies like "RequireAdmin".
- **Input Validation**: Regex for emails, length for passwords.
- **CORS**: Restricted to specific origins.
- **Best Practices**: No plain-text passwords; use HTTPS in prod.

## 11. Troubleshooting
- **DB Connection Failed**: Check env vars, Docker health check.
- **JWT Invalid**: Verify secret/issuer in config.
- **CORS Issues**: Ensure origins match in `Program.cs`.
- **Tests Fail**: Mock data in fakes (e.g., `FakeUserService`).
- **Build Errors**: Restore packages; check Node/.NET versions.