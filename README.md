# AWQP Semiconductor Quartz ERP/MES

Enterprise-grade ASP.NET Core 8 Clean Architecture reference implementation for a high-purity semiconductor quartz manufacturer.

## Included deliverables

- ASP.NET Core 8 Web API with JWT, Swagger/OpenAPI, Serilog, FluentValidation, AutoMapper, repository/unit-of-work, EF Core, and SQL Server.
- ASP.NET MVC/Razor UI with Bootstrap 5, jQuery, and AJAX dashboard screens.
- ERP/MES modules for customers, products, sales, production planning, MES operations, QMS, inventory, warehouse, vacuum packaging, shipping, traceability, document management, clean-room monitoring, audit, and dashboards.
- SQL Server scripts for schema, constraints, indexes, seed data, views, procedures, and triggers.
- Mermaid architecture/UML/ER/workflow diagrams.
- Docker Compose and Azure DevOps CI/CD pipeline.

## Structure

```text
src/
  AWQP.Domain/          Enterprise domain entities and enums
  AWQP.Application/     DTOs, validators, interfaces, use-case services
  AWQP.Infrastructure/  EF Core, Identity, JWT, repositories, seed data
  AWQP.Api/             REST API, middleware, Swagger, authorization policies
  AWQP.Web/             MVC/Razor Bootstrap screens
database/               SQL Server schema, seed, views, procedures, triggers
docs/                   Architecture, LLD, API catalog, deployment guide, diagrams
```

## Quick start

```bash
docker compose up --build
```

Then open:

- API Swagger: `http://localhost:8080/swagger`
- MVC UI: `http://localhost:8081`

The API image can seed reference data when `SeedDatabase=true`.

> Replace all demo passwords and JWT signing keys before production use.
