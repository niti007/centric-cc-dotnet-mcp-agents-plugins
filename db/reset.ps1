# Reload the Contoso Claims database from schema.sql + seed.sql.
# Safe to run any number of times - the seed is deterministic and idempotent.
$ErrorActionPreference = "Stop"

$container = if ($env:CONTOSO_MYSQL_CONTAINER) { $env:CONTOSO_MYSQL_CONTAINER } else { "contoso-mysql" }
$password  = if ($env:CONTOSO_MYSQL_PASSWORD)  { $env:CONTOSO_MYSQL_PASSWORD }  else { "ContosoDemo!23" }
$dir       = Split-Path -Parent $MyInvocation.MyCommand.Path

$running = docker ps --format '{{.Names}}'
if ($running -notcontains $container) {
    Write-Host "The '$container' container is not running."
    Write-Host "Start it with:  docker start $container"
    Write-Host "(or follow START-HERE.md if you have never created it)"
    exit 1
}

Write-Host "Reloading schema..."
Get-Content "$dir\schema.sql" -Raw | docker exec -i $container mysql -uroot -p"$password"
Write-Host "Reloading seed data..."
Get-Content "$dir\seed.sql" -Raw | docker exec -i $container mysql -uroot -p"$password"

$query = @"
SELECT 'policies' AS table_name, COUNT(*) AS rows_loaded FROM policies
UNION ALL SELECT 'adjusters', COUNT(*) FROM adjusters
UNION ALL SELECT 'claims', COUNT(*) FROM claims
UNION ALL SELECT 'claim_notes', COUNT(*) FROM claim_notes
UNION ALL SELECT 'payments', COUNT(*) FROM payments;
"@
docker exec -i $container mysql -uroot -p"$password" contoso_claims -e $query

Write-Host ""
Write-Host "Database reset. Expected: 120 / 8 / 500 / 900 / 180."
