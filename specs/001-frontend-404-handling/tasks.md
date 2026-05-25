# Tasks: Frontend 404 Handling

**Input**: Design documents from `specs/001-frontend-404-handling/`

**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Not requested — no test tasks generated.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Context for Implementers

**All changes are in two files**: `src/Web/src/App.jsx` and `src/Web/src/App.css`. There are no new files to create in the source tree.

**Key existing code locations in `src/Web/src/App.jsx`**:
- `useApi()` hook: lines 163–203 — the central HTTP abstraction. **Do NOT modify this hook.**
- `AsyncState` component: lines 1229–1242 — renders loading/error/children.
- `EmptyState` component: lines 1244–1251 — currently accepts only `title` and `body` props.
- `Shell` component: lines 205–221 — renders `<Header>` and `<Routes>`.
- `Dashboard` component: lines 321–441 — fetches articles + recommendations.
- `ArticleDetails` component: lines 535–784 — fetches article + comments.
- `Recommendations` component: lines 487–533 — fetches `/api/recommendations`.
- `ArticleCollection` component: lines 444–485 — fetches my-articles or bookmarks.
- `App` component: lines 1274–1284 — renders `<BrowserRouter><Shell /></BrowserRouter>`.

**Important**: Since all components live in one file, tasks that modify `App.jsx` CANNOT be parallelized. Follow the exact order below.

---

## Phase 1: Setup

**Purpose**: No project initialization needed — this is a feature addition to an existing codebase. This phase is a no-op.

*(No tasks — project is already initialized and running.)*

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create the core abstractions that ALL user stories depend on. These MUST be completed before any user story work begins.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T001 Create `useResource()` custom hook in `src/Web/src/App.jsx`

  **What to do**: Add a new function called `useResource` after the existing `useApi()` hook (after line 203). This hook wraps `useApi().request` and manages loading/data/error/empty state.

  **Exact code to add** (insert after the `useApi` function, before the `Shell` function):

  ```jsx
  function useResource(api, fetchFn, deps) {
    const [data, setData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [isEmpty, setIsEmpty] = useState(false);

    const load = useCallback(async () => {
      setLoading(true);
      setError("");
      setIsEmpty(false);
      setData(null);
      try {
        const result = await fetchFn(api);
        setData(result);
      } catch (err) {
        if (err.status === 404) {
          setIsEmpty(true);
        } else {
          setError(err.message || "Request failed");
        }
      } finally {
        setLoading(false);
      }
    }, deps);

    useEffect(() => {
      load();
    }, [load]);

    return { data, loading, error, isEmpty, reload: load };
  }
  ```

  **How it works**:
  - Takes `api` (the return value of `useApi()`), a `fetchFn` (an async function that receives `api` and returns data), and `deps` (dependency array for re-fetching).
  - When the fetch throws with `error.status === 404`, it sets `isEmpty = true` and does NOT set `error`.
  - When the fetch throws with any other status, it sets `error` to the error message.
  - Exposes `reload()` to re-trigger the fetch manually.

- [X] T002 Enhance the `EmptyState` component in `src/Web/src/App.jsx`

  **What to do**: Modify the existing `EmptyState` function (currently at lines 1244–1251) to accept two new optional props: `icon` and `action`.

  **Current code**:
  ```jsx
  function EmptyState({ title, body }) {
    return (
      <div className="empty-state">
        <h2>{title}</h2>
        <p>{body}</p>
      </div>
    );
  }
  ```

  **Replace with**:
  ```jsx
  function EmptyState({ title, body, icon: Icon, action }) {
    return (
      <div className="empty-state">
        {Icon && <Icon className="empty-state-icon" size={48} />}
        <h2>{title}</h2>
        {body && <p>{body}</p>}
        {action && (
          <button className="button subtle" type="button" onClick={action.onClick}>
            {action.label}
          </button>
        )}
      </div>
    );
  }
  ```

  **Backward compatibility**: Existing callers that only pass `title` and `body` continue to work. The `icon` and `action` props are optional.

