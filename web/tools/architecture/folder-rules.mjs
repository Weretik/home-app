// Folder rules (docs/rules.md, "Structure"). Each rule takes the tree and returns problem lines.
import { isGenerated, isProductionFile } from './source-tree.mjs';

export const MAX_FILES = 6;
/** Names that say nothing about what is inside; name a folder or file by what it does. */
export const DUMP_NAMES = new Set([
  'utils',
  'utilities',
  'util',
  'helpers',
  'helper',
  'common',
  'shared',
  'misc',
  'types',
  'constants',
  'lib',
  'stuff',
]);
/** What src/ itself may hold: the entry and the four areas (docs/architecture.md, "Folders"). */
export const SRC_ROOT = new Set([
  'main.tsx',
  'vite-env.d.ts',
  'app',
  'platform',
  'api-client',
  'features',
]);

const leaf = (path) => path.split('/').at(-1);
const checked = (tree) =>
  tree.folders.filter((folder) => !isGenerated(folder.path));

export function srcRootHoldsOnlyTheAreas(tree) {
  const src = tree.folders.find((folder) => folder.path === 'src');
  return [...src.files, ...src.subfolders, ...src.others]
    .filter((name) => !SRC_ROOT.has(name))
    .map((name) => `src/${name}: src/ holds only ${[...SRC_ROOT].join(', ')}`);
}

export function noDumpNames(tree) {
  const folders = checked(tree)
    .filter((folder) => DUMP_NAMES.has(leaf(folder.path).toLowerCase()))
    .map((folder) => `${folder.path}: dump folder name, name it by meaning`);
  const files = tree.files
    .filter((file) => !isGenerated(file.path))
    .filter((file) =>
      DUMP_NAMES.has(
        file.name.replace(/(\.test|\.spec)?\.tsx?$/, '').toLowerCase(),
      ),
    )
    .map((file) => `${file.path}: dump file name, name it by what it does`);
  return [...folders, ...files];
}

export function folderShape(tree) {
  const problems = [];
  for (const folder of checked(tree)) {
    if (folder.path === 'src') continue;
    const production = folder.files.filter(isProductionFile);
    const subfolders = folder.subfolders.filter((name) => name !== 'generated');
    const hasGenerated = folder.subfolders.includes('generated');
    if (production.length > MAX_FILES)
      problems.push(
        `${folder.path}: ${production.length} files, at most ${MAX_FILES}; split it into subfolders by meaning`,
      );
    if (production.length > 0 && subfolders.length > 0)
      problems.push(
        `${folder.path}: holds files and subfolders, keep one kind`,
      );
    if (production.length === 1 && subfolders.length === 0 && !hasGenerated)
      problems.push(`${folder.path}: a folder with a single file, merge it`);
    if (
      folder.files.length + folder.subfolders.length + folder.others.length ===
      0
    )
      problems.push(`${folder.path}: an empty folder, remove it`);
    const isArea = folder.path.split('/').length === 2; // src/<area> may hold one feature
    if (
      production.length === 0 &&
      subfolders.length === 1 &&
      !hasGenerated &&
      !isArea
    )
      problems.push(
        `${folder.path}: a wrapper folder around one subfolder, flatten it`,
      );
  }
  return problems;
}
