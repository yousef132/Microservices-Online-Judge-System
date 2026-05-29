const fs = require('fs');
let code = fs.readFileSync('d:/Microservices-Online-Judge-System/src/Web/src/App.jsx', 'utf8');

// remove ENDPOINTS array
code = code.replace(/const ENDPOINTS = \[[\s\S]*?\];/m, '');

// remove EndpointWorkbench route
code = code.replace(/<Route path="\/endpoints" element=\{<EndpointWorkbench api=\{api\} \/>\} \/>\n\s*/m, '');

// remove EndpointWorkbench component
code = code.replace(/function EndpointWorkbench\(\{ api \}\) \{[\s\S]*?\}\n\nfunction ArticleList/m, 'function ArticleList');

// remove EndpointCoverage component
code = code.replace(/function EndpointCoverage\(\) \{[\s\S]*?\}\n\nfunction PageHeading/m, 'function PageHeading');

fs.writeFileSync('d:/Microservices-Online-Judge-System/src/Web/src/App.jsx', code);