- [X] T003 Add CSS styles for enhanced `EmptyState` in `src/Web/src/App.css`

  **What to do**: Add the following CSS rules to the end of `src/Web/src/App.css` (after all existing styles). These style the new icon and action button inside empty states.

  ```css
  /* --- Empty State Enhancements --- */
  .empty-state-icon {
    color: var(--text-secondary, #888);
    margin-bottom: 0.75rem;
    opacity: 0.6;
  }

  .empty-state .button {
    margin-top: 1rem;
  }

  /* --- Page Error Boundary --- */
  .error-boundary-fallback {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 60vh;
    text-align: center;
    padding: 2rem;
    gap: 1rem;
  }

  .error-boundary-fallback h1 {
    font-size: 1.5rem;
    color: var(--text-primary, #fff);
  }

  .error-boundary-fallback p {
    color: var(--text-secondary, #888);
    max-width: 480px;
  }
  ```

- [X] T004 Create `PageErrorBoundary` class component in `src/Web/src/App.jsx`

  **What to do**: Add a new React class component called `PageErrorBoundary` in `src/Web/src/App.jsx`. Insert it after the `useResource` hook (added in T001) and before the `Shell` function.

  **Why a class component**: React Error Boundaries REQUIRE class components — there is no hooks equivalent. This is the only class component in the codebase and that is intentional.

  **Exact code to add**:

  ```jsx
  class PageErrorBoundary extends React.Component {
    constructor(props) {
      super(props);
      this.state = { hasError: false, error: null };
    }

    static getDerivedStateFromError(error) {
      return { hasError: true, error };
    }

    render() {
      if (this.state.hasError) {
        return (
          <div className="error-boundary-fallback">
            <h1>Something went wrong</h1>
            <p>An unexpected error occurred. Please try reloading the page.</p>
            <button
              className="button primary"
              type="button"
              onClick={() => {
                this.setState({ hasError: false, error: null });
                window.location.reload();
              }}
            >
              <RefreshCcw size={16} />
              Reload page
            </button>
          </div>
        );
      }
      return this.props.children;
    }
  }
  ```

  **IMPORTANT**: You must also add `React` to the imports at the top of `App.jsx`. Change:
  ```jsx
  import { useCallback, useEffect, useMemo, useState } from "react";
  ```
  to:
  ```jsx
  import React, { useCallback, useEffect, useMemo, useState } from "react";
  ```

- [X] T005 Wire `PageErrorBoundary` into the `Shell` component in `src/Web/src/App.jsx`

  **What to do**: Modify the `Shell` function to wrap `<Routes>` with the `PageErrorBoundary`.

  **Current code** (lines 205–221):
  ```jsx
  function Shell({ api }) {
    return (
      <div className="community-app">
        <Header api={api} />
        <Routes>
          <Route path="/" element={<Dashboard api={api} />} />
          ...
        </Routes>
      </div>
    );
  }
  ```

  **Replace with**:
  ```jsx
  function Shell({ api }) {
    return (
      <div className="community-app">
        <Header api={api} />
        <PageErrorBoundary>
          <Routes>
            <Route path="/" element={<Dashboard api={api} />} />
            ...
          </Routes>
        </PageErrorBoundary>
      </div>
    );
  }
  ```

  The `<Routes>` block stays exactly the same — just wrap it with `<PageErrorBoundary>`.

**Checkpoint**: Foundation ready. The `useResource()` hook, enhanced `EmptyState`, `PageErrorBoundary`, and all CSS are in place. User story implementation can now begin.

---

## Phase 3: User Story 1 - Component-Level Missing Data (Priority: P1) 🎯 MVP

**Goal**: When one component's data request returns a 404, that component shows an empty state while the rest of the page remains fully functional.

**Independent Test**: On the Dashboard page, if the recommendations API returns 404, the sidebar shows a clean empty state. The articles feed still works. On the ArticleDetails page, if comments return 404, the comments section shows an empty state. The article content still renders.

### Implementation for User Story 1

