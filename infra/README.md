# Infra Docker Compose

This folder contains the Docker Compose configuration to run the full stack locally:
- MySQL database
- Backend API (Csp.Api)
- Frontend (csp-web)

Prerequisites
- Docker Desktop (Windows) or Docker Engine + Compose

How to run
1. Open a terminal in this folder (`d:\case_study_project\lector-library\infra`).
2. Ensure the `.env` file is present in this folder. An example is provided in the repository at `infra/.env`.
3. Run:

```powershell
# Bring up services in foreground
docker compose up --build

# Or run in detached mode
docker compose up --build -d
```

Notes
- The compose file reads variables from the `infra/.env` file automatically (docker-compose loads a `.env` file from the compose folder).
- Backend API will be available at `http://localhost:${API_PORT}` (as defined in `infra/.env`).
- Frontend will be served on port `${FRONTEND_PORT}`.
- Database data is persisted to a Docker volume named `dbdata`.

Troubleshooting
- If the backend fails to start due to port conflicts, change `API_PORT` in `infra/.env`.
- If database initialization fails, check `MYSQL_ROOT_PASSWORD` and other DB env vars in `infra/.env`.

Security
- Do NOT commit real secrets to source control.
- The `.env` in this repo should be used for local development only.
