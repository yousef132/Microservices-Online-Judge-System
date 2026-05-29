import { http, HttpResponse, delay } from 'msw'

export const handlers = [
  http.get('/api/notifications/unread-count', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });
    if (scenario === 'empty') return HttpResponse.json({ count: 0 });

    

    return HttpResponse.json({ count: 5 })
  }),

  http.get('/api/search/suggestions', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });
    const q = url.searchParams.get('q');
    if (scenario === 'empty') return HttpResponse.json({ articles: [], tags: [], authors: [] });

    

    return HttpResponse.json({
      articles: [
        { id: 1, title: `Result for ${q}`, slug: 'result-1' },
        { id: 2, title: 'Best practices for state management', slug: 'best-practices' }
      ],
      tags: [
        { id: 1, name: 'SystemDesign', postCount: 120 },
        { id: 2, name: 'React19', postCount: 45 }
      ],
      authors: [
        { id: 1, username: 'alexlin_dev', displayName: 'Alex Lin' }
      ]
    })
  }),

  http.get('/api/explore/highlights', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });
    if (scenario === 'empty') return HttpResponse.json({ trendingTags: [], featuredArticles: [], activeCommunities: [] });

        

    return HttpResponse.json({
      trendingTags: [
        { name: 'rust-lang', postCount: 1205 },
        { name: 'webassembly', postCount: 840 },
        { name: 'system-design', postCount: 650 }
      ],
      featuredArticles: [
        { slug: 'migrating-spa-to-rsc', title: 'Migrating from SPA to React Server Components', authorName: 'alex_dev', voteCount: 342 },
        { slug: 'rust-concurrency-patterns', title: 'Advanced Concurrency Patterns in Rust', authorName: 'sarah.js', voteCount: 289 }
      ],
      activeCommunities: [
        { slug: 'rustacean-station', name: 'Rustacean Station', memberCount: 12400, activityBadge: 'hot' },
        { slug: 'frontend-wizards', name: 'Frontend Wizards', memberCount: 45200, activityBadge: 'active' }
      ]
    })
  }),

  http.get('/api/tags/summary', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });
    if (scenario === 'empty') return HttpResponse.json({ items: [], totalCount: 0 });

        

    return HttpResponse.json({
      items: [
        { id: '1', name: 'react', description: 'A JavaScript library for building user interfaces.', postCount: 142000, isFollowed: false },
        { id: '2', name: 'rust', description: 'A language empowering everyone to build reliable and efficient software.', postCount: 52000, isFollowed: true },
        { id: '3', name: 'machine-learning', description: 'Algorithms and statistical models that systems use to perform specific tasks.', postCount: 110000, isFollowed: false },
        { id: '4', name: 'system-design', description: 'Defining architecture, components, modules, interfaces, and data for a system.', postCount: 89000, isFollowed: false }
      ],
      totalCount: 4
    })
  }),

  http.get('/api/search', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });
    const q = url.searchParams.get('q') || '';
    const category = url.searchParams.get('category') || 'articles';
    if (scenario === 'empty') return HttpResponse.json({ query: q, category, items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 });

        

    let items = []
    if (category === 'articles') {
      items = [
        { slug: 'migrating-spa-to-rsc', title: `Migrating to RSC for ${q}`, tags: ['react', 'nextjs', 'performance'] },
        { slug: 'understanding-data-fetching-in-rsc', title: `Data Fetching in ${q}`, tags: ['react', 'tutorial', 'data-fetching'] }
      ]
    } else if (category === 'tags') {
      items = [
        { name: q.toLowerCase().replace(/\s+/g, '-'), postCount: 342 },
        { name: `${q.toLowerCase().replace(/\s+/g, '-')}-advanced`, postCount: 89 }
      ]
    } else if (category === 'authors') {
      items = [
        { username: `${q.toLowerCase().replace(/\s+/g, '_')}_dev`, displayName: `${q} Developer` },
        { username: `sarah_${q.toLowerCase().replace(/\s+/g, '_')}`, displayName: `Sarah ${q}` }
      ]
    }

    return HttpResponse.json({
      query: q,
      category,
      items,
      totalCount: 248,
      page: 1,
      pageSize: 20,
      totalPages: 13
    })
  }),

  // Phase 6
  http.get('/api/users/profile-settings', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({
      id: "u-123",
      username: "alex_dev",
      displayName: "Alex Developer",
      bio: "Full-stack engineer building scalable systems. Love open source and dark mode.",
      avatarUrl: "https://lh3.googleusercontent.com/aida-public/AB6AXuAhq9rTlDFODYBaGIZprxNWUFz1qkjvgK74OH8m5TsathziN827eJ7Nrs4i7uST5eez4xGKHTqUiCMNS1Opg6RWhTmI9BC6S5Ej3DcAXoGOKwfqsjrTCPcVoP-dlUoIJAVNTcH0v82DNTWtEXs5KRi_b3B6C3kjfVmz6zuFow7_VsNZTibgtVceKvpVsPtHWnbPHoQzHsInpo8ovIG6Oo6yy3ddYA5bMvm3zGYszr6ZcSCq87nQ1XyOWqmNP72mo0V090aeqnTpJo8"
    })
  }),
  http.put('/api/users/profile-settings', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        const body = await request.json()
    return HttpResponse.json({
      id: "u-123",
      username: "alex_dev",
      displayName: body.displayName,
      bio: body.bio,
      avatarUrl: body.avatarStorageKey || "https://lh3.googleusercontent.com/aida-public/AB6AXuAhq9rTlDFODYBaGIZprxNWUFz1qkjvgK74OH8m5TsathziN827eJ7Nrs4i7uST5eez4xGKHTqUiCMNS1Opg6RWhTmI9BC6S5Ej3DcAXoGOKwfqsjrTCPcVoP-dlUoIJAVNTcH0v82DNTWtEXs5KRi_b3B6C3kjfVmz6zuFow7_VsNZTibgtVceKvpVsPtHWnbPHoQzHsInpo8ovIG6Oo6yy3ddYA5bMvm3zGYszr6ZcSCq87nQ1XyOWqmNP72mo0V090aeqnTpJo8"
    })
  }),
  http.post('/api/users/avatar-upload-url', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({
      uploadUrl: "https://httpbin.org/put",
      storageKey: "https://lh3.googleusercontent.com/aida-public/AB6AXuAhq9rTlDFODYBaGIZprxNWUFz1qkjvgK74OH8m5TsathziN827eJ7Nrs4i7uST5eez4xGKHTqUiCMNS1Opg6RWhTmI9BC6S5Ej3DcAXoGOKwfqsjrTCPcVoP-dlUoIJAVNTcH0v82DNTWtEXs5KRi_b3B6C3kjfVmz6zuFow7_VsNZTibgtVceKvpVsPtHWnbPHoQzHsInpo8ovIG6Oo6yy3ddYA5bMvm3zGYszr6ZcSCq87nQ1XyOWqmNP72mo0V090aeqnTpJo8",
      expiresAt: new Date(Date.now() + 3600000).toISOString()
    })
  }),
  http.put('/api/users/security/password', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({ message: "Password updated successfully." })
  }),
  http.delete('/api/users/account', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({ message: "Account deleted." })
  }),
  http.get('/api/communities/followed', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({
      items: [
        { id: "c-1", name: "Rustaceans", slug: "rustaceans", description: "Safe, concurrent, and fast systems programming discussions. Share crates and patterns.", memberCount: 142000, activityBadge: "1.2k Online", avatarUrl: null, isFollowed: true },
        { id: "c-2", name: "React Advanced", slug: "react-advanced", description: "Deep dives into React internals, performance optimization, and complex state management.", memberCount: 385000, activityBadge: "4.5k Online", avatarUrl: null, isFollowed: true }
      ]
    })
  }),
  http.get('/api/communities/recommended', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({
      items: [
        { id: "c-3", name: "DevOps Mastery", slug: "devops-mastery", description: "CI/CD pipelines, container orchestration, infrastructure as code, and site reliability engineering.", memberCount: 89000, activityBadge: "800 Online", avatarUrl: null, isFollowed: false },
        { id: "c-4", name: "AI/ML Ops", slug: "ai-ml-ops", description: "Discussions on deploying and scaling machine learning models in production.", memberCount: 112000, activityBadge: "2.1k Online", avatarUrl: null, isFollowed: false }
      ]
    })
  }),
  http.post('/api/communities/:id/follow', async ({ request, params }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({ communityId: params.id, isFollowed: true }, { status: 201 })
  }),
  http.delete('/api/communities/:id/follow', async ({ request, params }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({ communityId: params.id, isFollowed: false })
  }),
  http.get('/api/feeds/custom', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({
      items: [
        { id: "f-1", name: "System Architecture", tags: ["rust", "distributed-systems"], communityIds: [], sortOrder: "new", createdAt: new Date().toISOString() },
        { id: "f-2", name: "Frontend Perf", tags: ["react", "webgl", "performance"], communityIds: [], sortOrder: "hot", createdAt: new Date().toISOString() },
        { id: "f-3", name: "AI/ML Ops", tags: ["pytorch", "mlops", "cuda"], communityIds: [], sortOrder: "top", createdAt: new Date().toISOString() }
      ]
    })
  }),
  http.post('/api/feeds/custom', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        const body = await request.json()
    return HttpResponse.json({ id: `f-${Date.now()}`, name: body.name, tags: body.tags, communityIds: body.communityIds, sortOrder: body.sortOrder, createdAt: new Date().toISOString() }, { status: 201 })
  }),

  // Phase 7
  http.get('/api/notifications', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({
      items: [
        {
          id: "n-1",
          type: "mention",
          actorName: "Sarah Chen",
          actorAvatar: "https://lh3.googleusercontent.com/aida-public/AB6AXuCPGh8m1JYjNQ7asSVJ5CsUi8nSOCDFTTJ5nxNbUW3MZDfIQdufQrqFjKC684ecOffXWiSgMpW51GmZGVUCmNn6zjPEQ46UQNf25jFjdkmqvuuFTJVwQtQQAoDqDvmkFkXhSNtFwdWFZwhJn5IREQUsjkPhdIA44DSzYUce5KisqK0D28l-HZyt5bQWxBwYyhjBHSd0TdJk1ESw1cQzu8-djq4B880gnPk_RimGRltdjnLtHRG-XnzJw57TBePbrFoI3O1XXYIGg_k",
          targetTitle: "feat/async-renderer",
          targetUrl: "#",
          isRead: false,
          contentExcerpt: "Hey @dev_user, I implemented the changes we discussed. Could you review the threading model on lines 45-60?",
          createdAt: new Date(Date.now() - 10 * 60000).toISOString()
        },
        {
          id: "n-2",
          type: "reply",
          actorName: "Alex Rivera",
          actorAvatar: "https://lh3.googleusercontent.com/aida-public/AB6AXuDW1ogvpVShoYOkZEjeB4wvqJUsJ9R99yVVBaqn0cf9vgsXpH-wTuHo1HNlOzqwEJ4lkJhkSFOv-empqNUikLTL2bCnPf3qLoeUEuHkynmU70DRJfwa7PwpsNbJ4edTLVMamDVMLia2yihVTA4SNvyYmY8YcruqlL4uEDMu0nftog362FWzOT5WeyvwWLql1YzFPnvnGyFiAqxv205iDZcjEDYyNQ5BprNFWMoeEXnqgZuhAXHmES_opXHbyGzrQQGqR1E3uNgajs8",
          targetTitle: '"Understanding Memory Leaks in Node.js"',
          targetUrl: "#",
          isRead: false,
          contentExcerpt: '"That makes sense. I hadn\'t considered the impact of closure scope retaining the large array. Thanks for the breakdown!"',
          createdAt: new Date(Date.now() - 60 * 60000).toISOString()
        },
        {
          id: "n-3",
          type: "System Alert",
          actorName: "System",
          actorAvatar: null,
          targetTitle: "#1042",
          targetUrl: "#",
          isRead: true,
          contentExcerpt: "main ← feat/auth-refactor",
          createdAt: new Date(Date.now() - 4 * 3600000).toISOString()
        },
        {
          id: "n-4",
          type: "like",
          actorName: "David Kim and 12 others",
          actorAvatar: null,
          targetTitle: '"Optimizing React Re-renders"',
          targetUrl: "#",
          isRead: true,
          contentExcerpt: null,
          createdAt: new Date(Date.now() - 24 * 3600000).toISOString()
        }
      ],
      unreadCount: 2,
      totalCount: 4,
      page: 1,
      pageSize: 20
    })
  }),
  http.put('/api/notifications/:id/read', async ({ request, params }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({ id: params.id, isRead: true })
  }),
  http.post('/api/notifications/read-all', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({ markedCount: 2 })
  }),

  // Phase 8
  http.get('/api/users/:username/analytics/summary', async ({ request, params }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({
      username: params.username,
      period: "30d",
      totalViews: 15420,
      votesReceived: 1425,
      commentCount: 342,
      topTags: [
        { name: "reactjs", postCount: 45 },
        { name: "rust", postCount: 32 },
        { name: "system-design", postCount: 28 }
      ],
      dailyViews: Array.from({ length: 30 }).map((_, i) => {
        const d = new Date();
        d.setDate(d.getDate() - (29 - i));
        return {
          date: d.toISOString().split('T')[0],
          count: Math.floor(Math.random() * 500) + 100
        };
      })
    })
  }),
  http.get('/api/analytics/community-detailed', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({
      activeUserCount: 124592,
      totalPostCount: 8234,
      flaggedContentCount: 45,
      newUserCount: 1250
    })
  }),

  // Phase 9
  http.get('/api/admin/metrics', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({
      activeUserCount: 124592,
      totalPostCount: 8234,
      flaggedContentCount: 47,
      newUserCount: 1250,
      snapshotAt: new Date().toISOString()
    })
  }),
  http.get('/api/admin/users', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({
      items: [
        { id: "u1", username: "Alex.Dev", email: "alex@example.com", role: "Admin", status: "Active", reputation: 14200, createdAt: "2021-10-12" },
        { id: "u2", username: "SarahScripts", email: "sarah@example.com", role: "Moderator", status: "Active", reputation: 8400, createdAt: "2022-01-05" },
        { id: "u3", username: "TrollMaster99", email: "tm@spam.com", role: "User", status: "Banned", reputation: -240, createdAt: "2023-11-11" },
        { id: "u4", username: "JSON_Derulo", email: "json@example.com", role: "User", status: "Active", reputation: 1200, createdAt: "2024-03-22" }
      ],
      totalCount: 14293,
      page: 1,
      pageSize: 20
    })
  }),
  http.put('/api/admin/users/:id/role', async ({ params, request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        const { role } = await request.json()
    return HttpResponse.json({ id: params.id, role })
  }),
  http.put('/api/admin/users/:id/status', async ({ params, request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        const { status } = await request.json()
    return HttpResponse.json({ id: params.id, status })
  }),
  http.get('/api/moderation/queue', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        return HttpResponse.json({
      items: [
        {
          id: "REP-8832",
          contentType: "Comment",
          contentExcerpt: "Honestly, if you're writing your auth service in Python in 2024, you're an absolute idiot...",
          reason: "Harassment",
          severity: "High",
          reportedAt: new Date().toISOString(),
          author: { username: "toxic_coder", reputation: -45, joinedAt: "2024-02-14", recentReports: 4 },
          reporter: { username: "sysadmin_jane" }
        }
      ]
    })
  }),
  http.post('/api/moderation/resolve', async ({ request }) => {

    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });

        const { reportId, action } = await request.json()
    return HttpResponse.json({ reportId, action, resolved: true })
  })
]
