// @vitest-environment node
// Each architecture rule catches a breach and passes a clean tree, so the rules table in docs/rules.md is backed by a
// working check, not only by a script that happens to print nothing.
import { posix } from 'node:path';
import {
  fileNames,
  hookFileNames,
  lineLimits,
  logicHasTests,
  noLiteralColours,
  oneComponentPerFile,
  oneLocale,
} from './file-rules.mjs';
import {
  folderShape,
  noDumpNames,
  srcRootHoldsOnlyTheAreas,
} from './folder-rules.mjs';
import {
  apiClientOnlyFromDataAccess,
  apiClientThroughItsIndex,
  clientInstanceOnlyFromDataAccess,
  featuresAreIsolated,
  importDirection,
} from './import-rules.mjs';
import { isCode, relativeImports } from './source-tree.mjs';

/** A tree like `readTree` gives, from `{ 'src/a/b.ts': 'text' }`; folders come from the paths. */
function treeOf(sources) {
  const folders = new Map();
  const folder = (path) => {
    if (!folders.has(path))
      folders.set(path, { path, files: [], subfolders: [], others: [] });
    return folders.get(path);
  };
  folder('src');
  const files = [];
  for (const [path, text] of Object.entries(sources)) {
    const parts = path.split('/');
    for (let i = 2; i < parts.length; i++) {
      const parent = folder(parts.slice(0, i - 1).join('/'));
      folder(parts.slice(0, i).join('/'));
      if (!parent.subfolders.includes(parts[i - 1]))
        parent.subfolders.push(parts[i - 1]);
    }
    const dir = parts.slice(0, -1).join('/');
    const name = parts.at(-1);
    if (name === '') continue;
    if (!isCode(name)) {
      folder(dir).others.push(name);
      continue;
    }
    folder(dir).files.push(name);
    files.push({
      path,
      name,
      text,
      imports: relativeImports(text).map((t) => posix.join(dir, t)),
    });
  }
  return { folders: [...folders.values()], files };
}

const component = (name) => `export function ${name}() {\n  return null;\n}\n`;
const pair = (dir) => ({
  [`${dir}/One.tsx`]: component('One'),
  [`${dir}/Two.tsx`]: component('Two'),
});

describe('folders', () => {
  test('src holds only the entry and the four areas', () => {
    expect(srcRootHoldsOnlyTheAreas(treeOf({ 'src/main.tsx': '' }))).toEqual(
      [],
    );
    expect(
      srcRootHoldsOnlyTheAreas(treeOf({ 'src/stuff/a.ts': '' })),
    ).toHaveLength(1);
  });

  test('at most six production files; tests and index.ts do not count', () => {
    const six = Object.fromEntries(
      [1, 2, 3, 4, 5, 6].map((n) => [`src/features/a/f${n}.ts`, '']),
    );
    const withTests = {
      ...six,
      'src/features/a/f1.test.ts': '',
      'src/features/a/index.ts': '',
    };
    expect(folderShape(treeOf(withTests))).toEqual([]);
    expect(
      folderShape(treeOf({ ...six, 'src/features/a/f7.ts': '' })).join(),
    ).toMatch(/7 files/);
  });

  test('files or subfolders, never both; no single file, no empty or wrapper folder', () => {
    expect(
      folderShape(
        treeOf({ ...pair('src/features/a'), ...pair('src/features/a/b') }),
      ).join(),
    ).toMatch(/files and subfolders/);
    expect(
      folderShape(treeOf({ 'src/features/a/x/One.tsx': '' })).join(),
    ).toMatch(/single file/);
    expect(folderShape(treeOf({ 'src/features/a/x/': '' })).join()).toMatch(
      /empty/,
    );
    expect(folderShape(treeOf(pair('src/features/a/inner'))).join()).toMatch(
      /wrapper/,
    );
  });

  test('generated code is exempt', () => {
    const generated = Object.fromEntries(
      [1, 2, 3, 4, 5, 6, 7].map((n) => [
        `src/api-client/generated/g${n}.ts`,
        '',
      ]),
    );
    expect(
      folderShape(
        treeOf({
          ...generated,
          ...pair('src/api-client/client'),
          'src/api-client/index.ts': '',
        }),
      ),
    ).toEqual([]);
  });

  test('no dump folder or file names', () => {
    expect(
      noDumpNames(treeOf({ 'src/features/a/utils/x.ts': '' })),
    ).toHaveLength(1);
    expect(
      noDumpNames(treeOf({ 'src/features/a/helpers.ts': '' })),
    ).toHaveLength(1);
    expect(
      noDumpNames(treeOf({ 'src/features/a/format-amount.ts': '' })),
    ).toEqual([]);
  });
});

