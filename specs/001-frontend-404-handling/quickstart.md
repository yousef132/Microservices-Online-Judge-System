# Quickstart: Frontend 404 Handling

## Prerequisites

- Node.js installed
- Docker Compose running (for backend services)
- Project cloned and dependencies installed (`npm install` in `src/Web`)

## Development

```bash
# Start backend services
docker compose up -d

# Start frontend dev server
cd src/Web
npm run dev
```

The frontend runs at `http://localhost:5173`.

## Testing the Feature

### Test 1: Component-level 404 handling
1. Navigate to `http://localhost:5173/`
2. The Dashboard loads with an articles feed and a recommendations sidebar
3. If the recommendations API returns a 404, the sidebar shows a clean empty state instead of breaking the page
4. The articles feed continues to work normally

### Test 2: Page-level error boundary
1. Navigate to `http://localhost:5173/articles/nonexistent-slug`
2. If the article does not exist (404), the page shows a clear "Article not found" empty state
3. Navigation back to the feed still works (page is not crashed)

### Test 3: Non-404 errors
1. Stop the Community API service
2. Navigate to any page
3. Components show error states but the page structure remains intact
4. The page-level error boundary only triggers for uncaught JavaScript exceptions, not API errors

## Key Files

| File | Purpose |
|---|---|
| `src/Web/src/App.jsx` | All components, hooks, and routing |
| `src/Web/src/App.css` | All component styles |
| `src/Web/src/index.css` | CSS variables and resets |
