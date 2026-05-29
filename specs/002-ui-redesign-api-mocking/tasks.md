# Tasks: Community Hub UI Redesign & Missing API Mocking

**Input**: Design documents from `specs/002-ui-redesign-api-mocking/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/missing-apis-contracts.md, quickstart.md

**Tests**: Not requested — no test tasks included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Install new dependencies, configure Tailwind CSS, initialize MSW, prepare project for redesign.

- [x] T001 Install `@tailwindcss/vite` and `msw@2` as devDependencies in `src/Web/package.json`
- [x] T002 Add `@tailwindcss/vite` plugin to `src/Web/vite.config.js`
- [x] T003 Add `@import "tailwindcss"` entry point to `src/Web/src/index.css` (before existing CSS variable definitions)
- [x] T004 Run `npx msw init public/ --save` to generate `src/Web/public/mockServiceWorker.js`
- [x] T005 [P] Create MSW worker setup file at `src/Web/src/mocks/browser.js`
- [x] T006 [P] Create MSW handlers scaffold at `src/Web/src/mocks/handlers.js` (empty handlers array, import structure)
- [x] T007 Update `src/Web/src/main.jsx` to conditionally start MSW worker when `import.meta.env.DEV` is true

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Create shared hooks and shell-level components that ALL user stories depend on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T008 Implement `useAuth(api)` hook at the top of `src/Web/src/App.jsx` — wraps `api.request` to intercept HTTP 401 responses, clears token, and redirects to `/auth?returnUrl=<current-path>`
- [x] T009 Implement `useNotificationCount(api)` hook in `src/Web/src/App.jsx` — subscribes to `useLocation()`, fetches `GET /api/notifications/unread-count` on every URL change when authenticated
- [x] T010 Implement `usePresignedUpload()` hook in `src/Web/src/App.jsx` — accepts upload URL endpoint and file, fetches presigned URL, PUTs file to storage provider, returns `{ upload, uploading, error, progress }`
- [x] T011 Implement `RequireAuth` wrapper component in `src/Web/src/App.jsx` — redirects unauthenticated users to `/auth?returnUrl=<current-path>`
- [x] T012 Implement `RequireRole` wrapper component in `src/Web/src/App.jsx` — checks JWT role claim (`Admin`, `SuperAdmin`), redirects unauthorized users with "Access denied" message
- [x] T013 Add MSW handler for `GET /api/notifications/unread-count` in `src/Web/src/mocks/handlers.js` (returns `{ count: 5 }` with `delay(500)` and scenario variants)

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 — Public Discovery & Onboarding (Priority: P1) 🎯 MVP

**Goal**: Guest visitors land on an engaging welcome page and can register or log in. Authenticated users see a redesigned shell with navigation, notification badge, and global search trigger.

**Independent Test**: Visit `/welcome` as a guest → verify hero, feature cards, CTAs render. Click "Sign Up" → complete registration → verify redirect to `/`. Verify notification badge in header updates on navigation.

### Implementation for User Story 1

- [x] T014 [US1] Redesign the `Shell` component in `src/Web/src/App.jsx` — apply Terminal Nexus Tailwind classes to the root layout (sidebar nav rail, top bar, content area)
- [x] T015 [US1] Redesign the `Header` component in `src/Web/src/App.jsx` — notification bell with unread count badge (from `useNotificationCount`), global search trigger button, auth-aware display (login link vs avatar + menu)
- [x] T016 [US1] Implement `WelcomePage` component in `src/Web/src/App.jsx` — static landing page at `/welcome` with hero section, feature highlight cards, CTA buttons linking to `/auth`. Reference `stitch_nexus_community_hub/devstack_landing_page/code.html` for layout
- [x] T017 [US1] Implement `AuthPage` component in `src/Web/src/App.jsx` — route `/auth` with tabbed login/register forms, client-side validation, error banners, `?returnUrl` query param handling. Wire to real `POST /users/login` and `POST /users/register` endpoints. Reference `stitch_nexus_community_hub/authentication_login_register/code.html`
- [x] T018 [US1] Implement `SearchOverlay` component in `src/Web/src/App.jsx` — Cmd+K palette-style overlay with backdrop blur, debounced input via `useDebouncedValue`, results grouped by Articles/Tags/Authors. Reference `stitch_nexus_community_hub/global_search_overlay/code.html`
- [x] T019 [US1] Add MSW handler for `GET /api/search/suggestions` in `src/Web/src/mocks/handlers.js` (returns articles, tags, authors arrays with `delay(300)` and empty/error scenario variants)
- [x] T020 [US1] Update `Shell` routing table in `src/Web/src/App.jsx` — register all 16 new routes (`/welcome`, `/auth`, `/explore`, `/tags`, `/search`, `/notifications`, `/communities`, `/communities/discover`, `/feeds/custom`, `/settings/profile`, `/settings/account`, `/profile/:username/analytics`, `/analytics/detailed`, `/admin`, `/admin/users`, `/moderation`) with appropriate `RequireAuth` and `RequireRole` wrappers
- [x] T021 [US1] Add unauthenticated root redirect logic in `src/Web/src/App.jsx` — if not authenticated, redirect `/` to `/welcome`

**Checkpoint**: Landing page, authentication flow, and redesigned shell with search overlay are fully functional.

---

## Phase 4: User Story 2 — Content Feed & Article Interaction (Priority: P1)

**Goal**: Authenticated users browse a redesigned feed, read articles, vote, comment (single-level replies), and bookmark. Guest users can view articles read-only.

**Independent Test**: Log in → view feed at `/` → sort by Newest/Hot/Top → open an article → vote → comment → reply → bookmark → verify all actions reflect immediately.

### Implementation for User Story 2

- [x] T022 [US2] Redesign `Dashboard` component in `src/Web/src/App.jsx` — Tailwind-styled article feed cards, sort controls (Newest/Hot/Top Voted), tag filter pills, sidebar metrics panel. Reference `stitch_nexus_community_hub/community_home_feed/code.html`
- [x] T023 [US2] Redesign `ArticleDetails` component in `src/Web/src/App.jsx` — full article layout with author line, cover image, tags, vote controls with optimistic update + rollback on error, bookmark toggle. Guest read-only mode: render interactions but trigger `/auth` redirect on click when unauthenticated. Single-level reply threads with vertical guide lines. Reference `stitch_nexus_community_hub/post_details_discussions/code.html`
- [x] T024 [US2] Redesign `ArticleEditor` component in `src/Web/src/App.jsx` — Monaco editor in body field, tag input field, cover image upload via presigned URL (`POST /api/articles/{id}/cover-image-upload-url`), publish/draft toggle. Reference `stitch_nexus_community_hub/create_new_post/code.html`
- [x] T025 [US2] Redesign `ArticleCollection` component in `src/Web/src/App.jsx` — tabbed view for My Articles (`/me`) and Bookmarks (`/bookmarks`), empty state with action. Reference `stitch_nexus_community_hub/saved_library/code.html`

**Checkpoint**: Core content loop (feed → read → interact → save) is fully functional with real API integration.

---

## Phase 5: User Story 3 — Content Discovery (Priority: P2)

**Goal**: Users discover content via the explore page, tags explorer, and search results page — all with category-based filtering and empty-state handling.

**Independent Test**: Navigate to `/explore` → verify trending content renders. Visit `/tags` → verify tag grid. Submit a search → visit `/search?q=test` → verify faceted results.

### Implementation for User Story 3

- [x] T026 [P] [US3] Implement `ExplorePage` component in `src/Web/src/App.jsx` — curated trending content grid with trending tags, featured articles, active communities. Reference `stitch_nexus_community_hub/explore_discover/code.html`
- [x] T027 [P] [US3] Implement `TagsExplorerPage` component in `src/Web/src/App.jsx` — tag directory grid with post count, description, follow/unfollow toggles. Reference `stitch_nexus_community_hub/tags_explorer/code.html`
- [x] T028 [P] [US3] Implement `SearchResultsPage` component in `src/Web/src/App.jsx` — faceted results grouped by Articles/Authors/Tags, synced with `?q=` and `?category=` query params, empty state. Reference `stitch_nexus_community_hub/search_results_themed/code.html`
- [x] T029 [P] [US3] Add MSW handler for `GET /api/explore/highlights` in `src/Web/src/mocks/handlers.js`
- [x] T030 [P] [US3] Add MSW handler for `GET /api/tags/summary` in `src/Web/src/mocks/handlers.js`
- [x] T031 [P] [US3] Add MSW handler for `GET /api/search` in `src/Web/src/mocks/handlers.js`

**Checkpoint**: All discovery surfaces render with mocked data, empty states, and error states.

---

## Phase 6: User Story 4 — Personalization & Profile Management (Priority: P2)

**Goal**: Users manage profile settings (including avatar upload via presigned URL), update account security, follow communities, and create custom feeds.

**Independent Test**: Navigate to `/settings/profile` → update display name + upload avatar → verify changes persist. Visit `/communities` → join a community → verify toggle state. Create a custom feed at `/feeds/custom`.

### Implementation for User Story 4

- [x] T032 [P] [US4] Implement `ProfileSettingsPage` component in `src/Web/src/App.jsx` — display name, bio, avatar upload (via `usePresignedUpload` hook + presigned S3 URL flow), live preview, validation errors. Reference `stitch_nexus_community_hub/user_profile_settings_themed_corrected/code.html`
- [x] T033 [P] [US4] Implement `AccountSettingsPage` component in `src/Web/src/App.jsx` — password change form with current/new/confirm fields, inline validation, account deletion with confirmation modal. Reference `stitch_nexus_community_hub/account_settings/code.html`
- [x] T034 [P] [US4] Implement `CommunitiesPage` component in `src/Web/src/App.jsx` — list of followed communities with Join/Leave toggle buttons, community metadata (member count, activity badge). Reference `stitch_nexus_community_hub/followed_communities_final_theme/code.html`
- [x] T035 [P] [US4] Implement `DiscoverCommunitiesPage` component in `src/Web/src/App.jsx` — recommended communities gallery with search, Join button. Reference `stitch_nexus_community_hub/followed_communities_themed_corrected/code.html`
- [x] T036 [P] [US4] Implement `CustomFeedsPage` component in `src/Web/src/App.jsx` — feed builder with tag/community/sort-order criteria checkboxes, list of saved feeds, create new feed form. Reference `stitch_nexus_community_hub/custom_feeds/code.html`
- [x] T037 [P] [US4] Add MSW handlers for profile settings endpoints in `src/Web/src/mocks/handlers.js`: `GET /api/users/profile-settings`, `PUT /api/users/profile-settings`, `POST /api/users/avatar-upload-url`
- [x] T038 [P] [US4] Add MSW handlers for account settings endpoints in `src/Web/src/mocks/handlers.js`: `PUT /api/users/security/password`, `DELETE /api/users/account`
- [x] T039 [P] [US4] Add MSW handlers for communities endpoints in `src/Web/src/mocks/handlers.js`: `GET /api/communities/followed`, `POST /api/communities/:id/follow`, `DELETE /api/communities/:id/follow`, `GET /api/communities/recommended`
- [x] T040 [P] [US4] Add MSW handlers for custom feeds endpoints in `src/Web/src/mocks/handlers.js`: `GET /api/feeds/custom`, `POST /api/feeds/custom`

**Checkpoint**: All personalization, settings, and community screens function with mocked data.

---

## Phase 7: User Story 5 — Notifications (Priority: P2)

**Goal**: Users view a consolidated notification center, mark notifications as read individually or all at once.

**Independent Test**: Navigate to `/notifications` → verify grouped notification list renders → click a notification to mark as read → click "Mark all as read" → verify all states transition.

### Implementation for User Story 5

- [x] T041 [US5] Implement `NotificationsPage` component in `src/Web/src/App.jsx` — notification list grouped by date, mark individual read on click, "Mark all as read" bulk action, unread visual distinction, deep links to target content. Reference `stitch_nexus_community_hub/notification_center/code.html`
- [x] T042 [P] [US5] Add MSW handlers for notification endpoints in `src/Web/src/mocks/handlers.js`: `GET /api/notifications`, `PUT /api/notifications/:id/read`, `POST /api/notifications/read-all`

**Checkpoint**: Notification center is fully interactive with mocked data; unread count badge in header syncs on navigation.

---

## Phase 8: User Story 6 — Analytics & Insights (Priority: P3)

**Goal**: Users view per-profile engagement analytics with time filters. Admins view platform-wide detailed analytics.

**Independent Test**: Visit `/profile/:username/analytics` → verify metrics and chart render for 30d default → toggle to 7d → verify update. Admin: visit `/analytics/detailed` → verify platform-wide stats.

### Implementation for User Story 6

- [x] T043 [P] [US6] Implement `UserAnalyticsPage` component in `src/Web/src/App.jsx` — engagement metrics cards (views, votes, comments), 7d/30d period filter toggle, custom SVG line chart for daily views, top tags grid. Reference `stitch_nexus_community_hub/user_profile_analytics/code.html`
- [x] T044 [P] [US6] Implement `DetailedAnalyticsPage` component in `src/Web/src/App.jsx` — platform-wide growth stats, demographic grid, activity patterns, CSV export trigger (mocked). Reference `stitch_nexus_community_hub/detailed_analytics/code.html`
- [x] T045 [P] [US6] Add MSW handler for `GET /api/users/:username/analytics/summary` in `src/Web/src/mocks/handlers.js`
- [x] T046 [P] [US6] Add MSW handler for `GET /api/analytics/community-detailed` in `src/Web/src/mocks/handlers.js`

**Checkpoint**: Both user-level and admin-level analytics dashboards render with mocked data.

---

## Phase 9: User Story 7 — Admin & Moderation Controls (Priority: P3)

**Goal**: Admin/SuperAdmin users see platform health dashboard, manage users, and process the moderation queue. Non-admin users are denied access.

**Independent Test**: Admin: visit `/admin` → verify metrics dashboard → visit `/admin/users` → search + change role → visit `/moderation` → approve/hide flagged item. Non-admin: navigate to `/admin` → verify "Access denied" redirect.

### Implementation for User Story 7

- [x] T047 [P] [US7] Implement `AdminDashboardPage` component in `src/Web/src/App.jsx` — platform health metric cards (active users, posts, flagged content, new users), status indicators. Reference `stitch_nexus_community_hub/admin_dashboard/code.html`
- [x] T048 [P] [US7] Implement `UserManagementPage` component in `src/Web/src/App.jsx` — searchable user table with columns (username, email, role, status, join date), role change dropdown, status change (Activate/Ban/Lock) actions. Reference `stitch_nexus_community_hub/user_management/code.html`
- [x] T049 [P] [US7] Implement `ModerationQueuePage` component in `src/Web/src/App.jsx` — flagged content list with content excerpt, reporter info, action buttons (Approve/Hide/Warn), confirmation dialogs. Reference `stitch_nexus_community_hub/moderation_queue/code.html`
- [x] T050 [P] [US7] Add MSW handler for `GET /api/admin/metrics` in `src/Web/src/mocks/handlers.js`
- [x] T051 [P] [US7] Add MSW handlers for user management endpoints in `src/Web/src/mocks/handlers.js`: `GET /api/admin/users`, `PUT /api/admin/users/:id/role`, `PUT /api/admin/users/:id/status`
- [x] T052 [P] [US7] Add MSW handlers for moderation endpoints in `src/Web/src/mocks/handlers.js`: `GET /api/moderation/queue`, `POST /api/moderation/resolve`

**Checkpoint**: All admin and moderation interfaces are fully interactive with mocked data; non-admin users are properly denied.

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Finalize all MSW handlers, generate missing API documentation, responsive validation, and overall polish.

- [ ] T053 Audit all MSW handlers in `src/Web/src/mocks/handlers.js` — ensure every handler includes `delay(500)`, and supports `?msw_scenario=empty`, `?msw_scenario=error`, and `?msw_scenario=not-found` query flags
- [ ] T054 [P] Generate `src/Web/src/docs/missing-apis.md` — compile all missing endpoint contracts from `specs/002-ui-redesign-api-mocking/contracts/missing-apis-contracts.md` into the application source tree
- [ ] T055 [P] Verify responsive rendering of all 21 screens from 375px to 1440px viewport widths — fix any horizontal scroll or content clipping issues
- [ ] T056 [P] Verify all loading states render for ≥100ms on every data-fetching interaction
- [ ] T057 [P] Verify all empty states display actionable messaging when data sets are empty
- [ ] T058 [P] Verify all error states display user-readable messages (no raw error objects or blank screens)
- [ ] T059 Verify token expiry flow — simulate 401 response → confirm redirect to `/auth?returnUrl=...` → re-authenticate → confirm return to original route
- [ ] T060 Verify guest read-only access at `/articles/:slug` — confirm interactions trigger auth flow
- [ ] T061 Verify admin route protection — non-admin navigates to `/admin`, `/admin/users`, `/moderation` → confirm "Access denied" redirect
- [ ] T062 Verify notification count badge updates on every URL change (path, query, hash)
- [ ] T063 [P] Code cleanup — remove unused CSS classes in `src/Web/src/App.css`, remove deprecated components (e.g., old `EndpointWorkbench` if replaced)
- [ ] T064 [P] Update `src/Web/src/App.css` — ensure existing non-Tailwind global styles (error boundary fallback, scrollbar overrides) are preserved and do not conflict with Tailwind classes
- [ ] T065 Run quickstart.md validation — verify `npm run dev` starts cleanly, MSW logs to console, all routes are reachable
- [ ] T066 Final visual audit — compare each screen against its corresponding `stitch_nexus_community_hub/*/code.html` reference for layout fidelity

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3–9)**: All depend on Foundational phase completion
  - US1 (Phase 3) and US2 (Phase 4) are both P1 and should be done first, sequentially (US1 → US2 since US2's article detail relies on the redesigned shell from US1)
  - US3 (Phase 5), US4 (Phase 6), US5 (Phase 7) are all P2 — can proceed in parallel after US1 + US2
  - US6 (Phase 8) and US7 (Phase 9) are both P3 — can proceed in parallel after P2 stories
- **Polish (Phase 10)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1 — Onboarding)**: Can start after Foundational (Phase 2). No dependencies on other stories. Establishes the shell, auth, and routing that all other stories rely on.
- **US2 (P1 — Feed & Articles)**: Depends on US1 (needs redesigned shell, routing table, and auth flow). Can run after US1 completion.
- **US3 (P2 — Discovery)**: Depends on US1 (needs shell and search overlay infrastructure). Independent of US2.
- **US4 (P2 — Personalization)**: Depends on US1 (needs shell and auth). Independent of US2 and US3.
- **US5 (P2 — Notifications)**: Depends on US1 (needs notification count hook in header). Independent of US2, US3, US4.
- **US6 (P3 — Analytics)**: Depends on US1 (needs shell and role gating). Independent of other stories.
- **US7 (P3 — Admin/Moderation)**: Depends on US1 (needs shell and `RequireRole` wrapper). Independent of other stories.

### Within Each User Story

- MSW handlers can be created in parallel with UI components ([P] marked)
- Component implementation follows a natural order: layout → data binding → interaction handlers → error/empty states

### Parallel Opportunities

- All Setup tasks (T001–T007): T005 and T006 can run in parallel
- All Foundational tasks (T008–T013): T011 and T012 can run in parallel
- After US1 completes: US3, US4, US5 can all start in parallel (different pages, different MSW handlers)
- After US2 completes: US6, US7 can start (independent of P2 stories)
- Within each US: component tasks marked [P] can be implemented simultaneously
- All Phase 10 verification tasks marked [P] can run in parallel

---

## Parallel Example: User Story 3 (Discovery)

```text
# These tasks can all launch in parallel (different files, no dependencies):
T026: Implement ExplorePage component in src/Web/src/App.jsx
T027: Implement TagsExplorerPage component in src/Web/src/App.jsx
T028: Implement SearchResultsPage component in src/Web/src/App.jsx
T029: Add MSW handler for GET /api/explore/highlights
T030: Add MSW handler for GET /api/tags/summary
T031: Add MSW handler for GET /api/search
```

> Note: While T026–T028 are all in the same file (`App.jsx`), they are non-overlapping component definitions at distinct line ranges, so they can be written in parallel by different agents as long as they don't conflict at the `Shell` routing level (which is handled in T020).

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T007)
2. Complete Phase 2: Foundational (T008–T013)
3. Complete Phase 3: User Story 1 — Onboarding (T014–T021)
4. **STOP and VALIDATE**: Landing page + auth + shell + search overlay work end-to-end
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Infrastructure ready
2. US1 (Onboarding) → Test independently → Demo (MVP!)
3. US2 (Feed & Articles) → Test independently → Demo (core loop complete)
4. US3 + US4 + US5 (Discovery, Personalization, Notifications) → Test independently → Demo
5. US6 + US7 (Analytics, Admin) → Test independently → Demo
6. Polish → Final validation → Release

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Developer A: US1 (Onboarding) → then US2 (Feed)
3. Once US1 is done:
   - Developer B: US3 (Discovery)
   - Developer C: US4 (Personalization)
   - Developer D: US5 (Notifications)
4. Once US2 is done:
   - Developer E: US6 (Analytics)
   - Developer F: US7 (Admin)
5. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files or non-overlapping sections, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- All component references point to `stitch_nexus_community_hub/*/code.html` for visual fidelity
- All MSW handlers must include `delay(500)` and scenario variants (empty, error, not-found)
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
