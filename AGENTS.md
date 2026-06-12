# AWQP Semiconductor Quartz ERP/MES

ASP.NET Core 8 Clean Architecture solution. See `README.md` for the product overview and `docs/` for architecture/diagrams.

## Cursor Cloud specific instructions

The VM snapshot already has the .NET 8 SDK (`~/.dotnet`, symlinked to `/usr/local/bin/dotnet`) and Docker installed; the startup update script runs `dotnet restore AWQP.sln`. Standard build/run commands live in `README.md`; the notes below are the non-obvious caveats.

### Services
- **SQL Server 2022** (required) — backing database for everything, run as a Docker container.
- **`AWQP.Api`** (required) — REST API + Swagger; entry point `src/AWQP.Api/Program.cs`.
- **`AWQP.Web`** (required for the UI) — MVC/Razor dashboard that calls the API via `ApiBaseUrl`.

No EF migrations or auto-seeding exist; the schema comes from the `database/*.sql` scripts.

### Build / lint / test
- Build (also serves as the compile/lint gate): `dotnet build AWQP.sln -c Debug`. Warnings are expected (missing XML-doc `CS1591`, package advisories); there should be **0 errors**.
- There are **no automated test projects** in this solution, so `dotnet test` has nothing to run.

### Running locally (dev mode)
1. Start SQL Server (idempotent):
   ```bash
   sudo dockerd >/tmp/dockerd.log 2>&1 &   # if the daemon is not already running
   sudo docker start awqp-sqlserver 2>/dev/null || \
   sudo docker run -d --name awqp-sqlserver -e ACCEPT_EULA=Y \
     -e MSSQL_SA_PASSWORD='Change_this_password_123!' -p 1433:1433 \
     -v awqp-sqlserver-data:/var/opt/mssql mcr.microsoft.com/mssql/server:2022-latest
   ```
2. Create the DB and apply scripts **in this order**: `001_schema`, `004_module_tables`, `002_seed_data`, `003_views_procedures_triggers`. You **must** pass `sqlcmd -I` (QUOTED_IDENTIFIER ON) or `001_schema.sql` fails with `Msg 1934` on filtered indexes:
   ```bash
   for f in 001_schema 004_module_tables 002_seed_data 003_views_procedures_triggers; do
     sudo docker exec -i awqp-sqlserver /opt/mssql-tools18/bin/sqlcmd \
       -S localhost -U sa -P 'Change_this_password_123!' -C -I -d AWQP_Manufacturing < database/$f.sql
   done
   ```
3. Run the API (the `appsettings.json` connection string has a placeholder password, so override it):
   ```bash
   export ASPNETCORE_ENVIRONMENT=Development
   export ASPNETCORE_URLS=http://0.0.0.0:5080
   export ConnectionStrings__ManufacturingDb="Server=localhost,1433;Database=AWQP_Manufacturing;User Id=sa;Password=Change_this_password_123!;TrustServerCertificate=True"
   dotnet run --project src/AWQP.Api
   ```
   Swagger: `http://localhost:5080/swagger`, health: `GET /health`.
4. Run the Web UI: `ASPNETCORE_URLS=http://0.0.0.0:5081 ApiBaseUrl=http://localhost:5080 dotnet run --project src/AWQP.Web`.

### Gotchas
- **`main` is empty.** The actual solution lives on feature branches. The `...df21` variant does **not** compile (a `JwtTokenService.cs` `IList`→`IReadOnlyList` type error and central-package floating versions); the `...generate-...-9e6d` variant builds cleanly and is the one set up here.
- **`/api/auth/login` is broken** (known app bug, not an environment issue). On login the app adds a `RefreshToken` to an already-tracked user and calls `SaveChanges`; because `BaseEntity` pre-assigns `Id = Guid.NewGuid()`, EF treats the reachable entity as `Modified` and emits `UPDATE` instead of `INSERT`, throwing `DbUpdateConcurrencyException` ("expected to affect 1 row(s), but actually affected 0"). Explicit `Repository.AddAsync` create paths (e.g. `POST /api/customers`) are unaffected.
- The seed data contains **no users**, so there is no working login account. To exercise authenticated endpoints, either insert an `auth.ApplicationUsers` row with a BCrypt hash (passwords must be ≥ 12 chars per the validator) or mint a JWT directly with the configured signing key (`Jwt:SigningKey`), issuer `AWQP`, audience `AWQP.Clients`, and a `ClaimTypes.Role` claim (e.g. `Admin`).
- When starting services via `tmux send-keys`, run `set +H` first — connection-string/password values contain `!`, which bash history expansion otherwise mangles.
