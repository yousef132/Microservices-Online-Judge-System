# Data Model: Frontend 404 Handling

## Entities

This feature is UI-only and does not introduce new backend entities or database changes. The "entities" below describe the in-memory state shapes used by the new frontend abstractions.

### ResourceState

The return value of the `useResource()` hook. Represents the lifecycle of a single API resource fetch.

| Field | Type | Description |
|---|---|---|
| `data` | `T \| null` | The fetched data when the request succeeds. `null` while loading or on error. |
| `loading` | `boolean` | `true` while the request is in flight. |
| `error` | `string` | Non-empty error message for non-404 errors. Empty string when no error. |
| `isEmpty` | `boolean` | `true` when the API returned a 404 (resource not found). `false` otherwise. |
| `reload` | `() => void` | Function to re-trigger the fetch. |

**State transitions:**

```
Initial → loading=true, data=null, error="", isEmpty=false
Success → loading=false, data=<response>, error="", isEmpty=false
NotFound → loading=false, data=null, error="", isEmpty=true
Error    → loading=false, data=null, error=<message>, isEmpty=false
```

### EmptyStateProps

Props for the enhanced `EmptyState` component.

| Prop | Type | Required | Description |
|---|---|---|---|
| `title` | `string` | Yes | Main heading text |
| `body` | `string` | No | Supporting description text |
| `icon` | `React.ComponentType` | No | A lucide-react icon component (e.g., `FileText`) |
| `action` | `{ label: string, onClick: () => void }` | No | Optional call-to-action button |

### PageErrorBoundaryState

Internal state of the `PageErrorBoundary` class component.

| Field | Type | Description |
|---|---|---|
| `hasError` | `boolean` | Whether an uncaught error was caught by the boundary. |
| `error` | `Error \| null` | The caught error object for display. |
