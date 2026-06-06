# Deployment Guide

## Local Docker Compose

1. Set a strong SQL Server SA password in environment variables or `.env`.
2. Run `docker compose up --build`.
3. Apply SQL scripts from `database/` or run EF Core migrations after installing the .NET SDK.
4. Browse API Swagger at `https://localhost:7001/swagger` and MVC at `https://localhost:7002`.

## Production Guidance

- Store JWT signing keys and SQL credentials in Azure Key Vault.
- Use Azure SQL Database or SQL Server Always On for production HA.
- Send Serilog output to centralized observability such as Application Insights, Seq, or ELK.
- Place API behind Azure Application Gateway or API Management.
- Configure clean room sensor integrations through message queues for resilience.
- Treat MES events as integration events so production, quality, inventory, and shipping modules can be separated into microservices later.

## Database Deployment Order

1. `database/001_schema.sql`
2. `database/004_module_tables.sql`
3. `database/002_seed_data.sql`
4. `database/003_views_procedures_triggers.sql`

## CI/CD Phases

- Restore and build .NET solution.
- Run tests when added.
- Build Docker images.
- Publish artifacts.
- Deploy API, Web, and SQL scripts through Azure DevOps environments with approvals.
