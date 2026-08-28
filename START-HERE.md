# Start here

**Open this repository in Claude Code and say:**

> Follow START-HERE.md and set up my environment.

Claude will work through the steps below and stop at the first thing that fails. You can
also do it by hand — the commands are all here.

---

## Claude: do these in order

Work through these steps one at a time. **Stop at the first failure** and tell me plainly
what broke and how to fix it — do not continue past a broken step, and do not work around
a failure by changing the repository.

### 1. Check prerequisites

Run each and report the version:

```bash
dotnet --version     # need 8.x
docker --version     # any recent version
git --version
```

If .NET 8 is missing → https://dotnet.microsoft.com/download/dotnet/8.0
If Docker is missing → https://www.docker.com/products/docker-desktop/

Stop here if either is missing.

### 2. Start the database

The workshop database runs in Docker on **port 3307**, so it won't collide with any MySQL
already installed on the machine.

```bash
docker run -d --name contoso-mysql --restart unless-stopped \
  -e MYSQL_ROOT_PASSWORD='ContosoDemo!23' \
  -e MYSQL_DATABASE=contoso_claims \
  -p 3307:3306 mysql:8.4
```

If the container already exists, start it instead: `docker start contoso-mysql`.

Then wait until it actually accepts connections — the container reports "up" well before
MySQL is ready:

```bash
until docker exec contoso-mysql mysqladmin ping -h 127.0.0.1 -uroot -p'ContosoDemo!23' --silent 2>/dev/null; do sleep 3; done
echo "MySQL is ready"
```

### 3. Load the schema and seed data

```bash
docker exec -i contoso-mysql mysql -uroot -p'ContosoDemo!23' < db/schema.sql
docker exec -i contoso-mysql mysql -uroot -p'ContosoDemo!23' < db/seed.sql
```

Then print the row counts so we can see it worked:

```bash
docker exec -i contoso-mysql mysql -uroot -p'ContosoDemo!23' contoso_claims -e "
SELECT 'policies' AS table_name, COUNT(*) AS rows_loaded FROM policies
UNION ALL SELECT 'adjusters', COUNT(*) FROM adjusters
UNION ALL SELECT 'claims', COUNT(*) FROM claims
UNION ALL SELECT 'claim_notes', COUNT(*) FROM claim_notes
UNION ALL SELECT 'payments', COUNT(*) FROM payments;"
```

Expected: **120 policies, 8 adjusters, 500 claims, 900 claim_notes, 180 payments.**
If any count is 0, the seed didn't load — say so rather than continuing.

### 4. Build and test

```bash
dotnet build
dotnet test
```

Expected: build succeeds with 0 errors, and **all 15 tests pass**.

If tests fail with a connection error, the database isn't reachable — go back to step 2.
If they fail on assertions, stop and show me the failure; don't try to fix the tests.

### 5. Tell me I'm ready

Print a short summary:

- the connection string: `Server=127.0.0.1;Port=3307;User ID=root;Password=ContosoDemo!23;Database=contoso_claims`
- how to reset the data (re-run step 3)
- how to run the API: `dotnet run --project src/ContosoClaims.Api`

Then stop. **Do not** register MCP servers, create agents, or install plugins — this
repository already ships `.mcp.json`, `.claude/agents/`, and `plugins/claims-kit/`, and
we use them during the session.

---

## Connecting a GUI client (optional)

Any MySQL client works — MySQL Workbench, DBeaver, DataGrip, or the VS Code MySQL
extension:

| Setting | Value |
|---|---|
| Host | `127.0.0.1` |
| Port | `3307` |
| User | `root` |
| Password | `ContosoDemo!23` |
| Schema | `contoso_claims` |

## Troubleshooting

**Port 3307 already in use** — something else grabbed it. Pick another port, and change
both the `docker run -p` mapping and `ConnectionStrings:ClaimsDb` in
`src/ContosoClaims.Api/appsettings.json` to match.

**`docker: command not found`** — Docker Desktop is installed but not running. Start it
and wait for the whale icon to settle.

**Tests pass locally but the seed looks wrong** — reload it. The seed is idempotent, so
re-running step 3 is always safe.
