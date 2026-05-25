# Implementation Plan: Frontend 404 Handling

**Branch**: `001-frontend-404-handling` | **Date**: 2026-05-23 | **Spec**: [spec.md](file:///d:/Microservices-Online-Judge-System/specs/001-frontend-404-handling/spec.md)

**Input**: Feature specification from `specs/001-frontend-404-handling/spec.md`

## Summary

HTTP 404 errors in the frontend currently break entire pages. This plan introduces component-level 404 isolation through a new `useResource()` hook that classifies 404 responses as empty states rather than errors, an enhanced `EmptyState` component with icon and action support, and a `PageErrorBoundary` for uncaught critical failures. All components using API calls will be refactored to use the new pattern.

## Technical Context

**Language/Version**: JavaScript (ES Modules) — React 19

**Primary Dependencies**: React 19, react-router-dom 7, lucide-react, Vite 8

**Storage**: N/A (frontend-only change, no backend/DB changes)

**Testing**: Manual browser testing + visual inspection (no existing test framework in the frontend)

**Target Platform**: Web browser (SPA served by Vite dev server / production build)

**Project Type**: Web application (Single Page Application)

**Performance Goals**: No performance regression — hooks must not introduce unnecessary re-renders

**Constraints**: Single-file architecture (`App.jsx` contains all components); Vanilla CSS only; no new dependencies

**Scale/Scope**: 7 page/component functions making API calls; 1 shared hook; 1 shared empty state component; 1 error boundary

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|---|---|---|
| **I. Global System Architecture** | ✅ Pass | No new services or infrastructure. Frontend-only change. |
| **IV. Tech Stack** | ✅ Pass | React 19, Vite 8, lucide-react, Vanilla CSS — all existing. No new dependencies. |
| **X. Frontend Architecture** | ✅ Pass | React 19 with hooks, functional components, `useApi()` pattern preserved, Vanilla CSS, lucide-react icons. |
| **XI. Code Style & Patterns** | ✅ Pass | Functional components with hooks. `useCallback`/`useMemo` used where needed. ES Modules. |

**Post-Phase-1 Re-check**: ✅ All gates still pass. Design introduces `useResource()` as a new custom hook (consistent with `useApi()` pattern). `PageErrorBoundary` uses a class component (required by React for Error Boundaries — this is the only permitted exception to the hooks-only rule).

## Project Structure

### Documentation (this feature)

```text
specs/001-frontend-404-handling/
├── plan.md                         # This file
├── research.md                     # Phase 0 output — codebase audit & design decisions
├── data-model.md                   # Phase 1 output — state shapes for hooks/components
├── quickstart.md                   # Phase 1 output — how to run and test
├── contracts/
│   └── frontend-contracts.md       # Phase 1 output — hook/component interface contracts
└── tasks.md                        # Phase 2 output (created by /speckit-tasks)
```

### Source Code (repository root)

```text
src/Web/src/
├── App.jsx              # All components — modified to add useResource(), PageErrorBoundary, enhanced EmptyState
├── App.css              # Component styles — modified to add empty state enhancements, error boundary styles
├── index.css            # Global resets + CSS variables (unchanged)
├── main.jsx             # React root mount (unchanged)
├── components/
│   └── CollaborativeEditor.jsx   # (unchanged — does not make API calls that return 404)
└── assets/              # (unchanged)
```

**Structure Decision**: All changes are within the existing single-file architecture (`App.jsx` + `App.css`). No new files are created in the source tree — this aligns with the project's current structure.

## Implementation Phases

### Phase A: Core Abstractions (App.jsx)

1. **Create `useResource()` hook** — new custom hook that:
   - Accepts a fetch function and dependency array
   - Calls `useApi().request` internally
   - Returns `{ data, loading, error, isEmpty, reload }`
   - Classifies `error.status === 404` as `isEmpty = true` (not an error)
   - All other errors set `error` string

2. **Enhance `EmptyState` component** — add optional props:
   - `icon` — lucide-react component rendered above title
   - `action` — `{ label, onClick }` rendered as a button below body

3. **Create `PageErrorBoundary`** — React class component:
   - Wraps route content in `Shell`
   - `componentDidCatch` logs the error
   - Renders a fallback UI with "Something went wrong" and a "Reload" button
   - Does NOT catch `useResource()` errors (those are state-managed, never thrown into render)

### Phase B: Component Refactoring (App.jsx)

4. **Refactor `Dashboard`** — separate recommendations into `useResource()` (404 → empty state in sidebar), keep articles feed as page-critical with existing try/catch
5. **Refactor `ArticleDetails`** — keep article fetch as page-critical, separate comments fetch into `useResource()` (404 → "No comments yet")
6. **Refactor `Recommendations`** — use `useResource()` so 404 → empty state instead of error
7. **Refactor `ArticleCollection`** — keep as page-critical (404 on my-articles/bookmarks is a genuine error)
8. **Update `AsyncState`** — add handling for `isEmpty` prop (render `EmptyState` with appropriate icon)

### Phase C: Styling & Polish (App.css)

9. **Add empty state icon styles** — center icon, muted colour, appropriate spacing
10. **Add empty state action button styles** — consistent with existing button design system
11. **Add error boundary fallback styles** — full-page centered fallback

### Phase D: Integration & Wiring

12. **Wire `PageErrorBoundary`** into `Shell` around `<Routes>`
13. **Verify all components** — ensure no 404 breaks a full page, empty states render correctly

## Complexity Tracking

No constitution violations. No new dependencies, no new services, no new database engines.
