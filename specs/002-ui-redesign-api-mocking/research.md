# Research: Community Hub UI Redesign & Missing API Mocking

**Feature**: `002-ui-redesign-api-mocking` | **Date**: 2026-05-29

## Decision Log

### 1. Tailwind CSS Installation Method

- **Decision**: Use `@tailwindcss/vite` plugin (Tailwind CSS v4 native Vite integration)
- **Rationale**: The project uses Vite 8. The `@tailwindcss/vite` plugin is the recommended integration for Vite and requires zero `tailwind.config.js` — it auto-detects class usage. It also eliminates the PostCSS pipeline.
- **Alternatives considered**: PostCSS plugin (`tailwindcss` + `autoprefixer`) — more configuration overhead; CDN script tag — cannot use Tailwind v4's JIT with a CDN.

### 2. MSW v2 Setup Pattern

- **Decision**: Create `src/mocks/browser.js` exporting the MSW worker, and `src/mocks/handlers.js` exporting the handlers array. In `main.jsx`, conditionally start the worker using `import.meta.env.DEV`.
- **Rationale**: This is the canonical MSW v2 browser-mode pattern. Keeping handlers in a separate file allows gradual handler deletion as real endpoints are implemented.
- **Alternatives considered**: Inline handlers in `main.jsx` — too noisy; separate file per handler — unnecessary for current scale.

### 3. Token Expiry Handling Strategy

- **Decision**: A `useAuth(api)` hook wraps the `api.request` function and intercepts HTTP 401 responses. On a 401, it clears the token and redirects to `/auth?returnUrl=<current-path>`.
- **Rationale**: Centralizing 401 handling in one hook prevents every page component from needing its own expiry logic. The `returnUrl` param allows seamless re-authentication UX (FR-026).
- **Alternatives considered**: Axios interceptors — project uses native `fetch`; global error boundary — too coarse-grained, cannot distinguish 401 from other errors.

### 4. Notification Count — Navigation-Driven Poll

- **Decision**: A `useNotificationCount(api)` hook subscribes to `useLocation()` from react-router-dom. Whenever `location` changes (path, search, or hash), it fires a `GET /api/notifications/unread-count` request if the user is authenticated.
- **Rationale**: This is the user's explicit requirement (FR-027). It avoids a background interval that would run even on static pages and aligns notification freshness with navigation intent.
- **Alternatives considered**: `setInterval` polling — continues in background unnecessarily; SSE/WebSocket — introduces backend infrastructure that doesn't exist yet.

### 5. Avatar Upload — S3 Presigned URL Flow

- **Decision**: A `usePresignedUpload()` hook: (1) calls `POST /api/users/avatar-upload-url` to get a presigned PUT URL; (2) directly PUTs the file to the storage provider URL with the correct `Content-Type` header; (3) calls `PUT /api/users/profile-settings` with the returned storage key to update the profile record.
- **Rationale**: Presigned URL direct upload avoids routing large files through the API gateway, eliminates server memory pressure, and is the user's explicitly chosen pattern (FR-029, Q5 clarification).
- **Alternatives considered**: Multipart through gateway — adds load on gateway; URL-based avatar — rejected by user.

### 6. Comment Threading Depth

- **Decision**: Single level — top-level comments only support direct replies; replies cannot themselves be replied to (FR-005, Q1 clarification).
- **Rationale**: Matches the current Community API data model and simplifies the thread rendering component significantly.
- **Alternatives considered**: Unlimited nesting — requires recursive component rendering and deeper API pagination; two levels — user chose single for now.

### 7. Guest Access to Article Detail Pages

- **Decision**: `/articles/:slug` is publicly accessible in read-only mode (FR-028, Q4 clarification). Interaction buttons (vote, comment, bookmark) are rendered but clicking triggers the auth flow (`/auth?returnUrl=<article-url>`).
- **Rationale**: Maximises SEO and content shareability — linked articles work for any recipient, not just logged-in users.
- **Alternatives considered**: Full private — eliminates external sharing; partial content hide — complex conditional rendering with unclear UX value.

### 8. Single-File Architecture Adherence

- **Decision**: All new page components are defined in `App.jsx`. New shared hooks (`useAuth`, `useNotificationCount`, `usePresignedUpload`) are added in the top "hooks" section before component definitions.
- **Rationale**: The project constitution (XII.A) mandates the single-file architecture. Adding new files for individual page components would violate this principle.
- **Alternatives considered**: Component files in `src/components/` — only permitted for externally-integrated components (e.g., `CollaborativeEditor.jsx`).

### 9. Admin/Moderator Route Gating

- **Decision**: Auth-required routes check `api.isAuthenticated`. Admin routes additionally check a `role` field decoded from the JWT payload (claims: `Admin`, `SuperAdmin`). A `RequireRole` wrapper component handles both redirect cases.
- **Rationale**: JWT role claims are already issued by `users.api`. Decoding the payload client-side for display/gating is safe since actual enforcement happens server-side.
- **Alternatives considered**: Server-side route guard — requires a backend roundtrip on every navigation; `useContext` for role state — unnecessary given the JWT already carries the claims.
