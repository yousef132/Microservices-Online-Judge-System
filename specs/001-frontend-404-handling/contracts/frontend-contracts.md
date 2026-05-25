# Contracts: Frontend 404 Handling

This feature is entirely frontend (React SPA). It does not introduce or modify any backend API contracts.

## Internal Component Contracts

### `useResource(fetchFn, deps)` Hook

**Signature:**
```
useResource(fetchFn: () => Promise<T>, deps: any[]) → ResourceState<T>
```

**Behaviour:**
- Calls `fetchFn` on mount and whenever `deps` change.
- If the response resolves, sets `data` and `isEmpty = false`.
- If the error has `status === 404`, sets `isEmpty = true` and does NOT set an error string.
- If the error has any other status, sets `error` to the error message.
- Exposes a `reload()` function to re-trigger the fetch.

### `EmptyState` Component

**Props:** See `EmptyStateProps` in `data-model.md`.

**Rendering contract:**
- Always renders a `<div className="empty-state">`.
- If `icon` is provided, renders the icon above the title.
- If `action` is provided, renders a button below the body text.

### `PageErrorBoundary` Component

**Props:**
- `children: React.ReactNode` — the subtree to protect.

**Rendering contract:**
- Renders `children` normally when no uncaught error exists.
- On uncaught error, renders a full-page fallback UI with a retry button that resets the boundary and reloads the page.
- Does NOT catch errors from `useResource()` (those are handled via state, not throws).
