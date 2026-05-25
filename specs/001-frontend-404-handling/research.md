# Research: Frontend 404 Handling

## Current State Analysis

### Codebase Audit

All frontend code lives in a single file: `src/Web/src/App.jsx` (1285 lines). There is one additional component file: `src/Web/src/components/CollaborativeEditor.jsx`.

### Existing Error Handling Patterns

**`useApi()` hook** (App.jsx:163–203):
- Central HTTP abstraction used by all components.
- On any non-2xx response, it constructs an `Error` with `error.status` and `error.details` and **throws**.
- No distinction between 404, 5xx, or network errors at the hook level.

**`AsyncState` component** (App.jsx:1229–1242):
- Renders a loading spinner when `loading === true`.
- Renders an `EmptyState` with title "Request failed" when `error` is truthy.
- Renders children otherwise.
- **Issue**: A 404 on a non-critical component data request shows the same error UI as a total server failure.

**`EmptyState` component** (App.jsx:1244–1251):
- Accepts only `title` and `body` props.
- No icon, no action button.
- Used for both "no data" and "error" states interchangeably.

### Component-by-Component Error Behaviour

| Component | API Calls | Error Behaviour | Classification |
|---|---|---|---|
| `Dashboard` | articles feed + recommendations | Feed error = page error; recommendations silently caught (`.catch(() => [])`) | Feed: page-critical; Recommendations: component-level |
| `ArticleDetails` | article + comments | 404 on article = entire page shows "Request failed" | Article: page-critical; Comments: component-level |
| `ArticleCollection` | articles/me or bookmarks | 404 = page error | Page-critical |
| `Recommendations` | /api/recommendations | 404 = page error | Component-level (should be empty state) |
| `ArticleEditor` | load article (edit mode) | 404 = page error | Page-critical (editing a nonexistent article is a real error) |
| `EndpointWorkbench` | user-triggered requests | Error shown inline | Already handled correctly (component-level) |

### Key Findings

1. **No React Error Boundary exists** — any uncaught throw in render crashes the entire app.
2. **All errors bubble to a single `error` state** per page component — no isolation between components sharing a page.
3. **Recommendations in Dashboard** already use a partial pattern (`.catch(() => [])`), but it's ad-hoc and not reusable.
4. **`EmptyState`** is too simple — it cannot distinguish between "no data available" and "resource not found" visually.

---

## Design Decisions

### Decision 1: Enhance `useApi()` vs. Create `useResource()`

- **Decision**: Create a new `useResource()` hook that wraps `useApi().request`.
- **Rationale**: `useApi()` is a low-level HTTP abstraction. `useResource()` adds state management (loading/data/error/empty) and 404 classification. Separating concerns keeps the codebase clean.
- **Alternatives considered**:
  - Modify `useApi()` directly — rejected because it would bloat the HTTP layer with UI state concerns.
  - Interceptor pattern in fetch — rejected because React state management still needs a hook.

### Decision 2: Error Boundary Strategy

- **Decision**: Create a single `PageErrorBoundary` class component wrapping `<Routes>` content.
- **Rationale**: React Error Boundaries require class components. A single boundary at the route level catches uncaught rendering errors while component-level 404s are handled by `useResource()` before they can throw.
- **Alternatives considered**:
  - Per-component error boundaries — rejected as over-engineering for the current app size.
  - Third-party library (react-error-boundary) — rejected per constitution: no new dependencies without amendment.

### Decision 3: EmptyState Enhancement

- **Decision**: Extend the existing `EmptyState` component with `icon` and `action` props.
- **Rationale**: Backward-compatible change — existing callers that only pass `title`/`body` continue to work. New callers can add a lucide-react icon and a call-to-action button.
- **Alternatives considered**:
  - New `ResourceEmptyState` component — rejected to avoid component proliferation in a single-file codebase.

### Decision 4: Classification Strategy

- **Decision**: Each component decides at the call site whether a 404 is "expected empty" or "critical error" by choosing `useResource()` (non-critical, 404 → empty) vs. `useApi().request` with traditional try/catch (critical, 404 → error).
- **Rationale**: Simple, explicit, no runtime config needed. The developer intent is clear in the code.
- **Alternatives considered**:
  - Central registry of critical vs. non-critical endpoints — rejected as over-engineering.

---

## Technology Fit

All decisions align with the constitution:
- **React 19 + hooks** — `useResource()` is a custom hook.
- **Vanilla CSS** — empty state styling added to `App.css`.
- **lucide-react** — icons for empty states.
- **No new dependencies** introduced.
