// App rules (docs/rules.md, "App"): the app is never indexed. Each rule takes the web root's files and returns
// problem lines.
import { existsSync, readFileSync } from 'node:fs';
import { join } from 'node:path';

export function neverIndexed(root) {
  const problems = [];
  const html = readFileSync(join(root, 'index.html'), 'utf8');
  if (!/<meta\s+name="robots"\s+content="[^"]*noindex/.test(html))
    problems.push(
      'index.html: needs <meta name="robots" content="noindex, …">',
    );
  const robots = join(root, 'public', 'robots.txt');
  const text = existsSync(robots) ? readFileSync(robots, 'utf8') : '';
  if (!/User-agent:\s*\*[\s\S]*Disallow:\s*\/\s*$/m.test(text))
    problems.push(
      'public/robots.txt: must disallow everything («Disallow: /»)',
    );
  return problems;
}
