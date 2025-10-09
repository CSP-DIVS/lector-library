# Infrastructure as Code

## infra/docker-compose.yml
Defines all containers/services required for local development in a single declarative YAML file.

### Services
- **db**: MySQL 8 container for persistent database storage.
- **backend**: Builds and runs the ASP.NET Core API (see infra/backend.Dockerfile).
- **frontend**: Builds and serves the React/Vite frontend (see infra/frontend.Dockerfile).

---

## infra/backend.Dockerfile
Builds and runs the ASP.NET Core backend API.
- **Build Stage (FROM dotnet/sdk:8.0):**
  - Copies source code, restores dependencies, and executes "dotnet publish" to create a release-ready app.
- **Run Stage (FROM dotnet/aspnet:8.0):**
  - Copies published output, sets up port 8080, and launches the API.
- **Context:** Used as the build config for the `backend` service in docker-compose.yml, ensuring environment parity and deployment consistency.

---

## infra/frontend.Dockerfile
Builds and serves the static frontend application via Nginx.
- **Build Stage (FROM node:20):**
  - Copies all frontend code, installs dependencies (`npm ci` preferred), and runs the full production build.
- **Serve Stage (FROM nginx:alpine):**
  - Serves static files in /usr/share/nginx/html on port 80.
- **Context:** Used as the build config for the `frontend` service in docker-compose.yml for local and production images, guaranteeing an identical and optimized SPA deployment.

---

### Usage Instructions
- Set up a `.env` file to provide database and port variables.
- Run `docker-compose up` from the `infra/` directory to build and start all services.