- [X] T006 [US1] Refactor `Dashboard` component to use `useResource()` for recommendations in `src/Web/src/App.jsx`

  **What to do**: In the `Dashboard` function (starts at line 321), the recommendations fetch is currently embedded in `loadFeed()` inside `Promise.all`. Separate it out so recommendations use `useResource()` independently.

  **Step 1 — Remove recommendations from `loadFeed()`**. Change the `loadFeed` function so it ONLY fetches the articles feed. Remove the `recommendations` state variable and the `Promise.all` wrapping.

  **Current `loadFeed` code** (simplified):
  ```jsx
  const [recommendations, setRecommendations] = useState([]);

  const loadFeed = useCallback(async () => {
    setLoading(true);
    setError("");
    ...
    try {
      const [feed, picks] = await Promise.all([
        api.request(`/api/articles?${query}`),
        api.isAuthenticated
          ? api.request("/api/recommendations?limit=5").catch(() => [])
          : Promise.resolve([]),
      ]);
      setArticles(feed);
      setRecommendations(Array.isArray(picks) ? picks : []);
    } catch (loadError) {
      setError(loadError.message);
    } finally {
      setLoading(false);
    }
  }, ...);
  ```

  **Replace with** (only fetch articles in loadFeed — recommendations handled separately):
  ```jsx
  const loadFeed = useCallback(async () => {
    setLoading(true);
    setError("");
    const query = new URLSearchParams({
      sort: filters.sort,
      page: String(filters.page),
      pageSize: "12",
    });
    if (filters.tag) query.set("tag", filters.tag);

    try {
      setArticles(await api.request(`/api/articles?${query}`));
    } catch (loadError) {
      setError(loadError.message);
    } finally {
      setLoading(false);
    }
  }, [api, filters.page, filters.sort, filters.tag]);
  ```

  **Step 2 — Add `useResource()` for recommendations**. Add this line inside `Dashboard`, after the existing state declarations:
  ```jsx
  const recResource = useResource(
    api,
    (a) => a.request("/api/recommendations?limit=5"),
    [api]
  );
  ```

  **Step 3 — Update the recommendations sidebar JSX**. Replace the current recommendations rendering in the `<aside>` section:

  **Current**:
  ```jsx
  {api.isAuthenticated ? (
    <CompactArticleList articles={recommendations} emptyText="No recommendations yet." />
  ) : (
    <EmptyState title="Token required" body="Paste a bearer token to load personalized picks." />
  )}
  ```

  **Replace with**:
  ```jsx
  {api.isAuthenticated ? (
    recResource.loading ? (
      <div className="loading-state"><Loader2 className="spin" size={22} /> Loading</div>
    ) : recResource.isEmpty ? (
      <EmptyState icon={Sparkles} title="No recommendations" body="We don't have personalized picks for you yet." />
    ) : recResource.error ? (
      <EmptyState title="Could not load picks" body={recResource.error} action={{ label: "Retry", onClick: recResource.reload }} />
    ) : (
      <CompactArticleList articles={Array.isArray(recResource.data) ? recResource.data : []} emptyText="No recommendations yet." />
    )
  ) : (
    <EmptyState icon={Sparkles} title="Token required" body="Paste a bearer token to load personalized picks." />
  )}
  ```

  **Step 4 — Remove the old `recommendations` state**. Delete the line `const [recommendations, setRecommendations] = useState([]);` since it's no longer used.

