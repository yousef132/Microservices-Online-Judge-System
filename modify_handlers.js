const fs = require('fs');

let code = fs.readFileSync('d:/Microservices-Online-Judge-System/src/Web/src/mocks/handlers.js', 'utf8');

// The logic we want to insert at the top of every handler:
const SCENARIO_INJECTION = `
    const url = new URL(request.url, 'http://localhost');
    const scenario = url.searchParams.get('msw_scenario');
    
    // Default delay
    await delay(500);

    if (scenario === 'error') return new HttpResponse(null, { status: 500 });
    if (scenario === 'not-found') return new HttpResponse(null, { status: 404 });
`;

// Find all `http.*(..., async ({ request, params }) => {`
// First let's normalize all handler arguments to ensure `request` is available
code = code.replace(/async \(\) => {/g, "async ({ request }) => {");
code = code.replace(/async \(\{ params \}\) => {/g, "async ({ request, params }) => {");

// Now remove existing scenario code and delays to avoid duplication
code = code.replace(/const url = new URL\(request\.url\);?\n/g, "");
code = code.replace(/const scenario = url\.searchParams\.get\('msw_scenario'\);?\n/g, "");
code = code.replace(/await delay\(\d+\);?\n/g, "");
code = code.replace(/if \(scenario === 'error'\) .*?\n/g, "");
code = code.replace(/if \(scenario === 'empty'\) .*?\n/g, "");

// We will manually add the empty scenario returns for array/object endpoints.
// But first, let's inject the standard boilerplate.
code = code.replace(/(http\.(get|post|put|delete)\('.*?', async \(\{.*?\}\) => \{)/g, "$1\n" + SCENARIO_INJECTION);

fs.writeFileSync('d:/Microservices-Online-Judge-System/src/Web/src/mocks/handlers2.js', code);
