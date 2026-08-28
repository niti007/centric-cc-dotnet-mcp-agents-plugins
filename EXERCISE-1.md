# Exercise 1 — MCP + Agents (30 minutes)

**Teams of 3–4. One machine driving, everyone reading.** Swap the driver at the halfway
mark.

---

## The situation

Contoso's internal audit team has flagged something. Their message:

> We've been told that on some approved claims, the adjuster who made the approval
> decision wasn't the adjuster the claim was assigned to. We need to know **which claims**,
> **who approved them**, and **how the system let it happen**. We have until the end of
> the day.

You have two things they don't: a live database you can query through an MCP server, and
a set of reviewer agents that can read the codebase.

The database tells you **what happened**. The agents tell you **why**. You need both.

---

## Part A — Finish the MCP server (12 min)

`mcp/ContosoClaims.Mcp/` is a working C# MCP server. Two of its tools are implemented;
two are exercise stubs that currently return a "NOT IMPLEMENTED" message when called.

1. Confirm the server is registered and reachable. `.mcp.json` already registers it as
   `claims`. Build first — the registration runs it with `--no-build`:

   ```bash
   dotnet build
   ```

   Then in Claude Code, `/mcp` should list `claims`. Ask it:

   > Use the claims MCP server to list the open claims for adjuster 3.

2. Implement **`find_claims_by_adjuster`** in `mcp/ContosoClaims.Mcp/ClaimsTools.cs`.
   The comment directly above the method gives the expected SQL and return shape. It
   takes an adjuster's full name or employee code and returns the claims that adjuster
   *decided*, alongside who each claim was *assigned* to.

   Use a parameterised `MySqlCommand`, following the two working tools in the same file.
   Rebuild, restart the MCP connection, and call it.

3. If you have time, implement `claim_stats_by_status` too. It's the easier of the two —
   do it second, not first.

**Done when:** asking Claude to find the claims decided by an adjuster returns real rows,
and mismatches are visible in the output.

---

## Part B — Answer the auditor (8 min)

Using the `claims-db-analyst` agent — which reaches the database *only* through the MCP
tools — answer the audit team's first two questions:

> Which approved claims were decided by an adjuster other than the one they were assigned
> to? Who decided them?

There are **7** such claims. One adjuster accounts for four of them.

Note what happens if you ask the analyst something its tools can't answer — it should
tell you which tool is missing rather than guess. That behaviour is written into the
agent's prompt, and it's the difference between an analyst and a fabricator.

---

## Part C — Find the cause in the code (10 min)

The data proves it happened. Now find the code path that allowed it.

Run **two agents in parallel**:

```
Run the claims-security-reviewer and efcore-perf-reviewer agents in parallel against
src/ContosoClaims.Api/Controllers/ClaimsController.cs and
src/ContosoClaims.Api/Legacy/PayoutReportBuilder.cs. Give me each agent's findings,
then a consolidated summary.
```

Do **not** tell the agents what to look for. Finding it is their job — if they miss it,
that's information about the agent's description, not about the code.

Then write `reports/audit.md` (create `reports/` if needed) containing:

1. The 7 claims, with claim numbers, assigned adjuster, and deciding adjuster.
2. **The exact controller action** that permits this, and why it differs from its
   siblings in the same file. Name the method and the line.
3. One paragraph on the fix you'd make.

Write it in your own words. "There's a security bug in the claims controller" is not an
audit finding — someone who hasn't read the code must be able to locate the exact method
from your description alone.

---

## Definition of done

- [ ] `find_claims_by_adjuster` returns real data from the live database
- [ ] `reports/audit.md` names all 7 claims
- [ ] `reports/audit.md` names the exact controller action and explains how it differs
      from the sibling actions around it
- [ ] You can say which agent found what

## Readout (5 min, one person per team)

1. Which tool did you implement, and what tripped you up?
2. Did the security agent find the cause on its own? If not, what did you change?
3. **One thing about the agents' output you didn't trust.** Every team should have one.