- [X] T007 [US1] Refactor `ArticleDetails` to isolate comments fetch using `useResource()` in `src/Web/src/App.jsx`

  **What to do**: In the `ArticleDetails` function (starts at line 535), the article and comments are fetched together inside the `load()` function. A 404 on comments currently breaks the entire article page. Separate comments into `useResource()`.

  **Step 1 — Remove comments from the `load()` function**. Change `load()` so it ONLY fetches the article. Remove the `comments` and `setComments` from the initial load.

  **Current load function** (simplified):
  ```jsx
  const [comments, setComments] = useState([]);

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const nextArticle = await api.request(`/api/articles/${slug}`);
      setArticle(nextArticle);
      const nextArticleId = getField(nextArticle, "id", "");
      if (nextArticleId) {
        const nextComments = await api.request(`/api/articles/${nextArticleId}/comments`);
        setComments(Array.isArray(nextComments) ? nextComments : []);
      }
    } catch (loadError) {
      setError(loadError.message);
    } finally {
      setLoading(false);
    }
  }, [api, slug]);
  ```

  **Replace the `load` function with** (only fetches the article):
  ```jsx
  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      setArticle(await api.request(`/api/articles/${slug}`));
    } catch (loadError) {
      setError(loadError.message);
    } finally {
      setLoading(false);
    }
  }, [api, slug]);
  ```

  **Step 2 — Add `useResource()` for comments**. Add this after the `articleId` line:
  ```jsx
  const commentsResource = useResource(
    api,
    (a) => articleId ? a.request(`/api/articles/${articleId}/comments`) : Promise.resolve([]),
    [api, articleId]
  );
  ```

  **Step 3 — Update comment-related functions**. The `submitComment` and `removeComment` functions currently call `setComments(...)`. Replace those with `commentsResource.reload()`:

  In `submitComment` — replace:
  ```jsx
  setComments(await api.request(`/api/articles/${articleId}/comments`));
  ```
  with:
  ```jsx
  commentsResource.reload();
  ```

  In `removeComment` — replace:
  ```jsx
  setComments(await api.request(`/api/articles/${articleId}/comments`));
  ```
  with:
  ```jsx
  commentsResource.reload();
  ```

  **Step 4 — Update the comments JSX section**. Wrap the comments area with resource state handling:

  Replace the `<CommentTree>` usage:
  ```jsx
  <CommentTree
    comments={comments}
    ...
  />
  ```
  with:
  ```jsx
  {commentsResource.loading ? (
    <div className="loading-state"><Loader2 className="spin" size={22} /> Loading comments</div>
  ) : commentsResource.isEmpty ? (
    <EmptyState icon={MessageSquare} title="No comments" body="Comments are not available for this article." />
  ) : commentsResource.error ? (
    <EmptyState title="Could not load comments" body={commentsResource.error} action={{ label: "Retry", onClick: commentsResource.reload }} />
  ) : (
    <CommentTree
      comments={Array.isArray(commentsResource.data) ? commentsResource.data : []}
      busy={busy}
      onReply={setReplyTo}
      onDelete={removeComment}
      onVote={(commentId, value) => vote(commentId, "Comment", value)}
    />
  )}
  ```

  **Step 5 — Remove the old `comments` state**. Delete the line `const [comments, setComments] = useState([]);`.

**Checkpoint**: At this point, User Story 1 should be fully functional. A 404 on recommendations (Dashboard sidebar) or comments (ArticleDetails) shows a clean empty state. The rest of the page continues to work. Test by visiting `/` and `/articles/some-slug`.

---

## Phase 4: User Story 2 - Critical Page Failure (Priority: P1)

**Goal**: When a user navigates to a page whose critical data cannot be loaded, they see a clear page-level error message (from the `PageErrorBoundary`) or a meaningful error in `AsyncState` — not a broken layout.

**Independent Test**: Navigate to `/articles/nonexistent-slug-12345`. The page should show "Request failed" with the error message. The app does NOT crash. Navigation back to `/` still works.

### Implementation for User Story 2

- [X] T008 [US2] Verify and refine `AsyncState` error display in `src/Web/src/App.jsx`

  **What to do**: The current `AsyncState` component (lines 1229–1242) already handles errors by rendering `<EmptyState title="Request failed" body={error} />`. Enhance it to use the new `icon` prop for a better visual.

  **Current code**:
  ```jsx
  function AsyncState({ loading, error, children }) {
    if (loading) {
      return (
        <div className="loading-state">
          <Loader2 className="spin" size={22} />
          Loading
        </div>
      );
    }
    if (error) {
      return <EmptyState title="Request failed" body={error} />;
    }
    return children;
  }
  ```

  **Replace with**:
  ```jsx
  function AsyncState({ loading, error, children }) {
    if (loading) {
      return (
        <div className="loading-state">
          <Loader2 className="spin" size={22} />
          Loading
        </div>
      );
    }
    if (error) {
      return <EmptyState icon={X} title="Request failed" body={error} />;
    }
    return children;
  }
  ```

  **Note**: `X` is already imported from `lucide-react` at the top of the file.

