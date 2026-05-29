# Missing API Contracts: Community Hub

**Feature**: `002-ui-redesign-api-mocking` | **Date**: 2026-05-29

All endpoints listed here are **not yet implemented** in the backend. They are mocked by MSW during development and must be implemented by the backend team before production launch.

---

## 1. Global Search Suggestions

**Endpoint**: `GET /api/search/suggestions`
**Auth Required**: No
**Used by**: `SearchOverlay` component (triggered on debounced input)

**Request**:
```
GET /api/search/suggestions?q={query}
Headers: (none required)
```

**Response 200**:
```json
{
  "articles": [
    { "slug": "string", "title": "string", "tags": ["string"] }
  ],
  "tags": [
    { "name": "string", "postCount": 0 }
  ],
  "authors": [
    { "username": "string", "displayName": "string", "avatarUrl": "string|null" }
  ]
}
```

**Response 200 (empty)**:
```json
{ "articles": [], "tags": [], "authors": [] }
```

---

## 2. Search Results

**Endpoint**: `GET /api/search`
**Auth Required**: No
**Used by**: `SearchResultsPage` component

**Request**:
```
GET /api/search?q={query}&category={articles|authors|tags}&page={1}&pageSize={20}
Headers: (none required)
```

**Response 200**:
```json
{
  "query": "string",
  "category": "articles|authors|tags",
  "items": [...],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

---

## 3. Explore Highlights

**Endpoint**: `GET /api/explore/highlights`
**Auth Required**: No
**Used by**: `ExplorePage` component

**Response 200**:
```json
{
  "trendingTags": [{ "name": "string", "postCount": 0 }],
  "featuredArticles": [{ "slug": "string", "title": "string", "authorName": "string", "voteCount": 0 }],
  "activeCommunities": [{ "slug": "string", "name": "string", "memberCount": 0, "activityBadge": "hot|active|quiet|null" }]
}
```

---

## 4. Tags Summary

**Endpoint**: `GET /api/tags/summary`
**Auth Required**: No
**Used by**: `TagsExplorerPage` component

**Response 200**:
```json
{
  "items": [
    {
      "id": "uuid",
      "name": "string",
      "description": "string",
      "postCount": 0,
      "isFollowed": false
    }
  ],
  "totalCount": 0
}
```

---

## 5. Notifications (List)

**Endpoint**: `GET /api/notifications`
**Auth Required**: Yes
**Used by**: `NotificationsPage` component

**Request**:
```
GET /api/notifications?page={1}&pageSize={20}
Authorization: Bearer {token}
```

**Response 200**:
```json
{
  "items": [
    {
      "id": "uuid",
      "type": "vote|comment|reply|mention",
      "actorName": "string",
      "actorAvatar": "string|null",
      "targetTitle": "string",
      "targetUrl": "string",
      "isRead": false,
      "createdAt": "ISO 8601"
    }
  ],
  "unreadCount": 0,
  "totalCount": 0,
  "page": 1,
  "pageSize": 20
}
```

---

## 6. Notification Unread Count

**Endpoint**: `GET /api/notifications/unread-count`
**Auth Required**: Yes
**Used by**: `useNotificationCount` hook (fires on every route change)

**Response 200**:
```json
{ "count": 0 }
```

---

## 7. Mark Notification as Read

**Endpoint**: `PUT /api/notifications/:id/read`
**Auth Required**: Yes
**Used by**: `NotificationsPage` on notification click

**Response 200**:
```json
{ "id": "uuid", "isRead": true }
```

**Response 404**:
```json
{ "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4", "title": "Not Found", "status": 404 }
```

---

## 8. Mark All Notifications as Read

**Endpoint**: `POST /api/notifications/read-all`
**Auth Required**: Yes
**Used by**: `NotificationsPage` "Mark all as read" button

**Response 200**:
```json
{ "markedCount": 0 }
```

---

## 9. Followed Communities

**Endpoint**: `GET /api/communities/followed`
**Auth Required**: Yes
**Used by**: `CommunitiesPage` component

**Response 200**:
```json
{
  "items": [
    {
      "id": "uuid",
      "name": "string",
      "slug": "string",
      "description": "string",
      "memberCount": 0,
      "activityBadge": "hot|active|quiet|null",
      "avatarUrl": "string|null",
      "isFollowed": true
    }
  ]
}
```

---

## 10. Follow / Unfollow Community

**Endpoint**: `POST /api/communities/:id/follow` | `DELETE /api/communities/:id/follow`
**Auth Required**: Yes
**Used by**: `CommunitiesPage`, `DiscoverCommunitiesPage` toggle button

**POST Response 201**:
```json
{ "communityId": "uuid", "isFollowed": true }
```

**DELETE Response 200**:
```json
{ "communityId": "uuid", "isFollowed": false }
```

---

## 11. Recommended Communities

**Endpoint**: `GET /api/communities/recommended`
**Auth Required**: Yes
**Used by**: `DiscoverCommunitiesPage` component

**Response 200**: Same shape as Followed Communities but `isFollowed: false` for all items.

---

## 12. Custom Feeds

**Endpoint**: `GET /api/feeds/custom` | `POST /api/feeds/custom`
**Auth Required**: Yes
**Used by**: `CustomFeedsPage` component

**GET Response 200**:
```json
{
  "items": [
    {
      "id": "uuid",
      "name": "string",
      "tags": ["string"],
      "communityIds": ["uuid"],
      "sortOrder": "new|hot|top",
      "createdAt": "ISO 8601"
    }
  ]
}
```

**POST Request Body**:
```json
{
  "name": "string",
  "tags": ["string"],
  "communityIds": ["uuid"],
  "sortOrder": "new|hot|top"
}
```

**POST Response 201**: Returns the created feed object.

**POST Response 400**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Name": ["The Name field is required."] }
}
```

