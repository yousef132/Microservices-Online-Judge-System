# Feature Specification: Community Hub UI Redesign & Missing API Mocking

**Feature Branch**: `002-ui-redesign-api-mocking`

**Created**: 2026-05-29

**Status**: Draft

**Input**: Full frontend redesign of the JudgeSync Community Hub across 21 screens using a high-fidelity design system, combined with a strategy for implementing and mocking backend APIs that are not yet available, and compiling documentation for all missing API contracts.

---

## Clarifications

### Session 2026-05-29

- Q: What is the maximum comment nesting depth supported? → A: Single level only — a reply cannot itself be replied to.
- Q: When an authenticated user's token expires mid-session, what should happen? → A: Redirect to `/auth?returnUrl=<current-path>` so the user is returned to their original page after re-authentication.
- Q: How should the frontend receive new notifications during an active user session? → A: The unread notification count is fetched exclusively when the URL changes (path, query parameter, or hash), acting as a navigation-driven poll.
- Q: Can an unauthenticated (guest) user view an article detail page at `/articles/:slug`? → A: Yes, read-only. Visitors can read the article and comments, but interactions (voting, commenting) redirect to `/auth`.
- Q: How are users expected to update their profile avatar? → A: Full file upload. The frontend retrieves an S3-compatible presigned URL from the backend and uploads the file directly to the storage provider.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Public Discovery & Onboarding (Priority: P1)

A guest visitor arrives at the platform for the first time. They land on a welcoming, visually rich landing page that communicates the platform's value, showcases features, and provides a clear call-to-action to sign up or log in. The visitor can navigate to explore public content before deciding to authenticate.

**Why this priority**: This is the first impression of the platform and directly affects user acquisition. A polished, informative landing page and smooth authentication flow are the highest-leverage surfaces for converting visitors into members.

**Independent Test**: Can be fully tested by visiting `/welcome` as a guest, verifying the landing page renders with all sections and CTAs, and completing the registration or login flow through `/auth`.

**Acceptance Scenarios**:

1. **Given** a guest user visits the platform root, **When** they are not authenticated, **Then** they are presented with the landing page featuring the platform's value proposition, feature highlights, and sign-up/login actions.
2. **Given** a guest user on the landing page clicks "Sign Up", **When** the authentication modal or page opens, **Then** they can toggle between login and registration tabs, fill in their credentials, and receive clear inline validation messages for any errors.
3. **Given** a user submits valid registration credentials, **When** the system processes the request, **Then** the user is authenticated, their token is stored, and they are redirected to the main feed.
4. **Given** a user submits invalid credentials during login, **When** the system rejects the request, **Then** a clear, non-dismissible error banner is shown and the form fields are not cleared.

---

### User Story 2 - Content Feed & Article Interaction (Priority: P1)

An authenticated member browses the main community feed, reads articles, votes on posts and comments, leaves comments (including threaded replies), and bookmarks content for later reading. All interactions feel immediate and provide clear visual feedback.

**Why this priority**: This is the core value loop of the platform. Users who cannot fluidly interact with content will churn. The feed and article detail views are the highest-traffic surfaces.

**Independent Test**: Can be fully tested by logging in, viewing the home feed at `/`, opening an article at `/articles/:slug`, casting votes, posting a comment, replying to a comment, and bookmarking the article — verifying each action reflects immediately in the UI.

**Acceptance Scenarios**:

1. **Given** an authenticated user on the home feed, **When** the page loads, **Then** a paginated list of articles is displayed with author, publication date, vote count, comment count, and tag metadata visible for each article.
2. **Given** a user on the home feed, **When** they apply a sort filter (Newest, Hot, Top Voted), **Then** the article list reorders without a full page reload.
3. **Given** a user opens an article detail page, **When** the article loads, **Then** the full body, cover image (if available), tags, author line, and threaded comments are rendered.
4. **Given** an authenticated user on an article detail page clicks the upvote button, **When** the vote is cast, **Then** the vote count updates immediately and the button reflects the user's active vote state.
5. **Given** a user submits a comment, **When** the comment is posted, **Then** it appears at the top of the comment thread without a full page reload.
6. **Given** a user clicks "Reply" on an existing comment, **When** they submit their reply, **Then** it appears nested under the parent comment.
7. **Given** a user clicks "Bookmark" on an article, **When** the action completes, **Then** the bookmark icon toggles state and the article appears in their `/bookmarks` list.

---

