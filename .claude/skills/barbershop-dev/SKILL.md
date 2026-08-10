---
name: barbershop-dev
description: This skill should be used when the user asks to "work on BarberShop", "add an endpoint", "add a feature to this project", "create a migration", "add a service", "fix a bug in the API/Web", or otherwise makes changes inside this repository (App.Api, App.Application, App.Domain, App.Persistence, App.Common, App.Web). Provides the project's layering rules, exception conventions, repository pattern, front-end conventions, and the migration workflow.
version: 1.0.0
---

# BarberShop development conventions

This skill packages the architecture rules and workflows this repository follows. Read `.agents/resumo-app.md` first for the current business-domain summary (entities, services, known limitations) — it is the source of truth for *what* the system does. This skill covers *how* to make changes consistently with the existing codebase.

## Project layout

Six projects in `BarberShop.sln`, referenced top-down:

- `App.Domain` — entities (`Entities/`), request/response DTOs (`DTO/`), enums (`Enums/`), and interfaces (`Interfaces/Application/I*Service.cs`, `Interfaces/Repository/IRepositoryBase.cs`). Zero dependencies, zero implementation.
- `App.Persistence` — `AppDbContext`, `RepositoryBase<TEntity>`, EF Core `Migrations/`. References `App.Domain` only.
- `App.Common` — cross-cutting utilities (`Criptografia`, `TextoHelper`). Zero dependencies.
- `App.Application` — one `Services/<Domain>Service.cs` per business domain, implementing the matching `I<Domain>Service`. All business rules live here.
- `App.Api` — thin controllers (`Controllers/`), the global exception handler (`Middlewares/AppExceptionHandler.cs`), background workers (`Workers/`).
- `App.Web` — Razor Pages that are empty HTML shells + vanilla jQuery front-end (`wwwroot/js`).

## Layering rule: controllers are thin, services own the business rules

A controller action does exactly three things: bind the request, call the service, return `Ok(...)`. Never put validation, branching on business state, or `try/catch` in a controller — the global exception handler (`App.Api/Middlewares/AppExceptionHandler.cs`) does that translation centrally.

```csharp
[HttpPost("Cadastrar")]
public IActionResult Cadastrar([FromBody] CadastrarFuncionarioRequestDTO request)
{
    _funcionariosService.Cadastrar(request);
    return Ok("Funcionário cadastrado com sucesso.");
}
```

Business exceptions use exactly two types, mapped centrally:
- `InvalidOperationException` → HTTP 400. Use for every validation failure and "not found" case.
- `UnauthorizedAccessException` → HTTP 401. Use only for authentication failures (`Logar`).

Never throw a bare `Exception` from a service — it falls through to the handler's generic 500 branch and gets logged as unexpected. If a new HTTP status is genuinely needed (rare — e.g. `LogarAdmin`'s 403 for a non-admin user), keep that one specific translation in the controller and explain why, but default to the two-exception vocabulary above.

One exception to the "no logic in controllers" rule: pure I/O/framework glue that has no business meaning, such as reading an `IFormFile` into a `byte[]`. The *validation* of that data (allowed extensions, size limits) still belongs in the service — see `UsuariosService.AtualizarFotoPerfil`.

## Repository pattern

`IRepositoryBase<TEntity>.FindById(int id)` returns `TEntity?` — it never throws. Every call site must handle the not-found case explicitly and produce a domain-appropriate message:

```csharp
var funcionario = _funcionarioRepository.FindById(id)
                  ?? throw new InvalidOperationException("Funcionário não encontrado.");
```

`Insert`/`Update`/`Remove` do not catch or wrap exceptions — a Postgres constraint violation (e.g. unique index) propagates as-is to the global exception handler, which logs it and returns a generic 500. If a duplicate is a *predictable* business scenario (not just an edge case), validate for it explicitly in the service beforehand and throw `InvalidOperationException` with a friendly message (see `UsuariosService.ValidarDadosUnicos`) rather than relying on the DB round-trip to fail.

`Query(predicate)` returns `IQueryable<TEntity>` with `AsNoTracking()` already applied — safe to chain `.Include()`, `.Where()`, `.Select()`, `.OrderBy()` before materializing with `.ToList()`/`.FirstOrDefault()`/`.Any()`.

