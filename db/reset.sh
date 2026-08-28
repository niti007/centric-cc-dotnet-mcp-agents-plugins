#!/usr/bin/env bash
# Reload the Contoso Claims database from schema.sql + seed.sql.
# Safe to run any number of times — the seed is deterministic and idempotent.
set -euo pipefail

CONTAINER="${CONTOSO_MYSQL_CONTAINER:-contoso-mysql}"
PASSWORD="${CONTOSO_MYSQL_PASSWORD:-ContosoDemo!23}"
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if ! docker ps --format '{{.Names}}' | grep -qx "$CONTAINER"; then
  echo "The '$CONTAINER' container is not running."
  echo "Start it with:  docker start $CONTAINER"
  echo "(or follow START-HERE.md if you have never created it)"
  exit 1
fi

echo "Reloading schema..."
docker exec -i "$CONTAINER" mysql -uroot -p"$PASSWORD" < "$DIR/schema.sql"
echo "Reloading seed data..."
docker exec -i "$CONTAINER" mysql -uroot -p"$PASSWORD" < "$DIR/seed.sql"

docker exec -i "$CONTAINER" mysql -uroot -p"$PASSWORD" contoso_claims -e "
SELECT 'policies' AS table_name, COUNT(*) AS rows_loaded FROM policies
UNION ALL SELECT 'adjusters', COUNT(*) FROM adjusters
UNION ALL SELECT 'claims', COUNT(*) FROM claims
UNION ALL SELECT 'claim_notes', COUNT(*) FROM claim_notes
UNION ALL SELECT 'payments', COUNT(*) FROM payments;"

echo
echo "Database reset. Expected: 120 / 8 / 500 / 900 / 180."
