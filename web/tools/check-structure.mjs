// Structure rules of the web app, checked in CI (docs/rules.md):
// - a folder holds at most six production files (tests and index.ts do not count; generated code is exempt);
// - a folder holds either files or subfolders, never both, and never a single file alone;
// - folders are named by feature or meaning, never as a dump (utils, helpers, common, ...);
// - a feature never imports another feature;
// - only src/platform and a feature's *-api.ts files import the API client;
// - a component file (.tsx) has at most 150 lines, any other module (.ts) at most 200 (tests and generated code are
//   exempt).
import { readFileSync, readdirSync, statSync } from 'node:fs';
import { dirname, join, relative, resolve, sep } from 'node:path';

const MAX_FILES = 6;
const MAX_COMPONENT_LINES = 150;
const MAX_MODULE_LINES = 200;
const FORBIDDEN = new Set([
  'utils',
  'utilities',
  'helpers',
  'common',
  'shared',
  'misc',
  'types',
  'constants',
  'lib',
]);
const root = new URL('..', import.meta.url).pathname;
const src = join(root, 'src');

const isCode = (name) => /\.(ts|tsx)$/.test(name) && !name.endsWith('.d.ts');
const isTest = (name) => /\.(spec|test)\.tsx?$/.test(name);
const isProductionFile = (name) =>
  isCode(name) && !isTest(name) && name !== 'index.ts';

function folders(dir) {
  const result = [dir];
  for (const name of readdirSync(dir)) {
    const path = join(dir, name);
    if (statSync(path).isDirectory() && name !== 'generated')
      result.push(...folders(path));
  }
  return result;
}

const problems = [];
const allFolders = folders(src);

for (const folder of allFolders) {
  const rel = relative(root, folder);
  const entries = readdirSync(folder);
  const files = entries.filter(
    (name) => statSync(join(folder, name)).isFile() && isProductionFile(name),
  );
  const subfolders = entries.filter(
    (name) =>
      statSync(join(folder, name)).isDirectory() && name !== 'generated',
  );

  if (FORBIDDEN.has(rel.split(sep).at(-1).toLowerCase()))
    problems.push(`${rel}: dump folder name, name it by meaning`);
  if (folder === src) continue;
  if (files.length > MAX_FILES)
    problems.push(
      `${rel}: ${files.length} files, split it into subfolders by meaning`,
    );
  if (files.length > 0 && subfolders.length > 0)
    problems.push(`${rel}: holds files and subfolders, keep one kind`);
  if (files.length === 1 && subfolders.length === 0)
    problems.push(`${rel}: a folder with a single file, merge it`);
}

const area = (path) => {
  const parts = relative(src, path).split(sep);
  return parts[0] === 'features' ? `features/${parts[1]}` : parts[0];
};

for (const folder of allFolders) {
  for (const name of readdirSync(folder).filter(isCode)) {
    const file = join(folder, name);
    const from = area(file);
    const text = readFileSync(file, 'utf8');
    const limit = name.endsWith('.tsx')
      ? MAX_COMPONENT_LINES
      : MAX_MODULE_LINES;
    const lines = text.split('\n').length - (text.endsWith('\n') ? 1 : 0);
    if (!isTest(name) && lines > limit)
      problems.push(
        `${relative(root, file)}: ${lines} lines, at most ${limit}; split it by responsibility`,
      );
    const imports = [...text.matchAll(/from\s+'(\.[^']+)'/g)].map((match) =>
      resolve(dirname(file), match[1]),
    );

    for (const target of imports) {
      const to = area(target);
      const rel = relative(root, file);
      if (
        from.startsWith('features/') &&
        to.startsWith('features/') &&
        to !== from
      )
        problems.push(
          `${rel}: imports ${to}; features never import each other`,
        );
      if (
        to === 'api-client' &&
        from !== 'api-client' &&
        from !== 'platform' &&
        !name.endsWith('-api.ts') &&
        !isTest(name)
      )
        problems.push(
          `${rel}: imports the API client; only platform/ and *-api.ts may`,
        );
    }
  }
}

if (problems.length > 0) {
  console.error(problems.join('\n'));
  process.exit(1);
}
console.log('Structure is fine.');
