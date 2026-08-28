---
description: Build a structured QA status report — build, tests, and coverage gaps — into reports/qa-report.md.
argument-hint: [optional focus area]
---

Produce a QA status report for the Contoso Claims API. Focus: **$ARGUMENTS** (if empty,
report on the whole solution).

Gather real evidence — run the commands, do not estimate:

1. `dotnet build` — record warnings and errors verbatim.
2. `dotnet test` — record the pass/fail/skip counts and the duration.
3. Read `tests/` and compare against `src/`. Identify **what is not covered**, ranked by
   risk rather than by line count. A route that mutates a claim's decision fields with no
   test is a higher-risk gap than an untested DTO mapper, even though the mapper may have
   more uncovered lines.

Write the report to `reports/qa-report.md` (create `reports/` if needed) with this shape:

```
# QA Report — <ISO date>
## Build
## Tests
## Coverage gaps, by risk
| Area | What is untested | Risk | Why |
## Recommended next tests
```

If the build or tests fail, that is the headline of the report — put it first and quote
the actual output. Never report a suite as green without having run it in this session.
