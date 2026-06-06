# Deployment Guide

## Prerequisites

- .NET 8 SDK
- SQL Server 2022 or Azure SQL
- Docker Desktop or compatible container runtime
- Azure DevOps project for CI/CD

## Local Docker deployment

```bash
docker compose up --build
```

Services:

- SQL Server: `localhost,1433`
- API: `http://localhost:8080`
- Web UI: `http://localhost:8081`

## Database deployment

Option 1: EF Core migrations

```bash
dotnet ef database update --project src/AWQP.Infrastructure --startup-project src/AWQP.Api
```

Option 2: SQL scripts

Run scripts in order:

1. `database/001_schema.sql`
2. `database/002_seed.sql`
3. `database/003_views.sql`
4. `database/004_stored_procedures.sql`
5. `database/005_triggers.sql`

## Production configuration

Set these as environment variables or secret-store values:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SigningKey`
- `Jwt__AccessTokenMinutes`
- `Jwt__RefreshTokenDays`

Never use the sample SQL Server password or JWT signing key outside local development.

## Azure deployment shape

Recommended production topology:

- Azure App Service or AKS for API
- Azure App Service or Static Front Door-backed App Service for MVC UI
- Azure SQL Database with zone redundancy
- Azure Key Vault for secrets
- Azure Monitor/Application Insights for telemetry
- Private Endpoint for SQL
- Managed Identity from apps to Key Vault

## Operational controls

- Enable SQL auditing and threat detection.
- Configure database backups and point-in-time restore.
- Emit Serilog logs to Application Insights, Seq, or SQL Server.
- Use deployment slots for API and Web.
- Run smoke tests against `/swagger` and dashboard endpoints after deployment.

## Migration strategy

The current implementation is a modular monolith. If one module requires independent scaling, extract it by:

1. Moving the module application services and entities to a separate service.
2. Replacing repository access with HTTP/gRPC or messaging.
3. Moving the corresponding tables to a service-owned database schema.
4. Publishing integration events for cross-module updates.
