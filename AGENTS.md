# AWQP Semiconductor Quartz ERP/MES

ASP.NET Core 8 Clean Architecture solution (Domain / Application / Infrastructure / Api / Web).
See `README.md` for the product overview and `docs/` for architecture and diagrams.

## Cursor Cloud specific instructions

The VM snapshot already has the .NET 8 SDK (installed at `~/.dotnet`, symlinked to `/usr/local/bin/dotnet`) and Docker (engine 29, configured for `fuse-overlayfs`). The startup update script runs `dotnet restore AWQP.sln`. Standard build/run commands live in `README.md`; the notes below are the non-obvious caveats.

### Services
- **SQL Server 2022** (required) — backing database for everything; runs as a Docker container. There are **no EF migrations or auto-seeding**; the schema comes entirely from the `database/*.sql` scripts.
- **`AWQP.Api`** (required) — REST API + Swagger; entry point `src/AWQP.Api/Program.cs`.
- **`AWQP.Web`** (required for the UI) — MVC/Razor dashboard that calls the API via `ApiBaseUrl`.

### Build / lint / test
- Build (also serves as the compile/lint gate): `dotnet build AWQP.sln -c Debug`. Warnings are expected (missing XML-doc `CS1591`, `AutoMapper` advisory `NU1903`); there should be **0 errors**.
- There are **no automated test projects** in this solution, so `dotnet test` has nothing to run.

### Running locally (dev mode)
Run `set +H` first in any shell that will send the connection string / SA password: they contain `!`, which bash history expansion otherwise mangles (`event not found`).

1. Start the Docker daemon if it is not already running, then start SQL Server (idempotent):
   ```bash
   sudo dockerd >/tmp/dockerd.log 2>&1 &   # only if `sudo docker info` fails
   sudo docker start awqp-sqlserver 2>/dev/null || \
   sudo docker run -d --name awqp-sqlserver -e ACCEPT_EULA=Y \
     -e MSSQL_SA_PASSWORD='Change_this_password_123!' -p 1433:1433 \
     -v awqp-sqlserver-data:/var/opt/mssql mcr.microsoft.com/mssql/server:2022-latest
   ```
2. Create the database and apply the scripts **in this order**: `001_schema`, `004_module_tables`, `002_seed_data`, `003_views_procedures_triggers`, `005_ehs_ppe`. The DB must be created first, and you **must** pass `sqlcmd -I` (QUOTED_IDENTIFIER ON) or `001_schema.sql` fails with `Msg 1934` on filtered indexes:
   ```bash
   SQLCMD="sudo docker exec -i awqp-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Change_this_password_123! -C -I"
   $SQLCMD -Q "IF DB_ID('AWQP_Manufacturing') IS NULL CREATE DATABASE AWQP_Manufacturing;"
   for f in 001_schema 004_module_tables 002_seed_data 003_views_procedures_triggers 005_ehs_ppe; do
     $SQLCMD -d AWQP_Manufacturing < database/$f.sql
   done
   ```
3. Run the API (the `appsettings.json` connection string password is a placeholder, so override it):
   ```bash
   export ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://0.0.0.0:5080
   export ConnectionStrings__ManufacturingDb="Server=localhost,1433;Database=AWQP_Manufacturing;User Id=sa;Password=Change_this_password_123!;TrustServerCertificate=True"
   dotnet run --project src/AWQP.Api
   ```
   Swagger: `http://localhost:5080/swagger`, health: `GET /health`.
4. Run the Web UI. Its `appsettings.json` defaults `ApiBaseUrl` to `https://localhost:7001`, so override it to the running API:
   ```bash
   ASPNETCORE_URLS=http://0.0.0.0:5081 ApiBaseUrl=http://localhost:5080 dotnet run --project src/AWQP.Web
   ```

### Gotchas
- **`main` is empty.** The actual solution lives on feature branches; check one out before building. This branch (`ehs-ppe-module`) builds cleanly and includes the MARUWA EHS/PPE module.
- **`/api/auth/login` is broken** (known app bug, not an environment issue): it adds a `RefreshToken` to an already-tracked user and calls `SaveChanges`; because `BaseEntity` pre-assigns `Id`, EF emits `UPDATE` instead of `INSERT` and throws `DbUpdateConcurrencyException`. The seed data also contains **no users**. Explicit create paths (e.g. `POST /api/customers`) are unaffected.
- To exercise authorized API endpoints directly, mint a JWT with the configured `Jwt:SigningKey`, issuer `AWQP`, audience `AWQP.Clients`, and a role claim (`http://schemas.microsoft.com/ws/2008/06/identity/claims/role`, e.g. `Admin`).
- **The EHS/PPE UI works without login.** `AWQP.Web`'s `ApiTokenProvider` mints its own short-lived Admin service token, and the `/Ppe/*` screens call the API through a same-origin proxy (`/Ppe/api/ehs/...`). So the PPE Master/Request/Issue/Inventory pages are the easiest end-to-end UI flow (Web → proxy → API → SQL Server).
