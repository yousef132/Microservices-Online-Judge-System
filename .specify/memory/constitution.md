<!--
Sync Impact Report
==================
Version change: 1.1.0 → 1.2.0
Added sections:
  - XIII. Missing API Mocking Strategy & UI Development
      — A. Missing API Gateways & State Machine
      — B. MSW Mocking & Sandboxing
      — C. Missing API Documentation Compilation
      — D. Design System Source
Modified principles:
  - XII.H. Anti-Patterns — Tailwind row updated to reflect exception for new UI components
  - IV. Tech Stack — Tailwind CSS authorized for new UI components
Removed sections: none
Templates requiring updates:
  ✅ plan-template.md — Constitution Check section now maps to named principles (I–XIII)
  ✅ spec-template.md — No structural conflicts detected
  ✅ tasks-template.md — No structural conflicts detected
Deferred TODOs: none
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
For detailed structural guidance on adding or modifying frontend features, see **Section XII**.

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

## XII. Web App Architecture Blueprint (AI Agent Guidance)

This section is a **mandatory reference for any AI model** (or human contributor) that adds, edits, or removes features from the frontend or full-stack system. It codifies the architectural patterns, file conventions, and step-by-step recipes that MUST be followed.

### A. File Organization & Module Boundaries

#### Current File Map

```
src/Web/
├── index.html                 # HTML shell — single <div id="root">
├── vite.config.js             # Vite dev server + proxy config
├── package.json               # Dependencies — type: "module"
├── .env                       # VITE_COMMUNITY_API_URL
├── eslint.config.js           # ESLint 9 flat config
└── src/
    ├── main.jsx               # ReactDOM.createRoot — NEVER edit unless changing providers
    ├── App.jsx                # PRIMARY FILE — routing, pages, shared components, hooks
    ├── App.css                # ALL component styles (scoped by class names)
    ├── index.css              # Global resets, CSS custom properties, font imports
    ├── components/
    │   ├── Header.jsx         # Standalone header (legacy/shared)
    │   └── CollaborativeEditor.jsx  # Monaco + Yjs real-time editor
    └── assets/                # Static images, SVGs
```

#### Module Boundary Rules

1. **`App.jsx` is the monolith file**. All page-level components, utility functions, custom hooks, and shared UI components currently live here. When adding a new feature:
   - If it is a **page component** or **inline UI component** → add it to `App.jsx`.
   - If it is a **standalone reusable widget** with its own lifecycle (e.g., a complex editor, a real-time component) → create a new file in `components/`.
   - The threshold: if the component exceeds ~150 lines AND is used in only one place AND has complex internal state (WebSocket connections, external library integration), it MAY be extracted to `components/`.

2. **`index.css`** contains ONLY:
   - `@import` for Google Fonts
   - CSS custom properties on `:root`
   - Global resets (`*`, `html`, `body`, `button`, `input`, etc.)
   - Focus-visible styles
   - Legacy collaboration component styles (`.app-container`, `.header`, `.panel`, etc.)
   - Yjs remote selection styles

3. **`App.css`** contains ALL component-level styles scoped by class name. New styles MUST be added here. Style blocks MUST be grouped by component and separated by a comment header.

4. **`main.jsx`** MUST NOT be modified unless adding a global React context provider or changing the root mount strategy.

5. **`vite.config.js`** MUST be updated when:
   - A new backend service requires a dev proxy route.
   - The dev server configuration changes.

#### When to Create a New File

| Scenario | Action |
|---|---|
| New page component (< 150 lines) | Add to `App.jsx` |
| New page component (> 150 lines, self-contained) | Add to `App.jsx` — extract later if reuse emerges |
| New shared UI component (used by ≥ 2 pages) | Add to `App.jsx` as a function, near other shared components |
| Complex widget with external library (Monaco, Yjs, charts) | Create `components/<WidgetName>.jsx` |
| New custom hook (< 30 lines) | Add to `App.jsx` near other hooks |
| New custom hook (> 30 lines or reused across files) | Create `hooks/<hookName>.js` |
| New utility function | Add to `App.jsx` near existing utilities |
| New CSS styles | Add to `App.css` under a component comment header |
| New CSS custom property | Add to `index.css` inside `:root` |

### B. Component Taxonomy & Composition Rules

#### Component Categories

The frontend uses four categories of components. Each has specific rules:

**1. Page Components** (rendered by `<Route>`)
- Examples: `Dashboard`, `ArticleDetails`, `ArticleEditor`, `ArticleCollection`, `Recommendations`, `EndpointWorkbench`
- MUST receive `api` as a prop (the `useApi()` return value)
- MUST be registered in the `<Routes>` inside `Shell`
- MUST use `<AsyncState>` or manual loading/error states for API calls
- MUST use `<PageHeading>` for the page title area

**2. Layout Components** (structural wrappers)
- Examples: `Shell`, `PageErrorBoundary`
- `Shell` owns the `<Header>` + `<Routes>` composition
- `PageErrorBoundary` wraps `<Routes>` to catch uncaught render errors

**3. Shared UI Components** (reusable across pages)
- Examples: `ArticleCard`, `ArticleList`, `CompactArticleList`, `CommentTree`, `CommentNode`, `VoteControl`, `TagRow`, `AuthorLine`, `EndpointCoverage`, `PageHeading`, `Pagination`, `AsyncState`, `EmptyState`, `MethodBadge`
- MUST be pure presentational where possible — receive data via props, emit events via callbacks
- MUST NOT call `useApi()` directly (exception: components that explicitly need auth state)

**4. External Integration Components** (in `components/`)
- Examples: `CollaborativeEditor`, `Header`
- MAY manage their own state, WebSocket connections, or external library instances
- MUST be imported into `App.jsx` when used

#### Composition Pattern

```
App (BrowserRouter)
└── Shell (receives api)
    ├── Header (receives api — for token management)
    └── PageErrorBoundary
        └── Routes
            ├── Dashboard (page)
            │   ├── ArticleList → ArticleCard (shared)
            │   ├── Pagination (shared)
            │   └── CompactArticleList (shared)
            ├── ArticleDetails (page)
            │   ├── VoteControl (shared)
            │   ├── CommentTree → CommentNode (shared)
            │   └── TagRow (shared)
            ├── ArticleEditor (page)
            ├── ArticleCollection (page)
            ├── Recommendations (page)
            └── EndpointWorkbench (page)
```

#### Adding a New Page Component — Checklist

1. Define the function component in `App.jsx` (before the `App()` function, near other page components).
2. Accept `{ api }` as props.
3. Add a `<Route path="/your-path" element={<YourPage api={api} />} />` inside `Shell`'s `<Routes>`.
4. Add a `<NavLink>` in `Header` if it should appear in the navigation bar.
5. Add styles to `App.css` under a `/* YourPage */` comment header.
6. Update the `ENDPOINTS` array if the page consumes new API endpoints.

### C. Data Flow & State Management

#### State Architecture

```
┌──────────────────────────────────────────────────────┐
│                      App()                           │
│  ┌──────────────┐                                    │
│  │  useApi()    │ → { request, token, setToken,      │
│  │  (hook)      │    isAuthenticated }               │
│  └──────┬───────┘                                    │
│         │ passed as `api` prop                       │
│         ▼                                            │
│  ┌──────────────┐                                    │
│  │   Shell      │                                    │
│  │  ┌─────────┐ │                                    │
│  │  │ Header  │ │ ← api.token, api.setToken          │
│  │  └─────────┘ │                                    │
│  │  ┌─────────────────────────────────────────────┐  │
│  │  │ Page Components                             │  │
│  │  │  - Own loading/error/data state (useState)  │  │
│  │  │  - Fetch via api.request(path, options)     │  │
│  │  │  - useResource() for secondary resources    │  │
│  │  │  - useCallback for stable fetch functions   │  │
│  │  └─────────────────────────────────────────────┘  │
│  └──────────────┘                                    │
└──────────────────────────────────────────────────────┘
```

#### Data Fetching Patterns

**Pattern 1: Page-Critical Data** (loading blocks the entire page)
```jsx
const [data, setData] = useState(null);
const [loading, setLoading] = useState(true);
const [error, setError] = useState("");

const load = useCallback(async () => {
  setLoading(true);
  setError("");
  try {
    setData(await api.request("/api/path"));
  } catch (err) {
    setError(err.message);
  } finally {
    setLoading(false);
  }
}, [api, /* other deps */]);

useEffect(() => { load(); }, [load]);
```
Use `<AsyncState loading={loading} error={error}>` to render.

