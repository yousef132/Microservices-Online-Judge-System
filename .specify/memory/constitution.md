<!--
Sync Impact Report
==================
Version change: [TEMPLATE] → 1.0.0
Added sections:
  - I. Global System Architecture
  - II. Service Architecture (Clean vs Vertical Slice)
  - III. Database Ownership
  - IV. Tech Stack (Non-Negotiable)
  - V. API Conventions
  - VI. Security & Authentication
  - VII. Async Messaging & Reliability
  - VIII. Observability
  - IX. Deployment & Containerisation
  - X. Frontend Architecture
  - XI. Code Style & Patterns
  - Governance
Modified principles: N/A (initial generation from codebase)
Removed sections: All placeholder tokens replaced
Templates requiring updates:
  ✅ plan-template.md — Constitution Check section now maps to named principles (I–XI)
  ✅ spec-template.md — No structural conflicts detected
  ✅ tasks-template.md — No structural conflicts detected
  ✅ commands/*.md — No agent-specific names (CLAUDE-only) detected
Deferred TODOs:
  - RATIFICATION_DATE: set to first-commit date estimate 2026-05-22 (project bootstrapped today)
-->

# Microservices Online Judge System — Constitution

## I. Global System Architecture

This project is a cloud-native **microservices-based Online Judge platform** ("JudgeSync") composed of the following autonomous services, all deployed via Docker Compose:

| Service | Technology | Responsibility |
|---|---|---|
| `users.api` | ASP.NET 8 + Carter + PostgreSQL | Identity, Auth (JWT issuance, refresh) |
| `corejudge.api` | ASP.NET 8 + Clean Architecture + PostgreSQL | Problems, Submissions, Contests, Code Execution |
| `community.api` | ASP.NET 8 + Vertical Slice + MongoDB | Articles, Comments, Votes, Bookmarks, Recommendations |
| `collaboration.api` | Node.js + Socket.IO + Yjs | Real-time collaborative code editing |
| `api.gateway` | ASP.NET 8 + YARP | Reverse proxy — single entry point for all client traffic |
| `web` | React 19 + Vite 8 | Frontend SPA |

**Infrastructure services**: PostgreSQL 16 (per-service DB), MongoDB 8, Redis 8, RabbitMQ 4.2, Elasticsearch 9, Kibana 9, Jaeger.

### Non-Negotiable Rules

- Every new service MUST be registered in `compose.yaml` and `compose-override.yaml`.
- Every service MUST expose a `/health` endpoint that returns HTTP 200.
- Services MUST NOT share databases. Each service owns its own data store exclusively.
- All external traffic MUST pass through `api.gateway` (YARP Reverse Proxy). Direct service-to-service HTTP calls from clients are forbidden.
- Cross-service communication MUST use async messaging via RabbitMQ + MassTransit. Synchronous HTTP calls between backend services are only permitted for read-only queries where latency guarantees are acceptable and explicitly justified.

---

## II. Service Architecture Patterns

Two architectural patterns coexist and MUST NOT be mixed within the same service.

### A. Clean Architecture (CoreJudge Service)

The `CoreJudge` service MUST maintain strict 4-layer separation:

```
CoreJudge.Domain          → Entities, Domain Events, Primitives (no external deps)
CoreJudge.Application     → Commands, Queries, Handlers, DTOs, Abstractions (references Domain only)
CoreJudge.Infrastructure  → EF Core, Redis, RabbitMQ, Elasticsearch implementations (references Application + Domain)
CoreJudge.API             → Controllers, Extensions, Swagger (references Application + Infrastructure)
```

Rules:
- Domain MUST NOT reference any infrastructure or application packages.
- Commands and Queries MUST implement `ICommand<TResponse>` / `IQuery<TResponse>` from `BuildingBlocks.Core.CQRS`.
- All commands/queries MUST be dispatched via MediatR — no direct service calls from controllers.
- Controllers MUST extend `BaseController` and return `Response` objects via `ResponseResult()`.
- FluentValidation validators MUST accompany every command/query that accepts user input.
- The `ValidationBehavior<,>` pipeline behavior MUST remain registered for all assemblies using MediatR.
- AutoMapper profiles MUST be defined in `CoreJudge.Application/Mapping` and registered in `InfrastructureDependencies`.

### B. Vertical Slice Architecture (Community Service)

The `Community` service MUST organize code by feature slice, not by layer:

```
Features/
└── <FeatureName>/
    └── <ActionName>/
        ├── <Action>Command.cs        (or Query)
        ├── <Action>Handler.cs        (MediatR IRequestHandler)
        ├── <Action>Endpoint.cs       (IEndpoint → MapEndpoint)
        ├── <Action>Validator.cs      (FluentValidation)
        └── (DTOs inline or in same folder)
```

Rules:
- Each feature slice MUST be self-contained. Cross-slice shared logic goes into `Common/`.
- Endpoints MUST implement `IEndpoint` and be auto-registered via `app.MapEndpoints()`.
- The `ValidationBehavior<,>` pipeline behavior MUST remain registered.
- MongoDB entities MUST use `[BsonId]` with `Guid` IDs represented as strings (`BsonType.String`).

### C. Users Service (Flat Carter Modules)

- Endpoints MUST be implemented as `ICarterModule` and explicitly registered in `Program.cs`.
- All business logic MUST be delegated to `IUserService` implementations — no business logic in modules.

### D. Collaboration Service (Node.js)

- MUST remain a Node.js / Express / Socket.IO service.
- Yjs (`y-socket.io`) MUST be used for CRDT-based real-time document sync.
- Health endpoint MUST respond at `GET /collaboration/health`.

---

## III. Database Ownership

Each service owns its database exclusively. Cross-service data access is FORBIDDEN.

| Service | Database | Engine |
|---|---|---|
| `users.api` | `usersdb` | PostgreSQL 16 (`users.db`) |
| `corejudge.api` | `corejudgedb` | PostgreSQL 16 (`corejudge.db`) + Redis (caching) + Elasticsearch (search) |
| `community.api` | `CommunityDb` | MongoDB 8 (`community.mongo`) |
| `collaboration.api` | `.yjs-storage` | LevelDB (local file, ephemeral) |

Rules:
- A service MUST NEVER connect to another service's database container.
- EF Core migrations MUST be applied at startup via `ApplyMigrationsWithRetryAsync()` (retry logic required for Docker Compose startup race conditions).
- MongoDB collections MUST be initialised via `MongoDbInitializer.InitializeAsync()` at startup.
- EF Core model configurations MUST use `IEntityTypeConfiguration<T>` classes placed in `Infrastructure/Configurations/`.
- The MassTransit Outbox pattern (`AddEntityFrameworkOutbox`) MUST remain enabled in CoreJudge to guarantee at-least-once delivery.

---

## IV. Tech Stack (Non-Negotiable)

### Backend (.NET Services)

| Concern | Package / Version |
|---|---|
| Framework | ASP.NET Core 8 (`net8.0`) |
| ORM | Entity Framework Core 8 + Npgsql 8 (PostgreSQL) |
| Messaging | MassTransit 8.2 + MassTransit.RabbitMQ 8.2 |
| Outbox Pattern | MassTransit.EntityFrameworkCore 8.2 |
| Mediator | MediatR (via BuildingBlocks) |
| Validation | FluentValidation (via BuildingBlocks) |
| Mapping | AutoMapper (CoreJudge only) |
| Caching | StackExchange.Redis 2.x |
| Search | Elastic.Clients.Elasticsearch 9.x |
| Auth | Microsoft.AspNetCore.Authentication.JwtBearer 8 |
| Identity | Microsoft.AspNetCore.Identity.EntityFrameworkCore 8 (Users) |
| Minimal API routing | Carter 8.0 (Users, CoreJudge infra) / IEndpoint pattern (Community) |
| Logging | Serilog |
| Tracing | OpenTelemetry + Jaeger (OTLP exporter) |
| API Documentation | Swashbuckle.AspNetCore 6.6 |
| Docker execution | Docker.DotNet 3.x (code sandbox via Docker socket) |

### Collaboration Service

| Concern | Package |
|---|---|
| Runtime | Node.js |
| HTTP | Express |
| WebSocket | Socket.IO 4.x |
| CRDT | Yjs + y-socket.io |

### Frontend

| Concern | Package / Version |
|---|---|
| Framework | React 19 |
| Build tool | Vite 8 |
| Routing | react-router-dom 7 |
| Icons | lucide-react |
| Code editor | @monaco-editor/react + monaco-editor |
| Real-time collab | yjs + y-monaco + y-socket.io + socket.io-client 4 |
| Linting | ESLint 9 |

**No new backend frameworks** may be introduced (e.g., NestJS, FastAPI, Django) without a constitutional amendment.
**No CSS frameworks** (e.g., Tailwind, Bootstrap) are currently used on the frontend — Vanilla CSS is the standard.

---

## V. API Conventions

### Routing

- YARP gateway routes: `/users/{**}`, `/corejudge/{**}`, `/collaboration/{**}`, `/community/{**}` (prefix-stripped before forwarding).
- Internal service routes use `api/[controller]` (CoreJudge) or explicit paths (Community, Users).
- All routes MUST be lowercase kebab-case.

### Response Shape (CoreJudge / Users)

Controllers MUST use the shared `Response` wrapper object from `CoreJudge.Domain.Primitives`. HTTP status codes MUST be derived from `response.StatusCode` via `BaseController.ResponseResult()`.

### Response Shape (Community)

Community endpoints return typed results directly (e.g., `Article`, `PagedResult<Article>`). `Results.CreatedAtRoute`, `Results.Ok`, `Results.NoContent` MUST be used.

### Swagger / OpenAPI

Every service MUST expose Swagger in Development mode. `AddSwaggerDocumentation()` extension MUST be called on new .NET services.

### Pagination

Paginated queries MUST use the `Pagination` building block from `BuildingBlocks.Core.Pagination`. Page number starts at `1`. Default page size is `10` (CoreJudge) or `12` (Community).

---

## VI. Security & Authentication

### JWT Authentication (All .NET Services)

- All .NET services MUST call `AddIdentity(configuration)` from `BuildingBlocks.Identity`.
- JWT tokens MUST be validated for: Issuer, Audience, Lifetime, Signing Key.
- `ClockSkew` MUST be ≤ 1 minute.
- Tokens MUST use `ClaimTypes.Role` for role claims and `JwtRegisteredClaimNames.Sub` for the user identity claim.
- `RequireHttpsMetadata = false` is permitted inside Docker Compose networks only.

### Roles

Defined in `BuildingBlocks.Identity.Roles`:

| Role | Description |
|---|---|
| `SuperAdmin` | Full system access |
| `Admin` | Platform admin (create/delete problems, manage contests) |
| `ProblemSetter` | Can create/edit problems |
| `Attemper` | Can solve problems |
| `User` | Basic authenticated user |

- Role checks on controllers MUST use `[Authorize(Roles = Roles.Admin)]` referencing the `Roles` constants class — no magic strings.

### Code Execution Sandbox

- User code MUST ONLY be executed inside isolated Docker containers (via Docker.DotNet + `run_code.sh`).
- The host Docker socket (`/var/run/docker.sock`) MUST be mounted into `corejudge.api` for sandbox management.
- Execution containers MUST be cleaned up after each run.
- The `CoreJudge.API` container MUST NOT run as root in production (Dockerfile switches back to `$APP_UID` after permission setup).

### Secrets Management

- Secrets (JWT keys, DB passwords, RabbitMQ credentials, MongoDB passwords) MUST NOT be committed to the repository.
- For local development: use `appsettings.Development.json` (gitignored) or environment variables.
- Production: secrets MUST be injected via environment variables or a secrets manager.

---

## VII. Async Messaging & Reliability

### Message Bus

- RabbitMQ is the sole message broker. No other broker may be introduced.
- MassTransit MUST be used as the abstraction layer (no direct RabbitMQ client calls).
- Queue naming MUST use kebab-case (`SetKebabCaseEndpointNameFormatter()`).

### Outbox Pattern

- The **EF Core Outbox** (MassTransit `AddEntityFrameworkOutbox`) MUST remain enabled in `CoreJudge` to guarantee durability.
- `UseBusOutbox()` MUST be enabled. In-memory outbox MUST NOT be used in production.
- `QueryDelay` MUST be configured (currently 10 s).

### Retry & DLQ

- Global retry policy: **Exponential backoff**, 3 retries, 2–10 s interval.
- Every consumer endpoint MUST bind a Dead Letter Queue (DLQ) via `BindDeadLetterQueue(...)`.
- Inbox deduplication (`UseEntityFrameworkOutbox` on receive endpoints) MUST remain enabled.

### Consumers

- Consumers MUST implement `IConsumer<TMessage>` from MassTransit.
- Each consumer MUST be explicitly registered with `x.AddConsumer<TConsumer>()`.
- Domain events MUST be defined in `<Service>.Domain/Events/` as plain C# records/classes.

---

## VIII. Observability

### Tracing (OpenTelemetry)

All .NET services MUST call `AddLoggingConfigs(configuration)` from `BuildingBlocks.Logging`, which configures:
- ASP.NET Core instrumentation (exceptions recorded)
- HTTP client instrumentation
- PostgreSQL (Npgsql) instrumentation
- Redis instrumentation
- gRPC client instrumentation
- OTLP export to Jaeger (`OTEL_EXPORTER_OTLP_ENDPOINT`)
- `AlwaysOnSampler` (100% sampling)

### Logging

- Serilog MUST be used as the logging provider on all .NET services.
- Minimum log level: `Information` in production, `Debug` in development.
- Log context enrichment (`Enrich.FromLogContext()`) MUST be enabled.

### Health Checks

- Every service MUST respond to `GET /health` → HTTP 200.
- Docker Compose `healthcheck` MUST be defined for every service.

---

## IX. Deployment & Containerisation

### Docker

- Every service MUST have a `Dockerfile` using multi-stage builds:
  - Stage `base`: runtime image (`mcr.microsoft.com/dotnet/aspnet:8.0`)
  - Stage `build`: SDK image for restore + build
  - Stage `publish`: produces trimmed release output
  - Stage `final`: copies published output only
- All .NET containers MUST expose port `8080` internally.
- The `collaboration.api` container uses `node:*` base image.
- The `web` container serves via Vite dev server on port `5173`.

### Docker Compose

- `compose.yaml`: base service declarations (no ports, no env vars).
- `compose-override.yaml`: environment-specific overrides (ports, env vars, dependencies).
- Services MUST declare `depends_on` with `condition: service_healthy` for all hard dependencies.
- Named volumes MUST be declared in the `volumes:` section of `compose.yaml`.
- Secrets/passwords in override files are for development only. Production deployments MUST use secrets injection.

### Service Ports (Development)

| Service | External Port |
|---|---|
| `api.gateway` | 5000 |
| `users.api` | 5001 |
| `corejudge.api` | 5002 |
| `collaboration.api` | 5003 |
| `community.api` | 5004 |
| `web` | 5173 |
| Jaeger UI | 16686 |
| RabbitMQ Management | 15672 |
| Kibana | 5601 |
| Elasticsearch | 9200 |
| Redis | 6379 |
| MongoDB | 27017 |

---

## X. Frontend Architecture

The `web` service is a **React 19 SPA** built with **Vite 8**.

### Structure

```
src/Web/src/
├── App.jsx           (primary routing shell + all page components)
├── App.css           (all component styles — vanilla CSS)
├── index.css         (global resets + CSS variables)
├── main.jsx          (React root mount)
├── components/       (shared reusable components)
└── assets/           (static assets)
```

### Rules

- **React 19** with hooks only — no class components.
- **react-router-dom v7** MUST be used for all client-side routing.
- **Vanilla CSS only** — no Tailwind, no CSS-in-JS, no Bootstrap.
- **lucide-react** is the sole icon library.
- The Monaco editor (`@monaco-editor/react`) MUST be used for all code editing surfaces.
- Real-time collaboration MUST use Yjs (`y-monaco`, `y-socket.io`) over the `collaboration.api` WebSocket path `/collaboration/socket.io`.
- JWT tokens MUST be stored in `localStorage` (key: `communityToken`) — consistent with current implementation.
- All API calls MUST go through the Vite dev proxy or the `api.gateway` in production — no direct service URLs in production builds.
- `VITE_COMMUNITY_API_URL` environment variable MUST be the sole configuration point for the community API base URL.

### Vite Proxy (Development)

- `/community-api` → `https://localhost:7014` (Community service)
- `/collaboration` → `http://collaboration.api:8080` (WebSocket-capable)

---

## XI. Code Style & Patterns

### C# (.NET)

- Target framework: **`net8.0`** across all services. No mixing of frameworks.
- `Nullable` MUST be enabled (`<Nullable>enable</Nullable>`).
- `ImplicitUsings` MUST be enabled (`<ImplicitUsings>enable</ImplicitUsings>`).
- Dependency injection MUST use extension methods (`AddApplication`, `AddInfrastructure`, `AddIdentity`, `AddLoggingConfigs`) — no configuration logic in `Program.cs`.
- `Program.cs` MUST remain minimal: register services → build app → configure pipeline → run.
- Exception handling MUST use `GlobalExceptionHandler` (registered via `AddExceptionHandler<GlobalExceptionHandler>()`) — no try/catch in controllers.
- Namespace MUST match folder structure.
- `record` types MUST be used for DTOs, commands, queries, and domain events where immutability is appropriate.
- `async`/`await` MUST be used for all I/O operations — no blocking calls.

### JavaScript / Node.js (Collaboration)

- CommonJS (`require`) is used — do not convert to ESM without explicit decision.
- Keep the service minimal: Express + Socket.IO + Yjs only.

### JavaScript / React (Frontend)

- ES Modules (`import`/`export`) only — `"type": "module"` in `package.json`.
- Functional components with hooks. No Redux or external state managers unless a constitutional amendment is made.
- `useCallback` and `useMemo` MUST be used for referentially stable callbacks and derived data in performance-sensitive components.
- `useApi()` custom hook pattern MUST be used for HTTP abstraction.
- Tags MUST be normalised to lowercase (`toLocaleLowerCase()`).
- Dates MUST be formatted with `Intl.DateTimeFormat` — no raw date string formatting.

---

## Governance

- This constitution supersedes all informal coding conventions, PR comments, and README guidelines.
- Any amendment requires: (a) documented rationale, (b) update to this file with incremented version, (c) migration plan if breaking existing code.
- Version increments follow semantic versioning:
  - **MAJOR**: removal or redefinition of a principle; breaking architectural change.
  - **MINOR**: new principle added or materially expanded guidance.
  - **PATCH**: wording clarification, typo fix, non-semantic refinement.
- All feature plans MUST reference the "Constitution Check" section and identify which principles apply.
- Complexity violations (e.g., adding a 5th service, introducing a new database engine) MUST be explicitly justified in the plan's "Complexity Tracking" table.
- Compliance MUST be reviewed at every PR by checking against the principles above.

**Version**: 1.0.0 | **Ratified**: 2026-05-22 | **Last Amended**: 2026-05-22
