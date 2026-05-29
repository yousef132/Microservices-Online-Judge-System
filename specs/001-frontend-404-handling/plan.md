

# plan.md (For `specs/002-stitch-ui-redesign/plan.md`)

```markdown
# Implementation Plan: 22-Phase Stitch UI/UX Redesign & Missing API Strategy

**Branch**: `002-stitch-ui-redesign` | **Date**: 2026-05-29 | **Spec**: [spec.md](file:///d:/Microservices-Online-Judge-System/specs/002-stitch-ui-redesign/spec.md)

## Summary

The frontend system is being redesigned page-by-page using high-fidelity exports from Stitch (21 screens featuring dynamic Tailwind classes mapped to the `Terminal Nexus` style guide). This plan specifies a 22-phase execution structure, ensuring every screen is built as a self-contained feature unit. 

Where the ASP.NET Core backend API exists, we wire up real integrations. Where the backend is missing, we fully implement the react state machine (hooks, contracts, and loaders) and mock the response using MSW v2 while documenting the exact JSON schemas.


---

## Implementation Phases

### Phase 1: Project Setup, Tailwind Integration & MSW Scaffold
* **Goal**: Establish the tooling environment for high-fidelity rendering and client-side API mocking.
* **Tasks**:
  1. Install Tailwind CSS v4 (or `@tailwindcss/vite` plugin) and `msw` (v2) in `src/Web/package.json`.
  2. Map design variables from `D:\Microservices-Online-Judge-System\stitch_nexus_community_hub\terminal_nexus\DESIGN.md` (colors, borders, typography) to the Tailwind theme config.
  3. Run `npx msw init public/ --save`.
  4. Create `src/mocks/browser.js` and `src/mocks/handlers.js` and register the worker conditionally in `src/main.jsx`.

---

### Phase 2: Screen 1 - DevStack Landing Page
* **Input File**: `stitch_nexus_community_hub/devstack_landing_page/code.html`
* **Route Mapping**: `/welcome` (unauthenticated fallback route)
* **API Status**: **None** (purely informational/static landing page).
* **Tasks**:
  1. Build a high-fidelity welcome landing page for guest visitors.
  2. Implement responsive navigations, call-to-actions, and interactive feature cards.

---

### Phase 3: Screen 2 - Authentication (Login & Register)
* **Input File**: `stitch_nexus_community_hub/authentication_login_register/code.html`
* **Route Mapping**: Modal trigger / `/auth`
* **API Status**: **Exists** (calls Gateway `/users/login` and `/users/register`).
* **Tasks**:
  1. Redesign login and registration tabs in accordance with the glassmorphic aesthetic.
  2. Implement client-side validation, error banners, and toggle transitions between login/register.
  3. Wire to the real identity gateway, storing the token in localStorage (`communityToken`).

---

### Phase 4: Screen 3 - Global Search Overlay
* **Input File**: `stitch_nexus_community_hub/global_search_overlay/code.html`
* **Route Mapping**: Global component (mounted inside `App.jsx` Header)
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/search/suggestions?q=...` -> Returns array of matched articles, tags, and users.
* **Tasks**:
  1. Implement the search overlay container (Cmd+K palette style) with backdrop blur.
  2. Write search debouncing hooks (`useDebouncedValue`).
  3. Build loading skeletal structures for search transitions.

---

### Phase 5: Screen 4 - Community Home Feed
* **Input File**: `stitch_nexus_community_hub/community_home_feed/code.html`
* **Route Mapping**: `/` (Main Feed)
* **API Status**: **Exists** (calls `/api/articles` with pagination and sorting).
* **Tasks**:
  1. Redesign feed dashboard card items, sidebar metrics, active filters, and tag lists.
  2. Connect to the existing community service feed APIs.
  3. Implement pull-to-refresh skeleton indicators.

---

### Phase 6: Screen 5 - Post Details & Discussions
* **Input File**: `stitch_nexus_community_hub/post_details_discussions/code.html`
* **Route Mapping**: `/articles/:slug`
* **API Status**: **Exists** (calls `/api/articles/{slug}` and `/api/articles/{articleId}/comments`).
* **Tasks**:
  1. Redesign full post layouts, author details, inline action items, and nested comments.
  2. Bind threaded comment indicators (vertical guidelines) and active hover highlight states.
  3. Integrate voting mechanisms (`POST /api/votes`) and bookmarking triggers.

