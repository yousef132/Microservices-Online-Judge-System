const fs = require('fs');
let code = fs.readFileSync('d:/Microservices-Online-Judge-System/src/Web/src/mocks/handlers.js', 'utf8');

const SCENARIO_INJECTION = `
    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });
`;

code = code.replace(/async \(\) => {/g, "async ({ request }) => {");
code = code.replace(/async \(\{ params \}\) => {/g, "async ({ request, params }) => {");

// remove existing scenario handling for the first 5 handlers to keep it uniform
code = code.replace(/const url = new URL\(request\.url\)(.|\n)*?if \(scenario === 'empty'\) \{\s*\n?\s*return HttpResponse\.json\(\{.*\}\)\s*\n?\s*\}?/g, '');
// just wipe out any standard empty returns for the top 5
code = code.replace(/if \(scenario === 'empty'\) return HttpResponse\.json\(\{.*\}\)/g, '');

// remove existing delays
code = code.replace(/await delay\(\d+\);?\n/g, '');

// now prepend our standard injection
code = code.replace(/(http\.(get|post|put|delete)\('.*?', async \(\{.*?\}\) => \{)/g, "$1\n" + SCENARIO_INJECTION);

// re-add the empty scenarios
code = code.replace(
  /http\.get\('\/api\/notifications\/unread-count', async \(\{ request \}\) => \{[\s\S]*?if \(scenario === 'not-found'\) return new HttpResponse\(null, \{ status: 404 \}\);/m,
  `$&
    if (scenario === 'empty') return HttpResponse.json({ count: 0 });`
);

code = code.replace(
  /http\.get\('\/api\/search\/suggestions', async \(\{ request \}\) => \{[\s\S]*?if \(scenario === 'not-found'\) return new HttpResponse\(null, \{ status: 404 \}\);/m,
  `$&
    const q = url.searchParams.get('q');
    if (scenario === 'empty') return HttpResponse.json({ articles: [], tags: [], authors: [] });`
);

code = code.replace(
  /http\.get\('\/api\/explore\/highlights', async \(\{ request \}\) => \{[\s\S]*?if \(scenario === 'not-found'\) return new HttpResponse\(null, \{ status: 404 \}\);/m,
  `$&
    if (scenario === 'empty') return HttpResponse.json({ trendingTags: [], featuredArticles: [], activeCommunities: [] });`
);

code = code.replace(
  /http\.get\('\/api\/tags\/summary', async \(\{ request \}\) => \{[\s\S]*?if \(scenario === 'not-found'\) return new HttpResponse\(null, \{ status: 404 \}\);/m,
  `$&
    if (scenario === 'empty') return HttpResponse.json({ items: [], totalCount: 0 });`
);

code = code.replace(
  /http\.get\('\/api\/search', async \(\{ request \}\) => \{[\s\S]*?if \(scenario === 'not-found'\) return new HttpResponse\(null, \{ status: 404 \}\);/m,
  `$&
    const q = url.searchParams.get('q') || '';
    const category = url.searchParams.get('category') || 'articles';
    if (scenario === 'empty') return HttpResponse.json({ query: q, category, items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 });`
);

fs.writeFileSync('d:/Microservices-Online-Judge-System/src/Web/src/mocks/handlers.js', code);
