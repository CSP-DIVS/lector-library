# CI/CD Pipeline Configurations

## `.github/workflows/azure-deploy.yml`
**Purpose:** Deploy both frontend and backend to Azure App Service, integrating build and deployment into one workflow.
**Trigger:** Runs on push to `main` or `deploy/usermanagement` branches and can be triggered manually.
**Key Steps:**
1. Checkout repository code.
2. Setup .NET 8 and Node.js 20 environments.
3. Install and build frontend (React + Vite, in `frontend/csp-web`).
4. Copy built frontend assets to backend static files directory `backend/Csp.Api/wwwroot`.
5. Restore, build, and publish backend (ASP.NET Core project in `backend/Csp.Api`).
6. Verify publish directory for correctness.
7. Deploy the built package to Azure App Service using the webapps-deploy GitHub Action.
**Secrets/Env Vars:**
- `AZURE_WEBAPP_PUBLISH_PROFILE` (required, granted from Azure App Service)
- `AZURE_WEBAPP_NAME`, connection strings, and other settings configured via Azure or pipeline env.
**Related Resources:** Azure App Service, Azure Portal for secrets, build logs on GitHub Actions.

---

## `.github/workflows/dotnet.yml`
**Purpose:** Automated build and test for the backend API to ensure code quality on every commit and pull request.
**Trigger:** Runs on all pushes and pull requests to any branch.
**Key Steps:**
1. Checkout code.
2. Setup .NET 8 environment.
3. Restore backend dependencies (`backend/Csp.Api`).
4. Build backend in Release mode.
5. Run tests (`dotnet test`).
**Notes:** Test failures are currently ignored (`|| true`) as a placeholder; update for strict test enforcement before production.

---

## `.github/workflows/node.yml`
**Purpose:** Automated build process for the frontend app on every commit and pull request.
**Trigger:** Runs on all pushes and pull requests to any branch.
**Key Steps:**
1. Checkout code.
2. Setup Node.js 20 environment.
3. Install frontend dependencies (`npm ci`, but falls back to `npm install` if lockfile missing or broken).
4. Build the frontend application (`npm run build`).
**Notes:** Assures that build errors are caught early and can be tracked per commit.
