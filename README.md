# JAT

Job Application Tracker with Model Context Protocol support.

## Tech Stack

- .NET 10
- ASP.NET Core Minimal API
- Entity Framework Core
- Microsoft SQL Server
- Model Context Protocol SDK
- Docker

## Features

- Track job applications, statuses and salary ranges
- Store recruiter and company contact details
- Manage application tags
- MCP tools for AI assistants
- Swagger UI API documentation
- Health checks for monitoring

## Quick Start

1. Create environment file:
```bash
cp .env.example .env
```

2. Start database and application:
```bash
docker compose up -d
```

## Endpoints

- Swagger UI: http://localhost:8080/swagger
- MCP Server: http://localhost:8080/mcp
- Health Live: http://localhost:8080/health/live
- Health Ready: http://localhost:8080/health/ready
