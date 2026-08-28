# Contoso Claims — schema contract (frozen)

Database `contoso_claims`, MySQL 8.4. Both the seed SQL and the EF Core model build
against exactly this. Do not rename columns or change types without saying so.

## policies
| column | type | notes |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| policy_number | VARCHAR(20) NOT NULL UNIQUE | e.g. `POL-2024-00042` |
| holder_name | VARCHAR(120) NOT NULL | |
| holder_email | VARCHAR(160) NOT NULL | |
| product_type | ENUM('auto','home','travel','liability') NOT NULL | |
| coverage_limit | DECIMAL(12,2) NOT NULL | |
| deductible | DECIMAL(10,2) NOT NULL | |
| effective_date | DATE NOT NULL | |
| expiry_date | DATE NOT NULL | |
| status | ENUM('active','lapsed','cancelled') NOT NULL DEFAULT 'active' | |

## adjusters
| column | type | notes |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| employee_code | VARCHAR(12) NOT NULL UNIQUE | e.g. `ADJ-004` |
| full_name | VARCHAR(120) NOT NULL | |
| email | VARCHAR(160) NOT NULL | |
| region | ENUM('north','south','east','west') NOT NULL | |
| is_active | TINYINT(1) NOT NULL DEFAULT 1 | |

## claims
| column | type | notes |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| claim_number | VARCHAR(20) NOT NULL UNIQUE | e.g. `CLM-2025-00317` |
| policy_id | INT NOT NULL | FK -> policies(id) |
| assigned_adjuster_id | INT NULL | FK -> adjusters(id). Who *should* handle it. |
| status | ENUM('submitted','under_review','approved','rejected','paid') NOT NULL DEFAULT 'submitted' | |
| incident_date | DATE NOT NULL | |
| reported_at | DATETIME NOT NULL | |
| description | TEXT NOT NULL | free text, may contain HTML |
| claimed_amount | DECIMAL(12,2) NOT NULL | |
| approved_amount | DECIMAL(12,2) NULL | set when status in (approved, paid) |
| decided_by_adjuster_id | INT NULL | FK -> adjusters(id). Who *actually* decided it. |
| decided_at | DATETIME NULL | |

Indexes: `idx_claims_status(status)`, `idx_claims_policy(policy_id)`,
`idx_claims_assigned(assigned_adjuster_id)`.

**Two different people:** `assigned_adjuster_id` is who a claim was given to;
`decided_by_adjuster_id` is who actually recorded the decision. They are separate columns
because, in a real claims operation, they are not guaranteed to be the same person —
whether they agree is a question the data can answer.

## claim_notes
| column | type | notes |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| claim_id | INT NOT NULL | FK -> claims(id) ON DELETE CASCADE |
| author_adjuster_id | INT NULL | FK -> adjusters(id) |
| body | TEXT NOT NULL | at least one row contains raw HTML |
| created_at | DATETIME NOT NULL | |

## payments
| column | type | notes |
|---|---|---|
| id | INT PK AUTO_INCREMENT | |
| claim_id | INT NOT NULL | FK -> claims(id) |
| amount | DECIMAL(12,2) NOT NULL | |
| paid_at | DATETIME NOT NULL | |
| method | ENUM('bank_transfer','cheque','card') NOT NULL | |
| reference | VARCHAR(40) NOT NULL UNIQUE | |

## Connection (workshop database)

```
Server=127.0.0.1;Port=3307;User ID=root;Password=ContosoDemo!23;Database=contoso_claims
```

Port 3307, not 3306: the trainer machine already runs a native MySQL on 3306.