---

### Phase 7: Screen 6 - Create New Post / Editor
* **Input File**: `stitch_nexus_community_hub/create_new_post/code.html`
* **Route Mapping**: `/articles/new` & `/articles/:slug/edit`
* **API Status**: **Exists** (calls `/api/articles`).
* **Tasks**:
  1. Redesign the post creation panel (form input fields, inline image uploads, and tag-input fields).
  2. Standardize focus indicators (primary blue accent + glow).
  3. Integrate Monaco Editor inside the body field for seamless code block formatting.

---

### Phase 8: Screen 7 - Saved Library & Bookmarks
* **Input File**: `stitch_nexus_community_hub/saved_library/code.html`
* **Route Mapping**: `/bookmarks` and `/me`
* **API Status**: **Exists** (calls `/api/bookmarks` and `/api/articles/me`).
* **Tasks**:
  1. Re-render the library hub using a tabbed system (e.g., Saved, Drafts, History).
  2. Implement item removing buttons and empty collection indicators.

---

### Phase 9: Screen 8 - Explore & Discover
* **Input File**: `stitch_nexus_community_hub/explore_discover/code.html`
* **Route Mapping**: `/explore`
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/explore/highlights` -> Returns curated trending tags, hot articles, and active communities.
* **Tasks**:
  1. Build a beautiful grid-based feed displaying top tags and featured spaces.
  2. Program client-side filtering matching the search query params.

---

### Phase 10: Screen 9 - Followed Communities (Final Theme)
* **Input File**: `stitch_nexus_community_hub/followed_communities_final_theme/code.html`
* **Route Mapping**: `/communities`
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/communities/followed` -> Returns list of followed community metadata (reputation, count, badge).
* **Tasks**:
  1. Code the list-based communities screen showcasing custom badges and active users.
  2. Map individual community rows with toggle buttons ("Joined"/"Join").

---

### Phase 11: Screen 10 - Followed Communities (Corrected Theme)
* **Input File**: `stitch_nexus_community_hub/followed_communities_themed_corrected/code.html`
* **Route Mapping**: `/communities/discover`
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/communities/recommended` -> Returns list of spaces the user is not following yet.
* **Tasks**:
  1. Build the recommendation card gallery layout.
  2. Embed search capabilities inside the space listings.

---

### Phase 12: Screen 11 - Tags Explorer
* **Input File**: `stitch_nexus_community_hub/tags_explorer/code.html`
* **Route Mapping**: `/tags`
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/tags/summary` -> Returns list of tag objects with popularity counts, descriptions, and user tracking.
* **Tasks**:
  1. Map out the high-information density tag directory grid.
  2. Add tag subscription triggers with active hover highlights.

---

### Phase 13: Screen 12 - Search Results
* **Input File**: `stitch_nexus_community_hub/search_results_themed/code.html`
* **Route Mapping**: `/search`
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/search?q=...&category=...` -> Returns matched posts, users, or tags.
* **Tasks**:
  1. Implement the search result layout featuring faceted categorization (Articles, Authors, Spaces).
  2. Connect the query string parser hook to synchronize results with the URL state.

---

### Phase 14: Screen 13 - Notification Center
* **Input File**: `stitch_nexus_community_hub/notification_center/code.html`
* **Route Mapping**: `/notifications`
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/notifications` -> Returns list of notifications (comments, mentions, votes).
  - `PUT /api/notifications/:id/read` -> Marks single notification as read.
  - `POST /api/notifications/read-all` -> Marks all as read.
* **Tasks**:
  1. Lay out the notification stack grouped by category and dates.
  2. Connect quick-action mark-as-read triggers and direct links.

---

