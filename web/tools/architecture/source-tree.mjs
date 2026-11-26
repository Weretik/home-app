// Reads src/ into plain data the rules check: every folder with its files and subfolders, every code file with its
// text and relative imports. Generated code (a folder named `generated`) is read for imports but exempt from the
// folder and file rules (docs/rules.md, "Structure").
import { readFileSync, readdirSync, statSync } from 'node:fs';
import { join, posix, relative, sep } from 'node:path';

export const isCode = (name) =>
  /\.(ts|tsx)$/.test(name) && !name.endsWith('.d.ts');
export const isTest = (name) => /\.(spec|test)\.tsx?$/.test(name);
/** A production file counts towards a folder's six; tests and `index.ts` do not. */
export const isProductionFile = (name) =>
  isCode(name) && !isTest(name) && name !== 'index.ts';
export const isGenerated = (path) => path.split('/').includes('generated');

/** Relative imports and re-exports of a file (`from './x'`, `import './x'`, `import('./x')`). */
export function relativeImports(text) {
  const pattern =
    /(?:\bfrom\s+|\bimport\s*\(?\s*)['"](\.{1,2}\/[^'"]*|\.{1,2})['"]/g;
  return [...text.matchAll(pattern)].map((match) => match[1]);
}

/**
 * The tree under `src` as `{ folders, files }`. Paths are POSIX and relative to the web root (`src/features/x`).
 * `folders`: `{ path, files: [names of code files], subfolders: [names] }`; `files`: `{ path, name, text, imports }`
 * with `imports` resolved to POSIX paths without extension.
 */
export function readTree(root) {
  const folders = [];
  const files = [];
  const walk = (dir) => {
    const rel = relative(root, dir).split(sep).join('/');
    const entries = readdirSync(dir);
    const folder = { path: rel, files: [], subfolders: [], others: [] };
    folders.push(folder);
    for (const name of entries) {
      const path = join(dir, name);
      if (statSync(path).isDirectory()) {
        folder.subfolders.push(name);
        walk(path);
      } else if (isCode(name)) {
        folder.files.push(name);
        const text = readFileSync(path, 'utf8');
        files.push({
          path: `${rel}/${name}`,
          name,
          text,
          imports: relativeImports(text).map((target) =>
            posix.join(rel, target),
          ),
        });
      } else folder.others.push(name);
    }
  };
  walk(join(root, 'src'));
  return { folders, files };
}