- [ ] T009 [US2] Verify `PageErrorBoundary` catches uncaught render errors in `src/Web/src/App.jsx`

  **What to do**: This is a verification task. The `PageErrorBoundary` was created in T004 and wired in T005. Verify the following:
  1. Open the app in the browser at `http://localhost:5173`.
  2. Navigate to `/articles/nonexistent-slug`. The `ArticleDetails` component should show the `AsyncState` error state ("Request failed" + error message) — NOT a white screen.
  3. Click the browser back button or navigate to `/`. The app should work normally.
  4. The `PageErrorBoundary` fallback ("Something went wrong" + Reload button) should ONLY appear if there's an uncaught JavaScript exception during rendering — NOT for API errors (which are handled by state).

**Checkpoint**: At this point, critical page failures show meaningful error states. The `PageErrorBoundary` catches any uncaught exceptions. Navigation remains functional after errors.

---

## Phase 5: User Story 3 - Non-404 Errors (Priority: P2)

**Goal**: When a network failure or server error (5xx) occurs, components handle it gracefully — optionally showing a retry button — without crashing the page.

**Independent Test**: Stop the Community API backend service. Navigate to `/`. The articles feed should show an error state with the error message. The page layout should remain intact (header, sidebar still render).

### Implementation for User Story 3

- [X] T010 [US3] Refactor `Recommendations` page to use `useResource()` in `src/Web/src/App.jsx`

  **What to do**: The standalone `Recommendations` page component (starts at line 487) currently uses manual try/catch. Refactor it to use `useResource()`.

  **Current code** (simplified):
  ```jsx
  function Recommendations({ api }) {
    const [limit, setLimit] = useState(10);
    const [articles, setArticles] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    const load = useCallback(async () => {
      setLoading(true);
      setError("");
      try {
        const result = await api.request(`/api/recommendations?limit=${limit}`);
        setArticles(Array.isArray(result) ? result : []);
      } catch (loadError) {
        setError(loadError.message);
      } finally {
        setLoading(false);
      }
    }, [api, limit]);

    useEffect(() => {
      load();
    }, [load]);

    return (
      <main className="single-column-page">
        <PageHeading ... />
        <AsyncState loading={loading} error={error}>
          <ArticleList articles={articles} />
        </AsyncState>
      </main>
    );
  }
  ```

  **Replace the entire function with**:
  ```jsx
  function Recommendations({ api }) {
    const [limit, setLimit] = useState(10);
    const resource = useResource(
      api,
      (a) => a.request(`/api/recommendations?limit=${limit}`),
      [api, limit]
    );

    return (
      <main className="single-column-page">
        <PageHeading
          eyebrow="Personalized"
          title="Recommendations"
          action={
            <label className="small-control">
              Limit
              <input
                min="1"
                max="50"
                type="number"
                value={limit}
                onChange={(event) => setLimit(event.target.value)}
              />
            </label>
          }
        />
        {resource.loading ? (
          <div className="loading-state"><Loader2 className="spin" size={22} /> Loading</div>
        ) : resource.isEmpty ? (
          <EmptyState icon={Sparkles} title="No recommendations" body="We don't have personalized picks for you yet." />
        ) : resource.error ? (
          <EmptyState icon={X} title="Could not load recommendations" body={resource.error} action={{ label: "Retry", onClick: resource.reload }} />
        ) : (
          <ArticleList articles={Array.isArray(resource.data) ? resource.data : []} />
        )}
      </main>
    );
  }
  ```

  **Key change**: A 404 now shows a friendly empty state. A 5xx or network error shows an error state with a "Retry" button.

**Checkpoint**: All user stories should now be independently functional. 404s show empty states, 5xx/network errors show error states with retry, and the `PageErrorBoundary` catches any uncaught exceptions.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final cleanup.

