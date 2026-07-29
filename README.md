# Change Management System (Engineering Request)

ASP.NET Core 8 Web API + React (Vite/TypeScript) for Engineering Requests (ER): create/edit, multi-file attachments, three-level verification, and COO approval.

## Stack

| Layer | Technology | Version |
|---|---|---|
| Backend | ASP.NET Core / .NET | 8.0 |
| ORM | Entity Framework Core | 8.0.11 |
| Database | **Microsoft SQL Server** | SQL Server–compatible (local Docker: Azure SQL Edge; production: SQL Server 2019/2022) |
| Auth | JWT Bearer | 8.0.11 |
| API docs | Swashbuckle (Swagger) | 6.9.0 |
| Frontend | React | 19.x |
| Routing | React Router DOM | 7.x |
| HTTP client | Axios | 1.x |
| Build tool | Vite | 8.x |
| Language (UI) | TypeScript | 6.x |

## Quick start

### 1. Start SQL Server (Docker)

```bash
docker compose up -d
# or:
docker run -d --name cms-sqlserver \
  -e 'ACCEPT_EULA=Y' \
  -e 'MSSQL_SA_PASSWORD=Your_strong_Password123' \
  -p 1433:1433 \
  mcr.microsoft.com/azure-sql-edge:latest
```

`docker-compose.yml` uses **Azure SQL Edge** (SQL Server wire-compatible) for local development. Point the connection string at full **SQL Server 2019/2022** in production.

Default connection (see `appsettings.json`):

```
Server=localhost,1433;Database=ChangeManagement;User Id=sa;Password=Your_strong_Password123;TrustServerCertificate=True;Encrypt=False
```

### 2. API

```bash
cd backend/ChangeManagement.Api
dotnet ef database update   # applies migrations (also auto-runs on startup)
dotnet run --urls http://localhost:5080
```

Swagger: http://localhost:5080/swagger

### 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

App: http://localhost:5173 (proxies `/api` and `/uploads` to the API)

## Demo users

Password for all: `Password123!`

| Email | Role |
|---|---|
| requester@cms.local | Requester |
| safety@cms.local | Safety verifier |
| depthead@cms.local | Dept Head |
| qa@cms.local | QA |
| coo@cms.local | COO |
| admin@cms.local | Admin |

## Workflow (initial scope)

1. Requester creates an ER (division PR/ENG/QA/IND). **ER No** is auto-generated as `YYMMXXX-N` (submission 1 or 2).
2. Each submission is valid **3 months**; max **2** submissions; resubmit window **89 days**.
3. Reason for change supports multi-select (Cost Down / Alternative Sourcing / Others).
4. Chemical fields appear only when “Applicable to chemical or materials” is checked.
5. Attachments: any file type; multiple images accumulate (not replaced); image preview popup on upload.
6. Sample quantity accepts **numbers only**.
7. On submit, email notifications are logged for Safety / Dept Head / QA assignees.
8. Verifiers **approve or reject only** (no field edits). Any rejection returns the ER to the requester with a comment (`Rejected`).
9. When all three approve, the ER goes to the **COO** queue.
10. COO can **Approve**, **Reject**, or **Re-Submit** (with Pass / Fail / Re-Submit outcome).
11. Index page supports **View** / **Edit**; editing never changes the ER number.
12. Safety / Dept Head / QA / COO land on their queue by default after login.

## Project layout

```
docker-compose.yml              # SQL Server 2022
backend/ChangeManagement.Api/   # API, domain, EF Core (SQL Server), uploads
frontend/                       # React UI
```
