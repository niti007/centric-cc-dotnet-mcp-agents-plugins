# Exercise 2 — Build a plugin (30 minutes)

**Same teams. Swap the driver from Exercise 1.**

---

## The situation

The review setup your team has been using — the reviewer agents, the `/claim-review`
command, the endpoint skill — works well on this repository. Three other teams at Contoso
want it, and none of them work on the claims codebase.

Copying `.claude/` between repositories by hand doesn't scale and immediately drifts.
Package it instead.

---

## Part A — Populate the scaffold (12 min)

`plugins/claims-kit/` is a plugin scaffold: a manifest, plus three empty directories, each
with a placeholder README explaining what belongs there.

1. Copy the two commands from `.claude/commands/` into `plugins/claims-kit/commands/`.
2. Copy the `aspnet-endpoint` skill directory from `.claude/skills/` into
   `plugins/claims-kit/skills/`.
3. Copy the three **reviewer** agents into `plugins/claims-kit/agents/` —
   `claims-security-reviewer`, `efcore-perf-reviewer`, `dotnet-style-reviewer`.

   Think before you copy the other two. `claims-db-analyst` depends on the `claims` MCP
   server existing; `xunit-test-writer` can edit files and run `dotnet test`. Decide
   deliberately whether each belongs in a plugin other teams will install, and be ready
   to defend the call in your readout.
4. Delete the placeholder READMEs from the directories you filled.
5. Bump `version` in `plugins/claims-kit/plugin.json` from `0.1.0` to `1.0.0`.

**Done when:** all three directories hold real files, and `plugin.json` is valid JSON with
the new version.

---

## Part B — Install it somewhere else and find out what breaks (15 min)

This is the part that matters. A plugin that only works in the repository it was authored
in isn't a plugin.

1. Make a clean directory **outside this repository** — genuinely elsewhere, not a
   subfolder:

   ```bash
   mkdir ~/plugin-test && cd ~/plugin-test && git init
   ```

   Put a small C# file or two in it, so there's something to review.

2. Install `claims-kit` from its path in your checkout, and confirm Claude Code sees it:
   `/plugin` should list it, and the commands and agents should appear.

3. Now actually use it there. Run `/claim-review` on your throwaway code, and run one of
   the reviewer agents.

4. **Write down what breaks.** Some of it will. Look specifically at:
   - Do the agents reference files that only exist in the claims repo (`CLAUDE.md`,
     `db/SCHEMA-CONTRACT.md`, `Dtos/`)? What does the agent do when they're missing?
   - Does `/qa-report` assume `dotnet test` and a `tests/` directory?
   - Does any agent give confident advice about a codebase it can't actually see?

Record your findings in `NOTES.md` in your throwaway directory: what you installed, what
you ran, and what happened.

**"It didn't work standalone" is a passing answer** — provided you can say precisely
*what* assumed the claims repo, and how you'd fix it. That finding is the whole point of
this exercise. A team that reports "everything worked fine" either got lucky or didn't
look hard enough.

---

## Part C — If you have time

Fix one portability problem you found. The usual culprit is an agent prompt that names
this repo's file layout as though every repo has it. Rewrite it so the agent degrades
gracefully — reads the conventions file *if there is one*, and says so plainly when there
isn't.

---

## Definition of done

- [ ] `plugins/claims-kit/` has real commands, skills, and agents; placeholders deleted
- [ ] `plugin.json` is valid JSON at version `1.0.0`
- [ ] The plugin is installed in a directory outside this repository
- [ ] A command from it has been run there
- [ ] `NOTES.md` records what broke, or names what you checked and found solid

## Readout (5 min, one person per team)

1. Which agents did you include, which did you leave out, and why?
2. What broke when you installed it elsewhere?
3. Would you actually ship this to another team on Monday? What would you fix first?