### User Story 3 - Content Discovery (Priority: P2)

A user wants to find relevant content beyond their main feed. They can use a command-palette-style global search, browse by tags, explore a curated discovery feed, and view search results with category-based filtering.

**Why this priority**: Discovery is the key driver of depth of engagement. Users who can only see their main feed will not explore the full breadth of the platform's content.

**Independent Test**: Can be fully tested by opening the global search (keyboard shortcut or header button), typing a query and seeing suggestions appear, navigating to `/tags` to browse all tags, and visiting `/explore` to see curated highlights.

**Acceptance Scenarios**:

1. **Given** a user triggers the global search overlay, **When** they type a query, **Then** a debounced list of suggested articles, tags, and authors appears within 300ms of the last keystroke.
2. **Given** a user on the Tags Explorer page at `/tags`, **When** the page loads, **Then** a grid of all available tags is displayed with post count and description for each.
3. **Given** a user on the Explore page at `/explore`, **When** the page loads, **Then** curated trending tags, featured articles, and active community highlights are presented.
4. **Given** a user submits a search query, **When** results load at `/search`, **Then** results are grouped by category (Articles, Authors, Tags) with a faceted filter to narrow results.
5. **Given** a search returns no results, **When** the empty state is displayed, **Then** a friendly message and suggestions for alternative queries are shown.

---

### User Story 4 - Personalization & Profile Management (Priority: P2)

A user can manage their public profile, update account security settings, view their authored articles, manage custom feed configurations, and follow communities of interest.

**Why this priority**: Personalization drives retention. Users who can tailor their experience are more likely to return regularly.

**Independent Test**: Can be fully tested by navigating to `/settings/profile`, updating display name and bio, visiting `/settings/account` to change password, and going to `/communities` to follow or unfollow a community.

**Acceptance Scenarios**:

1. **Given** a user navigates to `/settings/profile`, **When** the page loads, **Then** their current display name, bio, and avatar are pre-filled in the form.
2. **Given** a user updates their profile and saves, **When** the update succeeds, **Then** a success confirmation is shown and the new values are reflected immediately.
3. **Given** a user on `/settings/account` submits a password change with a non-matching confirmation, **When** they attempt to save, **Then** an inline validation error appears and the form is not submitted.
4. **Given** a user on `/communities`, **When** they click "Join" on a community, **Then** the button toggles to "Joined" and the community appears in their followed communities list.
5. **Given** a user on `/me`, **When** the page loads, **Then** only the articles authored by the authenticated user are displayed.

---

### User Story 5 - Notifications (Priority: P2)

A user can view a consolidated notification center showing all activity relevant to them (votes on their content, comments on their articles, mentions), and mark notifications as read individually or all at once.

**Why this priority**: Notifications close the social feedback loop, driving users back to the platform to engage with responses to their content.

**Independent Test**: Can be fully tested by navigating to `/notifications`, verifying the notification list renders grouped by type and date, clicking a notification to see it marked as read, and using the "Mark all as read" action.

**Acceptance Scenarios**:

1. **Given** a user navigates to `/notifications`, **When** the page loads, **Then** all unread and read notifications are displayed, grouped by date.
2. **Given** a user clicks a single notification, **When** the action completes, **Then** the notification is marked as read and visually distinguished from unread items.
3. **Given** a user clicks "Mark all as read", **When** the action completes, **Then** all notifications in the list transition to a read state.

---

### User Story 6 - Analytics & Insights (Priority: P3)

A user can view engagement analytics for their own profile (post views, votes received, comment activity over time), and an admin user can view detailed platform-wide analytics including growth metrics and activity patterns.

**Why this priority**: Analytics provide value to power users and administrators, but are not essential for the core community experience. They are planned after the primary interactive surfaces are complete.

**Independent Test**: Can be fully tested by visiting `/profile/:username/analytics` as the profile owner and verifying engagement graphs and key metrics render. Admin analytics at `/analytics/detailed` can be tested by an admin-role user.

**Acceptance Scenarios**:

1. **Given** a user visits their own analytics page, **When** the page loads, **Then** engagement metrics (total views, votes received, comment count) are displayed for the last 30 days.
2. **Given** a user toggles the time period filter from 30 days to 7 days, **When** the filter is applied, **Then** all displayed metrics update to reflect the selected period.
3. **Given** an admin user navigates to `/analytics/detailed`, **When** the page loads, **Then** platform-wide growth statistics, active user counts, and content volume metrics are displayed.

