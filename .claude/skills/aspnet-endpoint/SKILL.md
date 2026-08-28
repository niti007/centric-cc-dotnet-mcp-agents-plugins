---
name: aspnet-endpoint
description: Add a new endpoint to the Contoso Claims API following this repo's controller/service/DTO layering, auth filter, and test conventions. Use when adding or modifying an API route in this codebase.
---

# Adding an endpoint to Contoso Claims

Follow the existing layering. Read one sibling action before writing a new one —
`ClaimsController.GetById` is the reference shape.

## The layers

| Layer | Responsibility | Where |
|---|---|---|
| Controller | HTTP concerns, status codes, delegating | `src/ContosoClaims.Api/Controllers/` |
| Service | Logic and all `DbContext` access | `src/ContosoClaims.Api/Services/` |
| DTO | The wire shape, in and out | `src/ContosoClaims.Api/Dtos/` |
| Entity | The database shape — never returned from an action | `src/ContosoClaims.Api/Models/` |

## Checklist

1. **Route** — attribute-routed on the existing controller where it fits; a new
   controller only for a genuinely new resource. Use typed constraints (`{id:int}`).
2. **Auth** — controllers sit behind `AdjusterAuthFilter`, so a valid `X-Adjuster-Id`
   is guaranteed. That is authentication only. **If the endpoint reads or writes a
   specific claim, check the calling adjuster is the one assigned to it and return 403
   if not.** Match what the sibling actions do.
3. **Input validation** — validate in the controller, return `BadRequest` with a message
   naming the field. Do not let an invalid value reach the service.
4. **Service method** — `async`, `Async` suffix, returns entities or primitives. All
   EF Core access lives here. If you need related rows, load them in one query; never
   query inside a loop.
5. **DTO** — add to `Dtos/`, map in the service or a static mapper. Never return an
   entity: it leaks the schema and drags navigation properties into serialization.
6. **Money** — `decimal`, always. Never `double` or `float`.
7. **SQL** — LINQ, or a parameterised command. Never interpolate a caller-supplied value
   into a SQL string.
8. **Test** — add to `tests/ContosoClaims.Tests/`, using `CustomWebApplicationFactory`.
   Cover the happy path, the 404, and the 403 if the endpoint is claim-scoped. Look up
   rows rather than hardcoding ids — the seed gets reloaded.

## Verify

`dotnet build` then `dotnet test`. Both green before you call it done.
