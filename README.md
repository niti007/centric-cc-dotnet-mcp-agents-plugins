# Contoso Claims — Claude Code workshop

A working ASP.NET Core 8 + MySQL insurance-claims API, used as the codebase for a
one-day Claude Code workshop on **MCP, sub-agents, plugins, and CI**.

## Setup

**Open this repo in Claude Code and say "Follow START-HERE.md and set up my environment."**

It checks your prerequisites, starts MySQL in Docker, loads the seed data, and gets you to
a green build. Manual commands are in [START-HERE.md](START-HERE.md) if you'd rather do it
by hand.

You need: **.NET 8 SDK**, **Docker**, **git**, and **Claude Code**.

## What's in here

| Path | What |
|---|---|
| `src/ContosoClaims.Api/` | The API — controllers, services, EF Core, DTOs |
| `tests/ContosoClaims.Tests/` | xunit suite, 15 tests, green on a correct setup |
| `mcp/ContosoClaims.Mcp/` | A C# stdio **MCP server** exposing claims data |
| `db/` | Schema, seed data, and the frozen schema contract |
| `.claude/agents/` | Five predefined sub-agents |
| `.claude/commands/`, `.claude/skills/` | `/claim-review`, `/qa-report`, `aspnet-endpoint` |
| `plugins/claims-kit/` | Plugin scaffold — you populate it in Exercise 2 |
| `.github/workflows/ci.yml` | Build + test, and a headless Claude review on PRs |
| `EXERCISE-1.md`, `EXERCISE-2.md` | The two group exercises |

## The exercises

- **[Exercise 1](EXERCISE-1.md)** — MCP + agents. An audit question you can only answer by
  combining live database queries through an MCP server with agents reading the code.
- **[Exercise 2](EXERCISE-2.md)** — package the review setup as a plugin, install it into a
  different repository, and find out what breaks.

## The domain

Policies, adjusters, claims, notes, payments. An adjuster is *assigned* a claim, and an
adjuster *decides* it — those are two different columns, and whether they always agree
turns out to be an interesting question.

Auth is a deliberately simple `X-Adjuster-Id` header. That is a design choice for the
workshop, not an oversight.

## A note on this codebase

This repository contains **real, deliberately planted defects** — the kind that show up in
production code review. They are not marked in the source, because finding them is the
point. Do not copy this code into anything real.

The database credentials here belong to a throwaway local container. They are not secrets.