---

## 13. Profile Settings

**Endpoint**: `GET /api/users/profile-settings` | `PUT /api/users/profile-settings`
**Auth Required**: Yes
**Used by**: `ProfileSettingsPage` component

**GET Response 200**:
```json
{
  "id": "uuid",
  "username": "string",
  "displayName": "string",
  "bio": "string",
  "avatarUrl": "string|null"
}
```

**PUT Request Body**:
```json
{
  "displayName": "string",
  "bio": "string",
  "avatarStorageKey": "string|null"
}
```

**PUT Response 200**: Returns the updated profile object.

---

## 14. Avatar Presigned Upload URL

**Endpoint**: `POST /api/users/avatar-upload-url`
**Auth Required**: Yes
**Used by**: `usePresignedUpload` hook inside `ProfileSettingsPage`

**Request Body**:
```json
{ "contentType": "image/jpeg|image/png|image/webp" }
```

**Response 200**:
```json
{
  "uploadUrl": "https://storage.example.com/presigned-url-with-params",
  "storageKey": "avatars/{userId}/{uuid}.jpg",
  "expiresAt": "ISO 8601"
}
```

The frontend PUTs the file directly to `uploadUrl`, then sends `storageKey` to `PUT /api/users/profile-settings`.

---

## 15. Account Security — Password Change

**Endpoint**: `PUT /api/users/security/password`
**Auth Required**: Yes
**Used by**: `AccountSettingsPage` component

**Request Body**:
```json
{
  "currentPassword": "string",
  "newPassword": "string",
  "confirmNewPassword": "string"
}
```

**Response 200**:
```json
{ "message": "Password updated successfully." }
```

**Response 400** (validation failure):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "ConfirmNewPassword": ["Passwords do not match."] }
}
```

---

## 16. User Analytics Summary

**Endpoint**: `GET /api/users/:username/analytics/summary`
**Auth Required**: Yes (owner or admin)
**Used by**: `UserAnalyticsPage` component

**Request**:
```
GET /api/users/{username}/analytics/summary?period=7d|30d
Authorization: Bearer {token}
```

**Response 200**:
```json
{
  "username": "string",
  "period": "7d|30d",
  "totalViews": 0,
  "votesReceived": 0,
  "commentCount": 0,
  "topTags": [{ "name": "string", "postCount": 0 }],
  "dailyViews": [{ "date": "ISO 8601 date", "count": 0 }]
}
```

---

## 17. Admin — Platform Metrics

**Endpoint**: `GET /api/admin/metrics`
**Auth Required**: Yes (Admin/SuperAdmin)
**Used by**: `AdminDashboardPage` component

**Response 200**:
```json
{
  "activeUserCount": 0,
  "totalPostCount": 0,
  "flaggedContentCount": 0,
  "newUserCount": 0,
  "snapshotAt": "ISO 8601"
}
```

---

## 17.5. Admin — Detailed Analytics

**Endpoint**: `GET /api/analytics/community-detailed`
**Auth Required**: Yes (Admin/SuperAdmin)
**Used by**: `DetailedAnalyticsPage` component

**Response 200**:
```json
{
  "activeUserCount": 0,
  "totalPostCount": 0,
  "flaggedContentCount": 0,
  "newUserCount": 0
}
```

---

## 18. Admin — User List

**Endpoint**: `GET /api/admin/users`
**Auth Required**: Yes (Admin/SuperAdmin)
**Used by**: `UserManagementPage` component

**Request**:
```
GET /api/admin/users?q={search}&page={1}&pageSize={20}
Authorization: Bearer {token}
```

**Response 200**:
```json
{
  "items": [
    {
      "id": "uuid",
      "username": "string",
      "displayName": "string",
      "email": "string",
      "role": "User|ProblemSetter|Admin|SuperAdmin",
      "status": "Active|Banned|Locked",
      "joinedAt": "ISO 8601"
    }
  ],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20
}
```

---

## 19. Admin — Change User Role

**Endpoint**: `PUT /api/admin/users/:id/role`
**Auth Required**: Yes (Admin/SuperAdmin)

**Request Body**:
```json
{ "role": "User|ProblemSetter|Admin|SuperAdmin" }
```

**Response 200**:
```json
{ "userId": "uuid", "role": "string" }
```

---

## 20. Admin — Change User Status

**Endpoint**: `PUT /api/admin/users/:id/status`
**Auth Required**: Yes (Admin/SuperAdmin)

**Request Body**:
```json
{ "status": "Active|Banned|Locked" }
```

**Response 200**:
```json
{ "userId": "uuid", "status": "string" }
```

---

## 21. Moderation Queue

**Endpoint**: `GET /api/moderation/queue`
**Auth Required**: Yes (Admin/SuperAdmin)
**Used by**: `ModerationQueuePage` component

**Response 200**:
```json
{
  "items": [
    {
      "id": "uuid",
      "contentType": "article|comment",
      "contentId": "uuid",
      "contentExcerpt": "string",
      "reporterName": "string",
      "reason": "string",
      "reportedAt": "ISO 8601"
    }
  ],
  "totalCount": 0
}
```

---

## 22. Moderation — Resolve Report

**Endpoint**: `POST /api/moderation/resolve`
**Auth Required**: Yes (Admin/SuperAdmin)
**Used by**: `ModerationQueuePage` action buttons

**Request Body**:
```json
{
  "reportId": "uuid",
  "action": "approve|hide|warn",
  "moderatorNote": "string|null"
}
```

**Response 200**:
```json
{ "reportId": "uuid", "action": "approve|hide|warn", "resolvedAt": "ISO 8601" }
```
