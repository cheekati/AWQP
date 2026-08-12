# AWQP Semiconductor Quartz Manufacturing ERP/MES

Enterprise-grade ASP.NET Core 8 Clean Architecture solution for high-purity quartz manufacturing across semiconductor, compound semiconductor, optical fiber, and solar cell industries.

## Included Deliverables

- ASP.NET Core 8 Web API with JWT authentication, RBAC, Swagger/OpenAPI, Serilog, middleware, DI
- ASP.NET MVC/Razor/Bootstrap/jQuery dashboard UI
- Clean Architecture projects: Domain, Application, Infrastructure, API, Web
- EF Core DbContext, entity configurations, repository pattern, unit of work
- Manufacturing ERP/MES domain model covering customers, products, sales, production planning, MES, QMS, inventory, warehouse, clean room, packaging, shipping, traceability, and documents
- SQL Server scripts: schemas, tables, PK/FK/indexes/constraints, seed data, views, stored procedures, triggers
- Barcode and QR code services
- Mermaid architecture, UML, ER, workflow, and traceability diagrams
- Docker support and Azure DevOps CI/CD pipeline

## Projects

| Project | Responsibility |
| --- | --- |
| `AWQP.Domain` | Entities, enums, auditable base classes |
| `AWQP.Application` | DTOs, validators, service contracts, AutoMapper profiles |
| `AWQP.Infrastructure` | EF Core, SQL Server persistence, repositories, unit of work, JWT/token/services |
| `AWQP.Api` | REST APIs, authentication, authorization policies, Swagger, exception middleware |
| `AWQP.Web` | MVC/Razor dashboard screens with Bootstrap 5, jQuery, AJAX |

## Quick Start

```bash
docker compose up --build
```

Then open:

- API/Swagger: `http://localhost:7001/swagger`
- MVC Dashboard: `http://localhost:7002`

Apply database scripts in this order:

1. `database/001_schema.sql`
2. `database/004_module_tables.sql`
3. `database/002_seed_data.sql`
4. `database/003_views_procedures_triggers.sql`
5. `database/005_ehs_ppe.sql` (PPE module)
6. `database/006_ehs_work_permit.sql` (Work Permit supplier progress + atmospheric ranges)
7. `database/007_ehs_checkpoint_checksheet.sql` (Check Point Master + CheckSheet Items Sorting)

### Check Point Master / Items Sorting

Blazor routes:

- Check Point Master: `/checksheet` (`CheckSheet.razor`) — create/edit/delete check points and configure Minimum / Maximum Specs
- Items Sorting: `/ckpage` (`CkPage.razor`) — Department → Section → CheckSheet Name → Frequency → Date grid with Before Remarks, Specs Min/Max (from master), Actual Specs, up to 4 images, Priority, Score 0–10, Action 3/1/0

MVC redirects: `/CheckSheet` and `/CheckSheet/ItemsSorting`

Front page route: `/progressworkorderSupplier`

Displays all daily atmospheric testing readings for in-progress work permits, with acceptable ranges shown for each parameter:

| Parameter | Acceptable range |
| --- | --- |
| Oxygen Content (O₂) | 19.5 – 23.5% |
| Toxic Gas H₂S | ≤ 10 ppm |
| Carbon Monoxide (CO) | ≤ 25 ppm |
| Combustible Gas | ≤ 10% LEL |

Seed supplier code: `SUP001`

## Documentation

- [Architecture](docs/architecture.md)
- [Diagrams](docs/diagrams.md)
- [Deployment Guide](docs/deployment.md)

## Manufacturing Workflow

Raw Material Preparation -> Quartz Processing -> Forming -> Firing Process -> Inspection -> Clean Room Entry -> Final Cleaning -> Vacuum Packaging -> Shipping Inspection -> Dispatch

All post-firing operations are modeled for Class 1000 clean room control, including temperature, humidity, particle count, access logs, compliance tracking, and audit trail support.