### Phase 15: Screen 14 - User Profile Settings
* **Input File**: `stitch_nexus_community_hub/user_profile_settings_themed_corrected/code.html`
* **Route Mapping**: `/settings/profile`
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/users/profile-settings` -> Returns user display profiles.
  - `PUT /api/users/profile-settings` -> Updates display name, bio, and custom avatar assets.
* **Tasks**:
  1. Implement high-fidelity avatar crop drawers and text fields with live-updating previews.
  2. Map visual validation error indicators.

---

### Phase 16: Screen 15 - Account Settings
* **Input File**: `stitch_nexus_community_hub/account_settings/code.html`
* **Route Mapping**: `/settings/account`
* **API Status**: **Missing**
* **Mocks Required**:
  - `PUT /api/users/security/password` -> Updates user password blocks.
  - `DELETE /api/users/account` -> Terminate user account flow.
* **Tasks**:
  1. Wire input security rules and password validation prompts.
  2. Implement verification warning banners for account deletion triggers.

---

### Phase 17: Screen 16 - User Profile Analytics
* **Input File**: `stitch_nexus_community_hub/user_profile_analytics/code.html`
* **Route Mapping**: `/profile/:username/analytics`
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/users/:username/analytics/summary` -> Returns engagement score, graph datasets, top tags.
* **Tasks**:
  1. Build charts and key performance grids using dynamic SVG layouts (avoiding external graphing packages).
  2. Implement filter triggers for analytical metrics (7 days vs 30 days).

---

### Phase 18: Screen 17 - Detailed Analytics
* **Input File**: `stitch_nexus_community_hub/detailed_analytics/code.html`
* **Route Mapping**: `/analytics/detailed`
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/analytics/community-detailed` -> Returns growth factors, demographic stats, activity patterns.
* **Tasks**:
  1. Code dense analytical summaries featuring comparative percentages and detailed data grids.
  2. Implement data exporting buttons (CSV mock triggers).

---

### Phase 19: Screen 18 - Custom Feeds
* **Input File**: `stitch_nexus_community_hub/custom_feeds/code.html`
* **Route Mapping**: `/feeds/custom`
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/feeds/custom` -> Returns list of customized filter criteria feeds.
  - `POST /api/feeds/custom` -> Creates a custom tailored feed rule set.
* **Tasks**:
  1. Construct criteria checklists (tags, spaces, author filters).
  2. Bind active feed card components utilizing the dynamically filtered rulesets.

---

### Phase 20: Screen 19 - Admin Dashboard
* **Input File**: `stitch_nexus_community_hub/admin_dashboard/code.html`
* **Route Mapping**: `/admin`
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/admin/metrics` -> Returns platform aggregates (active users, posts, flags).
* **Tasks**:
  1. Implement high-level system logs displays, status lights, and metric card arrays.
  2. Restrict route access to authorized accounts (`Admin`/`SuperAdmin`).

---

### Phase 21: Screen 20 - User Management
* **Input File**: `stitch_nexus_community_hub/user_management/code.html`
* **Route Mapping**: `/admin/users`
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/admin/users` -> Returns lists of system users with roles and metrics.
  - `PUT /api/admin/users/:id/role` -> Updates roles for users.
  - `PUT /api/admin/users/:id/status` -> Bans, locks, or activates user accounts.
* **Tasks**:
  1. Render a detailed user tabular ledger with multi-action action indicators.
  2. Connect search search query fields to refine directory tables.

---

### Phase 22: Screen 21 - Moderation Queue
* **Input File**: `stitch_nexus_community_hub/moderation_queue/code.html`
* **Route Mapping**: `/moderation`
* **API Status**: **Missing**
* **Mocks Required**:
  - `GET /api/moderation/queue` -> Returns list of reported materials.
  - `POST /api/moderation/resolve` -> Submits moderation decisions.
* **Tasks**:
  1. Construct flag details drawers showing reported comment strings and reporter metadata.
  2. Implement quick processing triggers (Approve, Hide, Warn user).

---

### Phase 23: Global Verification & Missing APIs Contract Synthesis
* **Goal**: Perform comprehensive visual audits and compile complete implementation documentation for missing APIs.
* **Tasks**:
  1. Run comprehensive browser checks under multiple viewports to verify glassmorphic responses.
  2. Ensure MSW handlers map completely to all simulated pages.
  3. Generate `src/Web/src/docs/missing-apis.md` compiling all endpoint specifications.


