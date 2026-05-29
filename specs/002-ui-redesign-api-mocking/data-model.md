# Data Model: Community Hub UI Redesign

**Feature**: `002-ui-redesign-api-mocking` | **Date**: 2026-05-29

All entities are defined as they appear in the **frontend state** (React hook return values and MSW response shapes). Field types follow JSON conventions.

---

## Article

| Field | Type | Notes |
|---|---|---|
| `id` | `string (UUID)` | Unique identifier |
| `slug` | `string` | URL-safe identifier; used as route param |
| `title` | `string` | Required; max 255 chars |
| `body` | `string` | Markdown; required |
| `tags` | `string[]` | Array of lowercase tag strings |
| `status` | `"Draft" \| "Published"` | Default: `"Draft"` |
| `authorId` | `string (UUID)` | FK to User |
| `authorName` | `string` | Denormalised display name |
| `authorAvatar` | `string \| null` | URL to avatar image |
| `coverImageUrl` | `string \| null` | Full URL to cover image |
| `voteCount` | `number` | Signed integer (up - down) |
| `userVote` | `1 \| -1 \| 0 \| null` | Authenticated user's current vote; `null` if unauthenticated |
| `commentCount` | `number` | Total comment count |
| `viewCount` | `number` | Incremented server-side on detail page load |
| `isBookmarked` | `boolean` | Authenticated user's bookmark state; `false` if unauthenticated |
| `createdAt` | `string (ISO 8601)` | |
| `publishedAt` | `string (ISO 8601) \| null` | Null for drafts |

**State Transitions**: `Draft → Published` (on publish action); `Published → Draft` (on unpublish — not exposed in UI v1).

**Validation Rules (client-side)**:
- `title`: required, 1–255 chars
- `body`: required, non-empty
- `tags`: optional; each tag lowercase alphanumeric + hyphens

---

## Comment

| Field | Type | Notes |
|---|---|---|
| `id` | `string (UUID)` | Unique identifier |
| `articleId` | `string (UUID)` | FK to Article |
| `parentId` | `string (UUID) \| null` | `null` = top-level; non-null = direct reply |
| `body` | `string` | Required; markdown |
| `authorId` | `string (UUID)` | |
| `authorName` | `string` | |
| `authorAvatar` | `string \| null` | |
| `voteCount` | `number` | Signed integer |
| `userVote` | `1 \| -1 \| 0 \| null` | |
| `isDeleted` | `boolean` | Soft-deleted comments show placeholder text |
| `createdAt` | `string (ISO 8601)` | |

**Nesting Constraint**: `parentId` references only top-level comment IDs. A comment with a non-null `parentId` cannot itself have children (single-level threading, per clarification Q1).

---

## Tag

| Field | Type | Notes |
|---|---|---|
| `id` | `string (UUID)` | |
| `name` | `string` | Lowercase; unique |
| `description` | `string` | Short tagline |
| `postCount` | `number` | Total published articles using this tag |
| `isFollowed` | `boolean` | Authenticated user's follow state |

---

## Notification

| Field | Type | Notes |
|---|---|---|
| `id` | `string (UUID)` | |
| `type` | `"vote" \| "comment" \| "reply" \| "mention"` | |
| `actorName` | `string` | User who triggered the event |
| `actorAvatar` | `string \| null` | |
| `targetTitle` | `string` | Article title or comment excerpt |
| `targetUrl` | `string` | Deep link to the resource (e.g., `/articles/:slug#comment-id`) |
| `isRead` | `boolean` | |
| `createdAt` | `string (ISO 8601)` | |

**State Transition**: `isRead: false → isRead: true` (on click or "mark all as read" action).

---

## Community

| Field | Type | Notes |
|---|---|---|
| `id` | `string (UUID)` | |
| `name` | `string` | Display name |
| `slug` | `string` | URL-safe identifier |
| `description` | `string` | Short tagline |
| `memberCount` | `number` | |
| `activityBadge` | `"hot" \| "active" \| "quiet" \| null` | |
| `avatarUrl` | `string \| null` | |
| `isFollowed` | `boolean` | Authenticated user's follow state |

---

## CustomFeed

| Field | Type | Notes |
|---|---|---|
| `id` | `string (UUID)` | |
| `name` | `string` | User-defined label |
| `tags` | `string[]` | Filter: include articles with any of these tags |
| `communityIds` | `string[]` | Filter: include articles from these communities |
| `sortOrder` | `"new" \| "hot" \| "top"` | |
| `createdAt` | `string (ISO 8601)` | |

---

## UserProfile

| Field | Type | Notes |
|---|---|---|
| `id` | `string (UUID)` | |
| `username` | `string` | Immutable; used in route params |
| `displayName` | `string` | Editable; max 80 chars |
| `bio` | `string` | Editable; max 500 chars |
| `avatarUrl` | `string \| null` | Stored in S3-compatible storage |
| `avatarStorageKey` | `string \| null` | Key returned by presigned upload; sent to backend on profile save |
| `reputation` | `number` | Read-only; computed server-side |
| `postCount` | `number` | Read-only |
| `followerCount` | `number` | Read-only |
| `joinedAt` | `string (ISO 8601)` | Read-only |
| `role` | `"User" \| "ProblemSetter" \| "Admin" \| "SuperAdmin"` | |

**Validation Rules (client-side)**:
- `displayName`: required, 1–80 chars
- `bio`: optional, max 500 chars

---

## AdminMetrics

| Field | Type | Notes |
|---|---|---|
| `activeUserCount` | `number` | Users active in last 30 days |
| `totalPostCount` | `number` | |
| `flaggedContentCount` | `number` | Items in moderation queue |
| `newUserCount` | `number` | New registrations in last 7 days |
| `snapshotAt` | `string (ISO 8601)` | When metrics were last computed |

---

## UnreadNotificationCount (hook return shape)

| Field | Type | Notes |
|---|---|---|
| `count` | `number` | 0 when no unread notifications |
| `loading` | `boolean` | |

---

## PresignedUpload (hook return shape)

| Field | Type | Notes |
|---|---|---|
| `upload(file: File): Promise<string>` | `function` | Returns the storage key on success |
| `uploading` | `boolean` | |
| `progress` | `number (0–100)` | |
| `error` | `string \| ""` | |