---

### User Story 7 - Admin & Moderation Controls (Priority: P3)

Admin and SuperAdmin users can access an administrative dashboard showing platform health metrics, manage user accounts (view, change roles, ban/activate), and process a moderation queue of reported content.

**Why this priority**: Admin tooling is essential for platform health but affects a very small user segment (administrators only). It is built after the core community experience is established.

**Independent Test**: Can be fully tested by an Admin-role user navigating to `/admin`, verifying the platform metrics dashboard, going to `/admin/users` to search and manage users, and visiting `/moderation` to review and act on flagged content.

**Acceptance Scenarios**:

1. **Given** an Admin user navigates to `/admin`, **When** the page loads, **Then** key platform health metrics (active users, post volume, flagged content count) are displayed.
2. **Given** an Admin user on `/admin/users` searches for a username, **When** results are returned, **Then** the user list filters to matching records showing role, status, and join date.
3. **Given** an Admin user changes a user's role from "User" to "ProblemSetter", **When** the action completes, **Then** the updated role is reflected immediately in the user list.
4. **Given** a moderator views the moderation queue at `/moderation`, **When** a flagged item is displayed, **Then** the reported content, reporter details, and action buttons (Approve, Hide, Warn) are all visible.
5. **Given** a moderator selects "Hide" on a flagged item, **When** the action completes, **Then** the item is removed from the queue and a confirmation is shown.
6. **Given** a non-admin user attempts to navigate to `/admin`, **When** the route is accessed, **Then** they are redirected away with an "Access denied" message.

---

### Edge Cases

- What happens when the backend returns an error mid-interaction (e.g., a vote fails after optimistic update)? The UI must revert the optimistic state and display an error banner.
- What happens when a user's authentication token expires during a session? The system must detect the expired token on the next API call, clear the stored token, and redirect the user to `/auth?returnUrl=<current-path>`. After successful re-authentication the user is returned to the page they were on.
- What happens when a paginated list returns an empty result set on a page beyond the first? An empty state must be shown with a navigation option back to the first page.
- What happens when the global search overlay is opened on a slow connection? A loading skeleton must display and the overlay must not block the underlying page interaction.
- What happens when an admin action (ban, role change) is applied to an account that no longer exists? The system must display a clear, actionable error rather than a generic failure.
- What happens when a notification is clicked that links to deleted content? The user must see a "content unavailable" state rather than a broken page.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a public landing page accessible to unauthenticated users at a dedicated route.
- **FR-002**: The system MUST provide login and registration flows with client-side form validation and server-error display.
- **FR-003**: The system MUST display a paginated article feed with sort options (Newest, Hot, Top Voted) and tag-based filtering.
- **FR-004**: The system MUST display full article content with threaded comments, vote controls, and bookmark actions on the article detail page.
- **FR-005**: The system MUST allow authenticated users to submit top-level comments and single-level replies (one reply depth only — a reply cannot itself be replied to) on any article.
- **FR-006**: The system MUST allow authenticated users to upvote or downvote both articles and comments.
- **FR-007**: The system MUST allow authenticated users to bookmark and unbookmark articles, with the saved list accessible at a dedicated route.
- **FR-008**: The system MUST provide a global search overlay with debounced autocomplete suggestions for articles, tags, and users.
- **FR-009**: The system MUST provide a search results page that categorizes results by type and supports faceted filtering.
- **FR-010**: The system MUST provide a tags explorer page listing all available tags with metadata (post count, description).
- **FR-011**: The system MUST provide an explore/discover page showcasing curated trending content.
- **FR-012**: The system MUST allow authenticated users to follow and unfollow communities, with a dedicated communities page showing followed and recommended spaces.
- **FR-013**: The system MUST provide a notification center where users can view, and mark as read (individually or all at once), activity notifications.
- **FR-014**: The system MUST provide profile and account settings pages where users can update their display name, bio, and password.
- **FR-015**: The system MUST provide a custom feeds builder allowing users to define and save personalized feed filter criteria.
- **FR-016**: The system MUST display per-user engagement analytics accessible to the profile owner.
- **FR-017**: The system MUST restrict admin and moderation routes to users holding Admin or SuperAdmin roles.
- **FR-018**: The system MUST provide an admin dashboard displaying platform-level health and activity metrics.
- **FR-019**: The system MUST provide a user management screen where admins can search users, change roles, and change account status.
- **FR-020**: The system MUST provide a moderation queue where moderators can review reported content and apply resolution actions.
- **FR-021**: All backend endpoints that do not yet exist MUST be fully simulated with realistic mock data during development, including loading, empty, and error states.
- **FR-022**: All simulated endpoints MUST be documented with their full request/response contract in a central API documentation file.
- **FR-023**: The system MUST display appropriate loading indicators for every data-fetching interaction.
- **FR-024**: The system MUST display meaningful empty states (with actionable context) whenever a data set is empty.
- **FR-025**: The system MUST display non-blocking inline error states when non-critical data fetches fail, and blocking error states only when page-critical data is unavailable.
- **FR-026**: The system MUST detect an expired or invalid authentication token on any API call, clear the stored token, and redirect the user to `/auth?returnUrl=<current-path>`; after successful re-authentication the user MUST be returned to their original route.
- **FR-027**: The system MUST fetch the latest unread notification count whenever the application's URL changes (path, query, or hash changes) rather than using a fixed timer or real-time push connection.
- **FR-028**: The system MUST allow unauthenticated guest users to view article detail pages (including comments) in read-only mode; attempting any interaction (voting, commenting, bookmarking) MUST trigger the authentication flow.
- **FR-029**: The system MUST support avatar image uploads by first requesting an S3-compatible presigned URL from the backend, then uploading the image file directly to the storage provider using that URL.

