---
description: Run the three reviewer agents in parallel over a scope and consolidate their findings into one report.
argument-hint: [files or directory to review]
---

Review scope: **$ARGUMENTS** (if empty, review the current git diff; if there is no
diff, review `src/ContosoClaims.Api/Controllers/` and `src/ContosoClaims.Api/Services/`).

Run these three agents **in parallel**, each over the same scope:

- `claims-security-reviewer`
- `efcore-perf-reviewer`
- `dotnet-style-reviewer`

Then consolidate. The consolidated report is not a concatenation of three reports — it is
one document:

1. **Findings**, merged and ordered by severity across all three agents. Where two agents
   flagged the same line for different reasons, say so in one entry rather than listing it
   twice.
2. For each finding: `file:line`, severity, what actually goes wrong, and the fix.
3. **Disagreements or gaps** — anywhere an agent reported nothing in a category it was
   asked about, or where two agents' conclusions are in tension. Do not smooth these over;
   they are the most useful part of a multi-agent review.
4. A closing count by severity.

Do not fix anything. This command reports.
