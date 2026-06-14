# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

Tesseract (repo: `epsilon-homeserver`) is a [Matrix](https://spec.matrix.org/) homeserver implementation built on .NET 10 / ASP.NET Core. `matrix-docs.md` at the repo root is a local copy of the Matrix Client-Server API spec — consult it when implementing or matching behavior of `/_matrix/...` and `/.well-known/matrix/...` endpoints.

## Commands

```bash
# Restore / build / format
dotnet restore Tesseract.slnx
dotnet build Tesseract.slnx --configuration Release
dotnet format Tesseract.slnx --verify-no-changes   # CI and pre-commit run this; use without the flag to auto-fix

# Run all tests
dotnet test Tesseract.slnx

# Run a single test (by fully qualified name or filter expression)
dotnet test --filter "FullyQualifiedName~GetDomainDiscoveryTests.Handle_RepositoryReturnsInfo_ReturnsResponseWithSameData"
dotnet test tests/Tesseract.Application.Tests --filter "FullyQualifiedName~GetDomainDiscoveryTests"

# Run the web app locally (needs Postgres reachable via ConnectionStrings:Default)
dotnet run --project src/Tesseract.Web

# Full local stack (app + Postgres + Redis) via Docker
docker compose up
```

One-time tooling setup (installs husky git hooks via dotnet tool): `./configure-tooling.sh`.

### Git hooks (husky)

- `pre-commit`: runs `gitleaks protect --staged` and `dotnet format --verify-no-changes`. Code must be formatted before committing.
- `pre-push`: runs `dotnet build --no-restore`.

CI (`.github/workflows/pr-validation.yml`) additionally runs `gitleaks git .`, `dotnet build`, and `dotnet test`, and posts a results summary as a PR comment.

## Architecture

Clean Architecture with four projects under `src/`, each mirrored by a test project under `tests/` (`Tesseract.<Layer>.Tests`):

- **Tesseract.Domain** — plain domain types (entities, value objects). No framework dependencies. e.g. `Discovery/Values/DiscoveryInfo.cs`.
- **Tesseract.Application** — use cases via MediatR, plus abstractions (interfaces) for infrastructure the use cases depend on. References Domain only.
- **Tesseract.Infrastructure** — implements Application's abstractions: Postgres access via Dapper (`IDbConnectionFactory` / `NpgsqlConnectionFactory`), DbUp-based migrations (SQL files embedded as resources from `Common/Database/Migrations/*.sql`, run on startup via `app.RunMigrations()`), and options binding (e.g. `MatrixOptions` from the `Matrix` config section).
- **Tesseract.Web** — ASP.NET Core controllers, request/response contracts, error handling, and observability (Serilog + OpenTelemetry).

### Feature organization (vertical slices)

Code is grouped by feature area, not by technical layer, mirrored across all four projects with matching folder paths. The Matrix client-server discovery feature lives at:

- `src/Tesseract.Application/ClientServer/Discovery/` — MediatR query/handler pairs as static classes containing nested `Query`/`Response`/`Handler` types (e.g. `GetDomainDiscovery`, `GetSupportedVersions`), plus `Abstractions/` for repository interfaces (`IWellKnownRepository`, `IVersionRepository`).
- `src/Tesseract.Infrastructure/ClientServer/Discovery/` — repository implementations (`WellKnownRepository`, `VersionRepository`).
- `src/Tesseract.Web/ClientServer/Discovery/` — `WellKnownController`, `VersionController`, and `Contracts/` for HTTP response DTOs.

When adding a new Matrix endpoint, follow this same `<Area>/<Feature>` folder structure across the relevant projects, with a static class per use case holding `Query`/`Command`, `Response`, and `Handler`.

### Dependency injection

Each project (except Domain) exposes an `Add<Layer>(this IHostApplicationBuilder builder)` extension method (using C# extension members, e.g. `extension(IHostApplicationBuilder builder) { ... }`) in a `DependencyInjection.cs` at its root, registering its own services (MediatR handlers, repositories, options, etc.). `Program.cs` wires these up in order: `AddApplication()`, `AddInfrastructure()`, `AddWeb()`.

### Error handling

All unhandled exceptions are caught by `GlobalExceptionHandler` (`src/Tesseract.Web/Common/Errors/GlobalExceptionHandler.cs`) and translated to Matrix-spec error responses via `IMatrixExceptionMapper` / `MatrixExceptionMapper`, which maps .NET exception types to `(HttpStatusCode, MatrixErrorResponse)` pairs using the `MatrixErrorCodes` (`errcode` values from the spec). New domain/application exceptions that should produce specific Matrix error responses should be added as new cases in `MatrixExceptionMapper.Map`.

### Configuration

- Connection strings: `ConnectionStrings:Default` (Postgres), `ConnectionStrings:Redis`.
- Matrix federation/discovery config lives under the `Matrix` section, bound to `MatrixOptions` (`Matrix:Homeserver:BaseUrl`, `Matrix:IdentityServer:BaseUrl`).
- `docker-compose.yml` runs the app alongside `postgres:17-alpine` and `redis:8-alpine`, parameterized via env vars (`POSTGRES_*`, `REDIS_*`, `APP_PORT`, `ASPNETCORE_ENVIRONMENT`).

## Testing conventions

- xUnit + FluentAssertions + NSubstitute.
- Application-layer handler tests instantiate the `Handler` directly with `Substitute.For<I...Repository>()` mocks (see `GetDomainDiscoveryTests`), covering: success path, not-found/null cases, cancellation token propagation, and exception passthrough.
- Test method naming: `MethodUnderTest_Scenario_ExpectedOutcome`.
