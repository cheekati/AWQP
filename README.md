# Change Management System (Engineering Request)

ASP.NET Core 8 Web API + React (Vite/TypeScript) for Engineering Requests (ER): create/edit, multi-file attachments, three-level verification, and COO approval.

## Stack

- **Backend:** .NET 8, EF Core + SQLite, JWT auth, Swagger
- **Frontend:** React 19, React Router, Axios, Vite

## Quick start

### API

```bash
cd backend/ChangeManagement.Api
dotnet run --urls http://localhost:5080
```

Swagger: http://localhost:5080/swagger

### Frontend

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
backend/ChangeManagement.Api/   # API, domain, EF Core, uploads
frontend/                       # React UI
```
