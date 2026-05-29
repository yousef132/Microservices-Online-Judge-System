# Quickstart: Community Hub UI Redesign

**Feature**: `002-ui-redesign-api-mocking` | **Date**: 2026-05-29

## Prerequisites

- Node.js 20+
- Docker (optional — only needed to run real backend services)

## 1. Install Dependencies

```bash
cd src/Web
npm install
```

New packages installed: `@tailwindcss/vite` (devDep), `msw@2` (devDep).

## 2. Run Frontend with MSW Mocks (No Backend Required)

```bash
cd src/Web
npm run dev
```

The Vite dev server starts at `http://localhost:5173`.

MSW intercepts all unimplemented API calls automatically in development mode. You will see `[MSW] Mocking enabled` in the browser console.

## 3. Test MSW Edge-Case Scenarios

All MSW handlers support a `?msw_scenario=<scenario>` query parameter:

| Scenario | Description |
|---|---|
| *(default)* | Returns realistic paginated data |
| `?msw_scenario=empty` | Returns an empty result set |
| `?msw_scenario=error` | Returns HTTP 500 |
| `?msw_scenario=not-found` | Returns HTTP 404 |

**Example**: To test the empty notifications state, navigate to `/notifications?msw_scenario=empty`.

## 4. Test with Real Backend (Optional)

Start all backend services:

```bash
# From repository root
docker-compose up
```

Then start the frontend:

```bash
cd src/Web
npm run dev
```

Real endpoints (articles, comments, votes, bookmarks, auth) bypass MSW automatically. Mock handlers only fire for routes that have no real backend match.

## 5. Route Map

| Route | Description | Auth |
|---|---|---|
| `/welcome` | Landing page | Guest |
| `/auth` | Login / Register | Guest |
| `/` | Community feed | Guest (read), Auth (interact) |
| `/articles/:slug` | Article detail | Guest (read), Auth (interact) |
| `/articles/new` | Create article | Auth |
| `/articles/:slug/edit` | Edit article | Auth (owner) |
| `/me` | My articles | Auth |
| `/bookmarks` | Saved articles | Auth |
| `/explore` | Discover content | Guest |
| `/tags` | Tags explorer | Guest |
| `/search` | Search results | Guest |
| `/notifications` | Notification center | Auth |
| `/communities` | Followed communities | Auth |
| `/communities/discover` | Recommended communities | Auth |
| `/feeds/custom` | Custom feed builder | Auth |
| `/settings/profile` | Profile settings | Auth |
| `/settings/account` | Account settings | Auth |
| `/profile/:username/analytics` | User analytics | Auth (owner) |
| `/analytics/detailed` | Platform analytics | Admin |
| `/admin` | Admin dashboard | Admin |
| `/admin/users` | User management | Admin |
| `/moderation` | Moderation queue | Admin |

## 6. Missing APIs Documentation

All unimplemented endpoints are documented at:

```
src/Web/src/docs/missing-apis.md
```

This file serves as the handoff spec for the backend team to implement the real endpoints.
