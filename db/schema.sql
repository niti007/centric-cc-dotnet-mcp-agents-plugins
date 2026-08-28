-- Contoso Claims schema
-- MySQL 8.4. Built exactly to SCHEMA-CONTRACT.md. Idempotent: safe to run twice.

CREATE DATABASE IF NOT EXISTS contoso_claims;
USE contoso_claims;

-- Drop in FK-safe order (children before parents)
DROP TABLE IF EXISTS payments;
DROP TABLE IF EXISTS claim_notes;
DROP TABLE IF EXISTS claims;
DROP TABLE IF EXISTS adjusters;
DROP TABLE IF EXISTS policies;

CREATE TABLE policies (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    policy_number   VARCHAR(20) NOT NULL UNIQUE,
    holder_name     VARCHAR(120) NOT NULL,
    holder_email    VARCHAR(160) NOT NULL,
    product_type    ENUM('auto','home','travel','liability') NOT NULL,
    coverage_limit  DECIMAL(12,2) NOT NULL,
    deductible      DECIMAL(10,2) NOT NULL,
    effective_date  DATE NOT NULL,
    expiry_date     DATE NOT NULL,
    status          ENUM('active','lapsed','cancelled') NOT NULL DEFAULT 'active'
) ENGINE=InnoDB;

CREATE TABLE adjusters (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    employee_code   VARCHAR(12) NOT NULL UNIQUE,
    full_name       VARCHAR(120) NOT NULL,
    email           VARCHAR(160) NOT NULL,
    region          ENUM('north','south','east','west') NOT NULL,
    is_active       TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB;

CREATE TABLE claims (
    id                      INT AUTO_INCREMENT PRIMARY KEY,
    claim_number            VARCHAR(20) NOT NULL UNIQUE,
    policy_id               INT NOT NULL,
    assigned_adjuster_id    INT NULL,
    status                  ENUM('submitted','under_review','approved','rejected','paid') NOT NULL DEFAULT 'submitted',
    incident_date           DATE NOT NULL,
    reported_at             DATETIME NOT NULL,
    description             TEXT NOT NULL,
    claimed_amount          DECIMAL(12,2) NOT NULL,
    approved_amount         DECIMAL(12,2) NULL,
    decided_by_adjuster_id  INT NULL,
    decided_at              DATETIME NULL,
    CONSTRAINT fk_claims_policy FOREIGN KEY (policy_id) REFERENCES policies(id),
    CONSTRAINT fk_claims_assigned_adjuster FOREIGN KEY (assigned_adjuster_id) REFERENCES adjusters(id),
    CONSTRAINT fk_claims_decided_by_adjuster FOREIGN KEY (decided_by_adjuster_id) REFERENCES adjusters(id)
) ENGINE=InnoDB;

CREATE INDEX idx_claims_status ON claims(status);
CREATE INDEX idx_claims_policy ON claims(policy_id);
CREATE INDEX idx_claims_assigned ON claims(assigned_adjuster_id);

CREATE TABLE claim_notes (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    claim_id            INT NOT NULL,
    author_adjuster_id  INT NULL,
    body                TEXT NOT NULL,
    created_at          DATETIME NOT NULL,
    CONSTRAINT fk_notes_claim FOREIGN KEY (claim_id) REFERENCES claims(id) ON DELETE CASCADE,
    CONSTRAINT fk_notes_author FOREIGN KEY (author_adjuster_id) REFERENCES adjusters(id)
) ENGINE=InnoDB;

CREATE TABLE payments (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    claim_id    INT NOT NULL,
    amount      DECIMAL(12,2) NOT NULL,
    paid_at     DATETIME NOT NULL,
    method      ENUM('bank_transfer','cheque','card') NOT NULL,
    reference   VARCHAR(40) NOT NULL UNIQUE,
    CONSTRAINT fk_payments_claim FOREIGN KEY (claim_id) REFERENCES claims(id)
) ENGINE=InnoDB;
