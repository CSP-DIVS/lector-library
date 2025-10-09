# DocFx Documentation Setup

This directory contains the configuration and content for generating automated API documentation using DocFx.

## Prerequisites

Install DocFx globally:

```bash
dotnet tool install -g docfx
```

Or install locally in the project:

```bash
dotnet tool install docfx
```

## Project Structure

```
lector-library/
├── docfx.json              # DocFx configuration
├── index.md                # Documentation home page
├── toc.yml                 # Table of contents
├── articles/               # Documentation articles
│   ├── index.md
│   ├── toc.yml
│   ├── overview.md
│   ├── authentication.md
│   ├── lending-system.md
│   └── reservation-system.md
├── backend/
│   └── Csp.Api/
│       └── Csp.Api.csproj  # Configured for XML doc generation
└── _site/                  # Generated documentation (git-ignored)
```

## Configuration

### XML Documentation Generation

The `Csp.Api.csproj` has been configured to generate XML documentation:

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml</DocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

### DocFx Configuration

The `docfx.json` file configures:
- **Metadata**: API reference generation from C# projects
- **Build**: Content files, templates, and output settings
- **Global Metadata**: Site title, footer, search, etc.

## Building Documentation

### Step 1: Build the .NET Project

First, build the project to generate XML documentation:

```bash
cd backend/Csp.Api
dotnet build -c Release
```

This generates `Csp.Api.xml` in the `bin/Release/net8.0/` directory.

### Step 2: Generate API Metadata

Generate API reference metadata from the XML documentation:

```bash
docfx metadata docfx.json
```

This creates YAML files in the `api/` directory.

### Step 3: Build Documentation Site

Build the complete documentation website:

```bash
docfx build docfx.json
```

This generates a static website in the `_site/` directory.

### Step 4: Serve Documentation Locally

Preview the documentation in a browser:

```bash
docfx serve _site
```

Access the documentation at `http://localhost:8080`
