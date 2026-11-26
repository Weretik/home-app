// The architecture rules of the web app, checked in CI (`npm run architecture:structure`). The rules live in
// tools/architecture/ (one module per kind: folders, files, imports, app); docs/rules.md lists every rule with its
// check. Fails with one line per problem.
import { fileURLToPath } from 'node:url';
import { rootRules, treeRules } from './architecture/rules.mjs';
import { readTree } from './architecture/source-tree.mjs';

const root = fileURLToPath(new URL('..', import.meta.url));
const tree = readTree(root);
const problems = [
  ...treeRules.flatMap((rule) => rule(tree)),
  ...rootRules.flatMap((rule) => rule(root)),
];

if (problems.length > 0) {
  console.error(problems.join('\n'));
  process.exit(1);
}
console.log(
  `Structure is fine: ${treeRules.length + rootRules.length} rules, ${tree.files.length} files.`,
);