**Pattern 2: Secondary/Optional Data** (404 = empty, not error)
```jsx
const resource = useResource(
  api,
  (a) => a.request("/api/optional-data"),
  [api]
);
// resource = { data, loading, error, isEmpty, reload }
```
Use conditional rendering: `resource.loading ? <Loader> : resource.isEmpty ? <EmptyState> : resource.error ? <ErrorState> : <Content>`

**Pattern 3: Mutations** (user-triggered actions)
```jsx
const [busy, setBusy] = useState("");

const doAction = async () => {
  setBusy("action-name");
  try {
    await api.request("/api/path", { method: "POST", body: JSON.stringify(payload) });
    // Update local state or reload
  } catch (err) {
    setError(err.message);
  } finally {
    setBusy("");
  }
};
```

#### Rules

- `useApi()` MUST be called exactly once in `App()` and passed down as a prop. NEVER call `useApi()` in child components.
- All API paths MUST be relative (e.g., `/api/articles`). The `API_BASE` prefix is prepended by `useApi().request`.
- GET request deduplication is handled automatically by `IN_FLIGHT_GET_REQUESTS` — do not add your own deduplication.
- `useCallback` MUST wrap any function passed as a dependency to `useEffect` or `useResource`.
- `useMemo` MUST wrap any derived data that is expensive to compute or used as a dependency.
- `useRef` MUST be used for mutable values that should not trigger re-renders (e.g., `apiRef` in `useResource`).

### D. Routing & Navigation

#### Route Registration

All routes MUST be registered in the `Shell` component:

```jsx
<Routes>
  <Route path="/" element={<Dashboard api={api} />} />
  <Route path="/articles/new" element={<ArticleEditor api={api} mode="create" />} />
  <Route path="/articles/:slug" element={<ArticleDetails api={api} />} />
  <Route path="/articles/:slug/edit" element={<ArticleEditor api={api} mode="edit" />} />
  <Route path="/me" element={<ArticleCollection api={api} mode="mine" />} />
  <Route path="/bookmarks" element={<ArticleCollection api={api} mode="bookmarks" />} />
  <Route path="/recommendations" element={<Recommendations api={api} />} />
  <Route path="/endpoints" element={<EndpointWorkbench api={api} />} />
</Routes>
```

#### Rules

- Route paths MUST be lowercase kebab-case.
- Dynamic segments MUST use `:paramName` syntax.
- `useParams()` MUST be used to read route parameters.
- `useNavigate()` MUST be used for programmatic navigation.
- `useSearchParams()` MUST be used for query string state (e.g., filters, pagination).
- `<NavLink>` MUST be used in navigation bars (provides active state styling).
- `<Link>` MUST be used for all other in-app navigation — no `<a href>` for internal routes.

### E. Styling Conventions

#### CSS Architecture

| File | Purpose | When to Edit |
|---|---|---|
| `index.css` | Global resets, `:root` custom properties, font imports, legacy collab styles | Adding a new CSS variable or modifying global resets |
| `App.css` | All component styles, scoped by class names | Adding/modifying any component's appearance |

#### Naming Convention

- CSS class names MUST use **kebab-case** (e.g., `article-card`, `page-heading`, `vote-control`).
- Component-specific classes MUST be prefixed with the component's semantic name (e.g., `article-card-main`, `comment-header`, `endpoint-layout`).
- State modifiers MUST use separate classes (e.g., `active`, `compact`, `narrow`) — no BEM notation.
- Method-specific classes use the method name (e.g., `.method-badge.get`, `.method-badge.post`).

#### Design Tokens (from `:root`)

```css
:root {
  font-family: Inter, ui-sans-serif, system-ui, ...;
  color: #202124;
  background: #f7f4ee;
}
```

When adding new features:
- Background tones MUST harmonize with `#f7f4ee` (warm off-white).
- Text MUST use `#202124` or lighter shades for secondary text.
- Primary accent: teal `#216869` (derived from focus-visible outline).
- Interactive focus: `rgba(33, 104, 105, 0.28)` outline.
- MUST NOT introduce a conflicting colour palette.

#### Style Block Template

When adding styles for a new component, use this structure in `App.css`:

```css
/* ── YourComponentName ── */

.your-component { ... }
.your-component-header { ... }
.your-component .active { ... }
```

### F. Feature Addition Recipe (Step-by-Step)