- [X] T011 Remove any unused `useState` or `useCallback` imports/variables from refactored components in `src/Web/src/App.jsx`

  **What to do**: After all the refactoring above, some components may have unused state variables. Scan for:
  - `Dashboard`: Remove `const [recommendations, setRecommendations] = useState([]);` if still present.
  - `ArticleDetails`: Remove `const [comments, setComments] = useState([]);` if still present.
  - `Recommendations`: The entire function was replaced, but verify no orphaned variables remain.
  - Check that no unused imports remain at the top of the file.

- [ ] T012 Run quickstart.md validation in `specs/001-frontend-404-handling/quickstart.md`

  **What to do**: Follow the test scenarios in `quickstart.md` to validate the implementation:
  1. Start the app with `cd src/Web && npm run dev`.
  2. **Test 1**: Go to `http://localhost:5173/`. Verify the Dashboard loads. If recommendations API returns 404, the sidebar shows a clean empty state with a Sparkles icon.
  3. **Test 2**: Go to `http://localhost:5173/articles/nonexistent-slug`. Verify the page shows a meaningful error state (not a blank page or crash).
  4. **Test 3**: Stop the backend API. Navigate to any page. Verify components show error states but the page layout (header, navigation) stays intact.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No-op — project already initialized.
- **Foundational (Phase 2)**: No external dependencies — can start immediately. Tasks T001–T005 MUST be done in order (all modify `App.jsx`).
- **User Story 1 (Phase 3)**: Depends on Phase 2 completion.
- **User Story 2 (Phase 4)**: Depends on Phase 2 completion. Can run in parallel with US1, but since both modify `App.jsx`, do them sequentially.
- **User Story 3 (Phase 5)**: Depends on Phase 2 completion. Can run after US1 and US2.
- **Polish (Phase 6)**: Depends on all user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: Depends on Phase 2 (T001–T005). No dependencies on other stories.
- **User Story 2 (P1)**: Depends on Phase 2 (T004–T005 specifically). No dependencies on US1.
- **User Story 3 (P2)**: Depends on Phase 2 (T001–T002 specifically). No dependencies on US1 or US2.

### Within Each User Story

- All tasks within a user story MUST be done sequentially (they all modify `App.jsx`).

### Parallel Opportunities

- **T003** (CSS changes) can be done in parallel with T001, T002, T004 (JSX changes) since they are in different files.
- Beyond that, parallelism is limited because all components live in `src/Web/src/App.jsx`.

---

## Parallel Example: Phase 2

```text
# These two can run at the same time (different files):
Agent A: T001 + T002 + T004 + T005 (App.jsx changes)
Agent B: T003 (App.css changes)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 2: Foundational (T001–T005)
2. Complete Phase 3: User Story 1 (T006–T007)
3. **STOP and VALIDATE**: Test that 404s on recommendations and comments show empty states while the rest of the page works.
4. This alone delivers the core value of the feature.

### Incremental Delivery

1. Complete Phase 2 → Foundation ready
2. Add User Story 1 (T006–T007) → Test → MVP delivered!
3. Add User Story 2 (T008–T009) → Test → Critical failures handled gracefully
4. Add User Story 3 (T010) → Test → Server/network errors handled with retry
5. Polish (T011–T012) → Cleanup and validation

---

## Notes

- All tasks modify ONE of two files: `App.jsx` or `App.css`. There are no new source files.
- The `useApi()` hook is NOT modified — it remains the low-level HTTP abstraction.
- The new `useResource()` hook WRAPS `useApi().request` — it is a higher-level abstraction.
- `PageErrorBoundary` is the ONLY class component — this is required by React for Error Boundaries.
- `ArticleCollection` (my-articles, bookmarks) is intentionally NOT refactored to use `useResource()` — a 404 on those endpoints is a genuine error, not an expected empty state.
- `ArticleEditor` is intentionally NOT refactored — a 404 when editing an article means the article doesn't exist, which is a real error.
- `EndpointWorkbench` already handles errors inline and does NOT need changes.