### Key Entities

- **Article**: A community post with title, body, tags, author, status (Draft/Published), vote count, comment count, view count, and optional cover image.
- **Comment**: A user response to an article. Supports exactly one level of nesting: a top-level comment may have direct replies, but replies cannot themselves be replied to. Includes vote count and soft-delete capability.
- **Tag**: A categorical label with name, description, and associated post count.
- **Notification**: A system event directed at a user (vote received, comment posted, mention), with read/unread state.
- **Community**: A topic-based group that users can follow, with metadata (member count, description, activity badge).
- **CustomFeed**: A user-defined set of filter rules (tags, communities, sort order) that generates a personalized article feed.
- **UserProfile**: Public-facing profile data including display name, bio, avatar, and reputation/engagement metrics.
- **AdminMetrics**: Platform-aggregate statistics including active user counts, content volume, and moderation queue depth.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 21 UI screens load and render content correctly, with no blank or broken states under normal conditions.
- **SC-002**: Every page-level interaction (vote, comment, bookmark, follow) completes and the UI reflects the new state within 1 second on a standard broadband connection.
- **SC-003**: Global search suggestions appear within 300ms of the user stopping typing on a standard broadband connection.
- **SC-004**: All loading states are visible for at least 100ms and never leave the user staring at an empty container without feedback.
- **SC-005**: Every route that requires authentication redirects unauthenticated users to the login flow rather than showing a broken page.
- **SC-006**: Every route that requires an Admin or SuperAdmin role redirects unauthorized users with a clear access-denied message.
- **SC-007**: All unimplemented backend endpoints have fully functional mock responses that enable end-to-end manual testing of the complete UI without a running backend.
- **SC-008**: All missing API endpoints are documented in the central API contract file with request and response schemas before the feature is considered complete.
- **SC-009**: Every error scenario (network failure, 404, 500, validation error) surfaces a user-readable message rather than a raw error or blank screen.
- **SC-010**: The application is usable on viewport widths from 375px (mobile) to 1440px (desktop) without horizontal scrolling or content clipping.

---

## Assumptions

- The existing `users.api` backend endpoints (`/users/login`, `/users/register`) are available and will be wired with real API calls. Authentication token storage follows the current `localStorage` key convention (`communityToken`).
- The existing Community API endpoints for articles, comments, votes, and bookmarks are stable and match the current API contracts already used by the frontend.
- The design system's color palette, typography, spacing, and elevation model are defined and will be applied consistently across all screens.
- Tailwind CSS will be used for implementing component styles derived from the design system, as authorized by the project constitution for new UI components.
- MSW v2 will be used exclusively in development mode; production builds will not include the mock service worker.
- The admin and moderation interfaces are gated at the route level using the existing JWT role claims (`Admin`, `SuperAdmin`).
- Custom feeds, communities, notifications, analytics, profile settings, and account settings are features with no current backend implementation; these will be fully mocked during development.
- Mobile-first responsiveness is required for all screens; desktop layout is the primary design target, with mobile adaptation required.
- No new external library dependencies beyond Tailwind CSS and MSW v2 will be introduced.