Follow this exact sequence when adding a new frontend feature:

#### Step 1: Constitution Check

Before writing any code, verify the feature does not violate principles I–XII. Document findings in the plan's "Constitution Check" table.

#### Step 2: API Surface

1. Identify which backend service(s) the feature calls.
2. Verify the API routes exist in the backend.
3. Add new entries to the `ENDPOINTS` array in `App.jsx` if adding new API paths.
4. If calling a new backend service, add a proxy route in `vite.config.js`.

#### Step 3: Data Layer

1. Define the data fetching approach (Pattern 1, 2, or 3 from Section C).
2. Add any new helper functions (e.g., `getField` wrappers, formatters) near existing helpers.
3. If the feature needs a new custom hook, add it near `useApi()` and `useResource()`.

#### Step 4: Component Implementation

1. Add the page component function to `App.jsx`.
2. Accept `{ api }` as props.
3. Implement loading, error, and data states.
4. Compose using existing shared components (`AsyncState`, `EmptyState`, `Pagination`, `PageHeading`, etc.).
5. Add the `<Route>` in `Shell`.
6. Add the `<NavLink>` in `Header` (if navigable).

#### Step 5: Styling

1. Add component styles to `App.css` under a comment header.
2. Reuse existing class patterns (`.button`, `.panel-block`, `.loading-state`, `.toolbar`, etc.).
3. If a new CSS variable is needed, add it to `:root` in `index.css`.

#### Step 6: Verification

1. Verify the page renders without errors.
2. Verify loading states display correctly.
3. Verify error states display correctly (simulate a failed API call).
4. Verify 404 responses are handled gracefully (empty state, not crash).
5. Verify navigation to/from the new page works.
6. Verify the page is responsive (min-width: 320px).

### G. Feature Modification Recipe

When modifying an existing feature:

1. **Locate** the component in `App.jsx` — search by function name or route path.
2. **Identify scope**: Is it the page component, a shared component, a hook, or a utility?
3. **Assess blast radius**: What other components use the same shared components or hooks?
4. **Make the change**: Follow the same patterns used by surrounding code.
5. **Preserve existing behaviour**: Do not change function signatures, prop interfaces, or return shapes of hooks unless the plan explicitly calls for it.
6. **Update styles**: If the change affects layout or appearance, update `App.css`.
7. **Update `ENDPOINTS`**: If API paths changed, update the array.

### H. Anti-Patterns (MUST NOT)

The following patterns are explicitly **FORBIDDEN** and MUST NOT be introduced:

| Anti-Pattern | Why Forbidden | Do Instead |
|---|---|---|
| Calling `useApi()` in child components | Creates duplicate token state; breaks single-source-of-truth | Pass `api` as a prop from `App()` |
| Using `fetch()` directly | Bypasses token injection, deduplication, and error normalisation | Use `api.request()` |
| Storing state in global variables | Breaks React's rendering model | Use `useState`, `useRef`, or `useCallback` |
| Using `class` components | Violates React 19 hooks-only rule | Functional components (exception: `PageErrorBoundary`) |
| Adding Redux, Zustand, or Jotai | Over-engineering for current scale | Prop drilling + `useApi()` pattern |
| Inline styles (`style={{}}`) | Defeats CSS organisation | Add classes to `App.css` |
| CSS-in-JS (styled-components, emotion) | Violates Vanilla CSS rule | `App.css` |
| Tailwind / Bootstrap / any CSS framework | Violates tech stack rule — **exception**: Tailwind CSS is authorized for new UI components per Section XIII | Vanilla CSS (unless component is new) |
| Direct DOM manipulation (`document.querySelector`) | Breaks React's virtual DOM | Use refs (`useRef`) |
| Magic string API URLs | Breaks proxy and gateway routing | Use `API_BASE` + relative paths |
| `any` type assertions (if TypeScript is ever added) | Defeats type safety | Proper typing |
| Creating a separate `services/` or `api/` directory | Premature abstraction for current codebase scale | `useApi()` hook in `App.jsx` |
| Importing icons from anything other than `lucide-react` | Violates icon library rule | `import { IconName } from "lucide-react"` |

### I. Backend Feature Addition Reference

When a frontend feature requires backend changes, follow these patterns per service:

#### CoreJudge (Clean Architecture)

