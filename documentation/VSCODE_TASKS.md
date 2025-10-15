# VS Code Tasks Documentation

This document describes the available VS Code tasks for the Lector Library project, which provide convenient ways to run, build, and test both frontend and backend components.

## Table of Contents

- [Quick Start](#quick-start)
- [Available Tasks](#available-tasks)
- [How to Run Tasks](#how-to-run-tasks)
- [Task Categories](#task-categories)
- [Troubleshooting](#troubleshooting)
- [Advanced Usage](#advanced-usage)

---

## Quick Start

### 🚀 **Start Full Development Environment**

The fastest way to get started:

1. **Press `Ctrl+Shift+B`** (or `Cmd+Shift+B` on Mac)
2. This runs the **"Start Full Stack (Frontend + Backend)"** task
3. Both servers will start simultaneously in separate terminals

**What happens:**
- Backend API starts on `http://localhost:5192` (or configured port)
- Frontend dev server starts on `http://localhost:5173` 
- Environment variables are loaded from `.env` file
- Hot reload is enabled for both frontend and backend

---

## Available Tasks

### 🌟 **Primary Development Tasks**

#### **Start Full Stack (Frontend + Backend)** ⭐ *Default Build Task*
- **What it does:** Starts both frontend and backend development servers simultaneously
- **Use case:** Primary development workflow
- **Keyboard shortcut:** `Ctrl+Shift+B`
- **Terminals:** Creates separate terminals for frontend and backend
- **Background:** Yes (continues running while you code)

#### **Backend: Start Development Server**
- **What it does:** Starts the ASP.NET Core API server
- **Command:** `dotnet run` in `backend/Csp.Api/`
- **Features:**
  - Loads environment variables from `.env` file
  - Hot reload enabled (watches for file changes)
  - Expands environment variable placeholders in connection strings
- **URL:** `http://localhost:5192` (or `API_PORT` from `.env`)
- **Background:** Yes

#### **Frontend: Start Development Server**
- **What it does:** Starts the React + Vite development server
- **Command:** `npm run dev` in `frontend/csp-web/`
- **Features:**
  - Hot Module Replacement (HMR)
  - Fast refresh for React components
  - Proxies API calls to backend
- **URL:** `http://localhost:5173`
- **Background:** Yes

---

### 🏗️ **Build Tasks**

#### **Backend: Build**
- **What it does:** Compiles the .NET project without running it
- **Command:** `dotnet build` in `backend/Csp.Api/`
- **Use case:** Check for compilation errors, prepare for deployment
- **Output:** Build artifacts in `bin/` directory

#### **Frontend: Build**
- **What it does:** Creates production build of React app
- **Command:** `npm run build` in `frontend/csp-web/`
- **Use case:** Prepare for deployment, check bundle size
- **Output:** Static files in `dist/` directory

---

### 🧪 **Test Tasks**

#### **Backend: Run Tests**
- **What it does:** Runs all .NET test projects in the solution
- **Command:** `dotnet test --verbosity normal`
- **Includes:**
  - Unit tests (`Csp.Unit.Tests`)
  - Integration tests (`Csp.Api.Tests`) 
  - E2E tests (`Csp.E2E.Tests`)
- **Output:** Test results with pass/fail status and coverage

#### **Frontend: Run Tests**
- **What it does:** Runs Vitest test suite for React components
- **Command:** `npm run test` in `frontend/csp-web/`
- **Includes:**
  - Component tests
  - API integration tests
  - Utility function tests
- **Features:** Watch mode, coverage reporting

#### **Run All Tests**
- **What it does:** Runs both frontend and backend tests in parallel
- **Use case:** Full test suite validation before commits/deployments
- **Efficient:** Tests run simultaneously to save time

#### **test:unit** *(Legacy)*
- **What it does:** Runs only unit tests from `Csp.Unit.Tests`
- **Command:** `dotnet test backend/Csp.Unit.Tests/Csp.Unit.Tests.csproj`
- **Use case:** Quick unit test validation

---

## How to Run Tasks

### Method 1: Command Palette (Recommended)
1. **Open Command Palette:** `Ctrl+Shift+P` (or `Cmd+Shift+P`)
2. **Type:** `Tasks: Run Task`
3. **Select:** Choose your desired task from the list
4. **Result:** Task runs in integrated terminal

### Method 2: Keyboard Shortcuts
- **`Ctrl+Shift+B`** - Runs the default build task (Full Stack)
- **`Ctrl+Shift+P` → `Tasks: Run Build Task`** - Same as above

### Method 3: Terminal Menu
1. **Terminal Menu:** `Terminal` → `Run Task...`
2. **Select:** Choose from available tasks

### Method 4: Tasks Panel
1. **Open Tasks Panel:** `View` → `Command Palette` → `Tasks: Show Running Tasks`
2. **Manage:** View, restart, or terminate running tasks

---

## Task Categories

### 🏃‍♂️ **Development Workflow**

```
Start Development:
├── Start Full Stack (Frontend + Backend)  ← Recommended
├── Backend: Start Development Server
└── Frontend: Start Development Server

Build for Production:
├── Backend: Build
└── Frontend: Build

Quality Assurance:
├── Run All Tests
├── Backend: Run Tests
├── Frontend: Run Tests
└── test:unit
```

### 🎯 **Typical Workflows**

#### **Daily Development**
1. `Ctrl+Shift+B` - Start full stack
2. Code your features
3. `Tasks: Run Task` → `Run All Tests` - Validate changes

#### **Backend Focus**
1. `Backend: Start Development Server`
2. `Backend: Run Tests` - After changes

#### **Frontend Focus**
1. `Frontend: Start Development Server`
2. `Frontend: Run Tests` - After changes

#### **Pre-Deployment**
1. `Backend: Build` + `Frontend: Build` - Check compilation
2. `Run All Tests` - Full validation

---

## Task Features

### 🎨 **Terminal Management**
- **Dedicated Panels:** Each task gets its own terminal tab
- **Background Tasks:** Servers run without blocking VS Code
- **Instance Limits:** Prevents running multiple copies of the same server
- **Auto-Focus:** Reveals terminal output when tasks start

### 🔍 **Problem Detection**
- **Error Highlighting:** Compilation errors appear in Problems panel
- **Smart Patterns:** Recognizes .NET and TypeScript error formats
- **Background Monitoring:** Detects when servers are ready
- **Build Integration:** Links errors to source code locations

### 🔄 **State Management**
- **Process Tracking:** VS Code tracks running background tasks
- **Clean Termination:** `Ctrl+C` properly stops servers
- **Restart Capability:** Can restart tasks without closing terminals
- **Resource Cleanup:** Prevents port conflicts and resource leaks

---

## Environment Configuration

### Backend Environment Variables
The backend tasks automatically load environment variables from `.env` file:

```bash
# Database Configuration
DB_HOST=localhost
DB_PORT=3306
DB_NAME=lector-library
DB_USER=root
DB_PASSWORD=your-password

# JWT Configuration
JWT_SECRET_KEY=your-jwt-secret

# Environment
ASPNETCORE_ENVIRONMENT=Development
```

### Frontend Environment Variables
The frontend uses Vite's environment variable system:
- `.env` - Default environment variables
- `.env.local` - Local overrides (ignored by git)
- `.env.development` - Development-specific variables

---

## Troubleshooting

### 🚨 **Common Issues**

#### **Backend won't start**
```bash
# Check environment variables
cat backend/Csp.Api/.env

# Verify database connection
# Check MySQL is running on specified port

# Check port conflicts
netstat -an | grep :5192
```

#### **Frontend build errors**
```bash
# Clear node modules and reinstall
cd frontend/csp-web
rm -rf node_modules package-lock.json
npm install
```

#### **Tests failing**
```bash
# Backend: Check test database connection
# Frontend: Check test environment setup

# Run tests individually to isolate issues
dotnet test backend/Csp.Unit.Tests/
npm run test --run
```

### 🔧 **Task Not Running**
1. **Check VS Code version:** Ensure VS Code is up to date
2. **Reload window:** `Ctrl+Shift+P` → `Developer: Reload Window`
3. **Check workspace:** Ensure you're in the correct workspace folder
4. **Verify paths:** Ensure task paths match your project structure

### 🐛 **Terminal Issues**
- **Clear terminal:** Right-click terminal → `Clear`
- **Kill terminal:** Right-click terminal → `Kill Terminal`
- **New terminal:** `Ctrl+Shift+`` (backtick)

---

## Advanced Usage

### 🎛️ **Customizing Tasks**

#### **Modify task settings:**
Edit `.vscode/tasks.json` to customize:
- Command arguments
- Working directories
- Environment variables
- Terminal behavior

#### **Add new tasks:**
```json
{
    "label": "My Custom Task",
    "type": "shell",
    "command": "your-command",
    "options": {
        "cwd": "${workspaceFolder}/your-path"
    }
}
```

### 🔗 **Task Dependencies**
Tasks can depend on other tasks:
```json
{
    "label": "Deploy",
    "dependsOn": ["Backend: Build", "Frontend: Build"],
    "dependsOrder": "sequence"
}
```

### 🎯 **Problem Matchers**
Configure how VS Code parses error output:
- `$msCompile` - .NET compiler errors
- `$tsc` - TypeScript errors
- Custom patterns for other tools

### 🎨 **Presentation Options**
Control how task output is displayed:
- `reveal`: When to show terminal (`always`, `silent`, `never`)
- `panel`: Terminal grouping (`shared`, `dedicated`, `new`)
- `clear`: Whether to clear terminal before running

---

## Integration with Development Tools

### 🔄 **Git Hooks**
Integrate tasks with git hooks:
```bash
# .git/hooks/pre-commit
#!/bin/bash
# Run tests before commit
code --wait --command "workbench.action.tasks.runTask" "Run All Tests"
```

### 🐳 **Docker Integration**
Tasks can be extended to work with Docker:
```json
{
    "label": "Start with Docker",
    "command": "docker-compose",
    "args": ["up", "-d"]
}
```

### 🚀 **CI/CD Integration**
Use same commands in CI/CD pipelines:
```yaml
# GitHub Actions example
- name: Build Backend
  run: dotnet build backend/Csp.Api/

- name: Build Frontend  
  run: |
    cd frontend/csp-web
    npm run build
```

---

## Task Reference Quick Guide

| Task | Shortcut | Purpose | Background |
|------|----------|---------|------------|
| Start Full Stack | `Ctrl+Shift+B` | Development environment | ✅ |
| Backend Server | Manual | API development | ✅ |
| Frontend Server | Manual | UI development | ✅ |
| Backend Build | Manual | Compilation check | ❌ |
| Frontend Build | Manual | Production build | ❌ |
| All Tests | Manual | Full validation | ❌ |
| Backend Tests | Manual | API testing | ❌ |
| Frontend Tests | Manual | UI testing | ❌ |

---

*This documentation is part of the Lector Library Management System. For more information, see the main project README.*