describe('files', () => {
  test('a component has at most 150 lines, a module 200; tests are exempt', () => {
    const lines = (n) => 'x\n'.repeat(n);
    expect(lineLimits(treeOf({ 'src/app/A.tsx': lines(150) }))).toEqual([]);
    expect(lineLimits(treeOf({ 'src/app/A.tsx': lines(151) }))).toHaveLength(1);
    expect(lineLimits(treeOf({ 'src/app/a.ts': lines(201) }))).toHaveLength(1);
    expect(lineLimits(treeOf({ 'src/app/a.test.ts': lines(900) }))).toEqual([]);
  });

  test('components are PascalCase .tsx, the rest kebab-case', () => {
    expect(
      fileNames(treeOf({ 'src/app/Menu.tsx': '', 'src/app/use-menu.ts': '' })),
    ).toEqual([]);
    expect(fileNames(treeOf({ 'src/app/Menu.ts': '' }))).toHaveLength(1);
    expect(fileNames(treeOf({ 'src/app/menu_items.ts': '' }))).toHaveLength(1);
  });

  test('one component per file, named after it', () => {
    expect(
      oneComponentPerFile(treeOf({ 'src/app/One.tsx': component('One') })),
    ).toEqual([]);
    expect(
      oneComponentPerFile(
        treeOf({ 'src/app/One.tsx': component('One') + component('Two') }),
      ),
    ).toHaveLength(1);
    expect(
      oneComponentPerFile(treeOf({ 'src/app/One.tsx': component('Two') })),
    ).toHaveLength(1);
  });

  test('a hook lives in use-*.ts or *-api.ts', () => {
    const hook = 'export function useThing() {}\n';
    expect(hookFileNames(treeOf({ 'src/app/use-thing.ts': hook }))).toEqual([]);
    expect(hookFileNames(treeOf({ 'src/app/thing-api.ts': hook }))).toEqual([]);
    expect(hookFileNames(treeOf({ 'src/app/thing.ts': hook }))).toHaveLength(1);
  });

  test('literal colours only in app/theme', () => {
    const red = "const c = '#ff0000';\n";
    expect(
      noLiteralColours(treeOf({ 'src/app/theme/palette.ts': red })),
    ).toEqual([]);
    expect(
      noLiteralColours(treeOf({ 'src/features/a/Card.tsx': red })),
    ).toHaveLength(1);
    expect(
      noLiteralColours(
        treeOf({
          'src/features/a/Card.tsx': "const c = 'rgba(0, 0, 0, 0.1)';",
        }),
      ),
    ).toHaveLength(1);
  });

  test('the locale only in platform/locale.ts', () => {
    const uk = "export const locale = 'uk-UA';\n";
    expect(oneLocale(treeOf({ 'src/platform/locale.ts': uk }))).toEqual([]);
    expect(
      oneLocale(
        treeOf({ 'src/features/a/format.ts': "new Intl.NumberFormat('ru-RU');" }),
      ),
    ).toHaveLength(1);
    expect(
      oneLocale(treeOf({ 'src/features/a/sort.ts': "a.localeCompare(b, 'ru');" })),
    ).toHaveLength(1);
  });

  test('pure logic in a feature has a test next to it', () => {
    const logic = 'export function total() {}\n';
    expect(
      logicHasTests(treeOf({ 'src/features/a/total.ts': logic })),
    ).toHaveLength(1);
    expect(
      logicHasTests(
        treeOf({
          'src/features/a/total.ts': logic,
          'src/features/a/total.test.ts': '',
        }),
      ),
    ).toEqual([]);
    expect(logicHasTests(treeOf({ 'src/features/a/a-api.ts': logic }))).toEqual(
      [],
    );
  });
});

describe('imports', () => {
  test('features never import each other', () => {
    const tree = treeOf({
      'src/features/a/x.ts': "import { y } from '../b/y';",
      'src/features/b/y.ts': '',
    });
    expect(featuresAreIsolated(tree)).toHaveLength(1);
  });

  test('imports go main → app → features → platform → api-client', () => {
    const ok = treeOf({
      'src/app/root/App.tsx': "import { A } from '../../features/a/A';",
      'src/features/a/a-api.ts': "import { api } from '../../platform/api';",
      'src/platform/api.ts': "import { x } from '../api-client';",
    });
    expect(importDirection(ok)).toEqual([]);
    for (const [path, text] of [
      ['src/features/a/A.tsx', "import { t } from '../../app/theme/theme';"],
      ['src/platform/x.ts', "import { A } from '../features/a/A';"],
      ['src/api-client/x.ts', "import { api } from '../platform/api';"],
    ])
      expect(importDirection(treeOf({ [path]: text }))).toHaveLength(1);
    expect(
      importDirection(
        treeOf({
          'src/features/a/A.test.tsx':
            "import { t } from '../../app/theme/theme';",
        }),
      ),
    ).toEqual([]);
  });

  test('the API client only from platform/ and *-api.ts, the instance only from *-api.ts', () => {
    const client = "import { apiCall } from '../../api-client';";
    const instance = "import { api } from '../../platform/api';";
    expect(
      apiClientOnlyFromDataAccess(
        treeOf({ 'src/features/a/a-api.ts': client }),
      ),
    ).toEqual([]);
    expect(
      apiClientOnlyFromDataAccess(treeOf({ 'src/features/a/A.tsx': client })),
    ).toHaveLength(1);
    expect(
      clientInstanceOnlyFromDataAccess(
        treeOf({ 'src/features/a/a-api.ts': instance }),
      ),
    ).toEqual([]);
    expect(
      clientInstanceOnlyFromDataAccess(
        treeOf({ 'src/features/a/form.ts': instance }),
      ),
    ).toHaveLength(1);
  });

  test('the API client through its index only', () => {
    expect(
      apiClientThroughItsIndex(
        treeOf({
          'src/features/a/a-api.ts':
            "import type { paths } from '../../api-client/generated/api';",
        }),
      ),
    ).toHaveLength(1);
  });
});