1. **Domain**: Add entity in `CoreJudge.Domain/Entities/`, domain event in `Events/`.
2. **Application**: Add command/query in `Application/Features/<Feature>/`, handler, validator, DTOs.
3. **Infrastructure**: Add EF Core configuration in `Infrastructure/Configurations/`, repository implementation.
4. **API**: Add controller in `API/Controllers/`, register in DI.
5. **Migration**: `dotnet ef migrations add <Name>` from the API project.

#### Community (Vertical Slice)

1. Create folder: `Features/<FeatureName>/<ActionName>/`.
2. Add `<Action>Command.cs`, `<Action>Handler.cs`, `<Action>Endpoint.cs`, `<Action>Validator.cs`.
3. Endpoint auto-registers via `IEndpoint` scanning.

#### Users (Carter)

1. Add `ICarterModule` implementation.
2. Delegate logic to `IUserService`.
3. Register module in `Program.cs`.

#### Cross-Service Communication

1. Define event record in the publishing service's `Domain/Events/`.
2. Define consumer `IConsumer<TEvent>` in the subscribing service.
3. Register consumer with `x.AddConsumer<TConsumer>()`.
4. Verify outbox pattern is enabled if the publisher uses EF Core.

#### Gateway

1. Add route cluster in `api.gateway`'s YARP configuration.
2. Follow existing route pattern: `/<service-prefix>/{**catch-all}`.

---

## XIII. Missing API Mocking Strategy & UI Development

When adding features not yet supported by the backend or developing new UI components, all AI agents MUST strictly comply with the following instructions:

### A. Missing API Gateways & State Machine

If a screen or component relies on data/actions not yet provided by the current backend:

1. **Full Implementation Required**: Write complete React state hooks, custom fetch actions, local storage binders, validation states, and components as if the backend endpoints already exist. DO NOT skip coding the interactions, submit buttons, loading indicators, empty slots, or error display banners.
2. **Hook Pattern**: Create structured hooks (e.g., using the `useResource` or `useApi` pattern) to encapsulate the data loading flow.

### B. MSW Mocking & Sandboxing

1. **Mock Service Worker**: All unimplemented endpoints MUST be mocked in `src/Web/src/mocks/handlers.js` using MSW v2.
2. **Deterministic Data Simulation**: MSW handlers MUST mock realistic data payload matrices (e.g., paginated lists, model entities, validation faults) to enable complete front-end manual testing without launching an updated backend stack.
3. **State Simulators**: Include mock latency (e.g., `delay(500)`) and expose states to trigger edge cases (empty search results, resource 404s, backend 500 crashes) to verify front-end resilience.
4. **Worker Registration**: The MSW service worker MUST be conditionally started in `src/Web/src/main.jsx` for development mode only. The worker script (`mockServiceWorker.js`) MUST be initialized in `src/Web/public/` via `npx msw init public/ --save`.

### C. Missing API Documentation Compilation

All missing APIs developed during UI development MUST be documented in a central file `src/Web/src/docs/missing-apis.md`. For each endpoint, the document MUST list:

1. **Metadata**: Endpoint URL Route, HTTP Verb, and Auth Requirements.
2. **Usage Context**: Which UI component or Hook initiates this request.
3. **Request Specification**: Expected Headers and Request JSON body format.
4. **Response Specification**: JSON schemas for:
   - Success Responses (HTTP 200/201)
   - Validation Failures (HTTP 400 with Problem Details)
   - Resource Not Found (HTTP 404)
5. **Data Contract**: Entity JSON structures with accurate data types (e.g., UUID strings, datetime stamps).

### D. Design System Source

The application design system MUST adhere to the following rules:

- **Color palette**: Deep slate backgrounds (`#0b1326`), primary accent (`#adc6ff` / `#4d8eff`), secondary (`#ddb7ff`), tertiary (`#4cd7f6`), error (`#ffb4ab`).
- **Typography**: Inter for UI text; JetBrains Mono for labels, metadata, and reputation scores.
- **Elevation model**: Layered glassmorphism — `backdrop-filter: blur(12px)`, 1px borders at 8–15% white opacity.
- **Spacing rhythm**: 8px base unit; component padding `16px`; feed item gap `24–32px`.
- **Shapes**: Rounded 4px (small), 8px (cards), pill (badges/avatars).

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

**Version**: 1.2.0 | **Ratified**: 2026-05-22 | **Last Amended**: 2026-05-29