## Adding a new domain end-to-end

Follow this order (mirrors how `Funcionarios` was added):

1. Entity in `App.Domain/Entities/`, DTOs in `App.Domain/DTO/`, interface in `App.Domain/Interfaces/Application/I<Domain>Service.cs`.
2. If the entity needs a non-default relationship (one-to-one, cascade behavior, etc.), configure it explicitly in `App.Persistence/AppDbContext.cs` `OnModelCreating` — do not rely on EF conventions alone for anything but the simplest one-to-many FK.
3. Service in `App.Application/Services/<Domain>Service.cs`, registered in `App.Application/DependencyInjectionConfig.cs` via `services.AddTransient<I<Domain>Service, <Domain>Service>();`.
4. Thin controller in `App.Api/Controllers/<Domain>Controller.cs`.
5. If the schema changed, generate and review a migration (see below).
6. Front-end: one `wwwroot/js/services/<domain>Service.js` file with one function per endpoint (`<Domain>_<Action>`, thin wrappers around `Get`/`Post`/`Delete`/`PostFile` from `core/requester.js`), registered as a `<script>` in `_Layout.cshtml` if used site-wide, or only in the specific page's `Scripts` section if page-scoped (see how `apexcharts.min.js` is only loaded in `Admin/Index.cshtml`).
7. **Update `.agents/resumo-app.md`** — see the mandatory rule below.

Reuse services from other domains via constructor injection of their interface rather than duplicating validation — e.g. `FuncionariosService` injects `IUsuariosService` to create the linked admin account instead of re-implementing user creation.

## Migrations

```
dotnet ef migrations add <DescriptiveName> --project App.Persistence --startup-project App.Api
```

Always read the generated `.cs` file before applying — confirm nullability, cascade/set-null behavior, and index uniqueness match intent (EF infers unique indexes from one-to-one `HasOne().WithOne()` config automatically). Apply with:

```
dotnet ef database update --project App.Persistence --startup-project App.Api
```

The local dev connection string is in `App.Api/appsettings.Development.json` (Postgres on `localhost:5432`). When adding a column to a table that already has rows in a live/dev database, prefer a nullable column over a required one with a backfill, unless a sane default value truly exists — see `Agendamentos.FuncionarioId`.

Before running any `dotnet build`/`dotnet ef` command, check whether an `App.Api.exe` process is already running (`dotnet run` or IDE debug session) — it locks the build output DLLs and causes `MSB3027`/`MSB3021` copy errors that look like build failures but aren't compile errors. Confirm with the user before killing such a process; it may be theirs.

## Front-end conventions

- Razor Pages (`App.Web/Pages/*.cshtml.cs`) have empty `OnGet()` — all logic lives in `wwwroot/js/pages/<page>.js`.
- No real server-side authentication: login result is stored as JSON in `sessionStorage` (`getUsuarioLogado`/`salvarSessaoUsuario`/`limparSessaoUsuario` in `core/functions.js`). The front-end trusts the `isAdmin` field client-side; do not treat this as a security boundary.
- Third-party JS libraries are vendored locally under `wwwroot/lib/<name>/`, matching `bootstrap`/`jquery` — never load from a CDN in a page shipped to users.
- Reuse existing CSS component classes for new card/list UI instead of adding new ones — e.g. `.servico-card` is generic enough for any radio-select grid, not service-specific. When two independent card groups reuse the same class, scope jQuery `.active`-toggling to each group's container so one doesn't clear the other's selection.
- `core/requester.js` (`Get`/`Post`/`PostFile`/`Delete`) already attaches the auth header and handles 401/403 redirects — use it rather than raw `$.ajax`.

## Mandatory: keep `.agents/resumo-app.md` current

Any change to an entity, service business rule, endpoint, or front-end flow must be reflected in `.agents/resumo-app.md` in the same session — update the relevant numbered section (or add a new one) and the "Limitações conhecidas" section if the change introduces or resolves one. This file is the first thing to read when picking up work in this repo; letting it drift out of date defeats its purpose.
