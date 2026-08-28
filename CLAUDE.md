# Contoso Claims API

An insurance claims-processing API used as the working codebase for the Claude Code
workshop. ASP.NET Core 8, EF Core 8, MySQL 8.4.

## Running it

```bash
dotnet build          # builds the API, the tests, and the MCP server
dotnet test           # runs against the live MySQL — the database must be seeded
dotnet run --project src/ContosoClaims.Api
```

The database must be up and seeded before tests will pass. See `START-HERE.md`.

## Layout

| Path | What |
|---|---|
| `src/ContosoClaims.Api/Controllers/` | HTTP surface — attribute-routed controllers |
| `src/ContosoClaims.Api/Services/` | Business logic; owns all `DbContext` access |
| `src/ContosoClaims.Api/Data/` | `ClaimsDbContext` |
| `src/ContosoClaims.Api/Models/` | EF entities — never returned from an action |
| `src/ContosoClaims.Api/Dtos/` | Wire shapes, in and out |
| `src/ContosoClaims.Api/Legacy/` | Older report code, kept as-is |
| `src/ContosoClaims.Api/Auth/` | `AdjusterAuthFilter` |
| `tests/ContosoClaims.Tests/` | xunit, `WebApplicationFactory` |
| `mcp/ContosoClaims.Mcp/` | C# stdio MCP server exposing claims data |
| `db/` | `schema.sql`, `seed.sql`, and the frozen schema contract |

## Conventions

- **Layering.** Controllers handle HTTP and delegate. Services hold logic and own the
  `DbContext`. Controllers never query `ClaimsDbContext` directly.
- **DTOs at the boundary.** Actions return types from `Dtos/`, never EF entities.
- **Async all the way.** `Async` suffix on async methods. No `.Result` or `.Wait()`.
- **Money is `decimal`.** Never `double` or `float` for a monetary value.
- **SQL.** LINQ, or a parameterised command. Never interpolate caller-supplied values
  into a SQL string.
- **Nullable reference types are on.** Don't paper over a real null with `!`.

## Auth

Deliberately simple, so the workshop can focus on other things: an `X-Adjuster-Id`
header, resolved by `AdjusterAuthFilter`. No JWT, no ASP.NET Identity — this is a known
design choice, not an oversight.

**Authentication is not authorization.** The filter proves *an* adjuster is calling. Any
endpoint that reads or writes a *specific* claim must additionally check that the caller
is the adjuster that claim is assigned to, and return 403 if not.

## Database

MySQL 8.4 on **port 3307** locally (the trainer machine runs another MySQL on 3306).
Connection string lives in `appsettings.json` under `ConnectionStrings:ClaimsDb`, and is
overridable via the `ConnectionStrings__ClaimsDb` environment variable — that is how CI
points at its own MySQL service on 3306.

Reset the data at any time:

```bash
mysql -h 127.0.0.1 -P 3307 -u root -p'ContosoDemo!23' < db/schema.sql
mysql -h 127.0.0.1 -P 3307 -u root -p'ContosoDemo!23' < db/seed.sql
```

The seed is deterministic and idempotent — reloading always produces the same rows.

The credentials in this repo are for a throwaway local container. They are not secrets,
and nothing here should ever be pointed at a real database.
