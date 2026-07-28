# Change Management System (ChangeOps)

Enterprise Engineering Request (ER) Change Management System built with:

- **Backend:** ASP.NET Core 8 Web API
- **Frontend:** React + TypeScript (Vite)
- **ORM:** Entity Framework Core (Code First)
- **Database:** SQL Server (production) / SQLite (local demo)
- **Auth:** JWT + Role-Based Authorization
- **Architecture:** Clean Architecture (Domain, Application, Infrastructure, API)

## Solution Structure

```
src/
  ChangeManagement.Domain/          # Entities, enums, repository contracts
  ChangeManagement.Application/     # DTOs, validators, services, AutoMapper
  ChangeManagement.Infrastructure/  # EF Core, JWT, email, file storage, DI
  ChangeManagement.API/             # REST controllers, Swagger, CORS
client/                             # React SPA
docs/database-schema.sql            # SQL Server reference schema
```

## Features

- Engineering Request form with auto ER number, validation date, 89-day expiry, max 2 submissions
- Dynamic fields for chemical/materials, test lot identification, multi-select reasons
- Multi-file attachments with image preview popup, download, and delete before final submission
- Workflow: Requester → Safety → Department Head → QA → COO
- Role dashboards and queues (Requester, Safety, Dept Head, QA, COO, Administrator)
- Approve / Reject / Resubmit with mandatory comments where required
- In-app notifications + email notification service (SMTP, disabled by default in demo)
- Dashboard statistics and charts
- Search, filter, sort, pagination

## Roles & Demo Users

Password for all seeded users: `Password@123`

| Username   | Role            |
|------------|-----------------|
| requester  | Requester       |
| safety     | Safety          |
| depthead   | DepartmentHead  |
| qa         | QA              |
| coo        | COO             |
| admin      | Administrator   |

## Prerequisites

- .NET 8 SDK
- Node.js 20+
- SQL Server (optional for production). SQLite is used by default for local runs.

## Configuration

`src/ChangeManagement.API/appsettings.json`

- `Database:Provider` = `Sqlite` (default) or `SqlServer`
- `ConnectionStrings:DefaultConnection` for active provider
- For SQL Server, set:

```json
"Database": { "Provider": "SqlServer" },
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=ChangeManagementDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Email notifications:

```json
"EmailSettings": {
  "Enabled": true,
  "SmtpHost": "smtp.example.com",
  "SmtpPort": 587,
  "FromEmail": "noreply@company.com",
  "FromName": "Change Management System",
  "UserName": "...",
  "Password": "...",
  "UseSsl": true
}
```

## Run Backend

```bash
cd src/ChangeManagement.API
dotnet restore
dotnet run --urls http://localhost:5080
```

- Swagger: http://localhost:5080/swagger
- Health: http://localhost:5080/health

EF Core Code First creates/seeds the database on startup (`EnsureCreated` + seed data).

## Run Frontend

```bash
cd client
npm install
npm run dev
```

Open http://localhost:5173

Optional: set `VITE_API_URL=http://localhost:5080/api` in `client/.env`.

## Workflow Rules

1. Requester creates/saves draft, uploads attachments, then submits.
2. Status becomes **In Progress**; Safety, Dept Head, and QA are notified.
3. Any verifier rejection → **Rejected**, returns to requester with comments (editable/resubmit, max 2 submissions).
4. All three approvals → **AwaitingCooApproval**.
5. COO Approve → **Approved**; Reject → **Rejected**; Resubmit → **Resubmitted** (comments mandatory).

## API Overview

- `POST /api/auth/login`
- `GET/POST /api/masterdata/*`
- `GET/POST/PUT /api/engineering-requests*`
- `POST/DELETE/GET /api/attachments/{erId}/...`
- `GET/POST /api/verification/*`
- `GET/POST /api/coo/*`
- `GET/POST /api/notifications*`
- `GET /api/dashboard/stats`

## Production Notes

- Switch provider to SQL Server and apply migrations or run `docs/database-schema.sql` as a reference.
- Replace JWT secret and enable HTTPS.
- Configure real SMTP credentials.
- Persist uploads to durable storage (current demo uses local `uploads/`).
- Harden CORS origins for the deployed SPA URL.
