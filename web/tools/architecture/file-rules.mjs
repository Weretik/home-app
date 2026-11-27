// File rules (docs/rules.md, "Structure", "Tests and checks" and docs/design.md). Each rule takes the tree and
// returns problem lines. Tests and generated code are exempt unless a rule says otherwise.
import { isGenerated, isTest } from './source-tree.mjs';

/** Line limits per production file; over the limit, split it by responsibility. */
export const MAX_LINES = { '.tsx': 150, '.ts': 200 };

const production = (tree) =>
  tree.files.filter((file) => !isTest(file.name) && !isGenerated(file.path));
const extension = (name) => (name.endsWith('.tsx') ? '.tsx' : '.ts');
const lineCount = (text) => text.replace(/\n$/, '').split('\n').length;

export function lineLimits(tree) {
  return production(tree)
    .filter((file) => lineCount(file.text) > MAX_LINES[extension(file.name)])
    .map((file) => {
      const kind = extension(file.name);
      return `${file.path}: ${lineCount(file.text)} lines, limit ${MAX_LINES[kind]} for ${kind === '.tsx' ? 'a component (.tsx)' : 'a module (.ts)'}; split it by responsibility`;
    });
}

const PASCAL = /^[A-Z][A-Za-z0-9]*$/;
const KEBAB = /^[a-z0-9]+(-[a-z0-9]+)*$/;
const baseName = (name) => name.replace(/(\.test|\.spec)?\.tsx?$/, '');

/** Component files are PascalCase `.tsx`; everything else is kebab-case. */
export function fileNames(tree) {
  const problems = [];
  for (const file of tree.files.filter((f) => !isGenerated(f.path))) {
    const base = baseName(file.name);
    if (PASCAL.test(base) && !file.name.endsWith('.tsx'))
      problems.push(
        `${file.path}: a PascalCase name is for a component (.tsx); name a module in kebab-case`,
      );
    else if (!PASCAL.test(base) && !KEBAB.test(base))
      problems.push(
        `${file.path}: name a component in PascalCase and anything else in kebab-case`,
      );
  }
  return problems;
}

/** Top-level component declarations: `function Name(` or `const Name = (` at the start of a line. */
const componentDeclarations = (text) =>
  [
    ...text.matchAll(
      /^(?:export\s+)?(?:function\s+([A-Z]\w*)\s*[(<]|const\s+([A-Z]\w*)\s*=\s*(?:\([^)]*\)|\w+)\s*=>)/gm,
    ),
  ].map((match) => match[1] ?? match[2]);

/** One component per file, and it is the file's name. */
export function oneComponentPerFile(tree) {
  const problems = [];
  for (const file of production(tree).filter((f) => f.name.endsWith('.tsx'))) {
    const names = componentDeclarations(file.text);
    if (names.length > 1)
      problems.push(
        `${file.path}: ${names.length} components (${names.join(', ')}); one component per file`,
      );
    const base = baseName(file.name);
    if (names.length === 1 && PASCAL.test(base) && names[0] !== base)
      problems.push(
        `${file.path}: declares ${names[0]}; a component file is named after its component`,
      );
  }
  return problems;
}

/** A module that exports a React hook is a hook file (`use-*.ts`) or the feature's data access (`*-api.ts`). */
export function hookFileNames(tree) {
  return production(tree)
    .filter((file) => file.name.endsWith('.ts'))
    .filter((file) =>
      /^export\s+(?:function\s+use[A-Z]|const\s+use[A-Z])/m.test(file.text),
    )
    .filter((file) => !/^use-.+\.ts$|-api\.ts$/.test(file.name))
    .map(
      (file) =>
        `${file.path}: exports a hook; name it use-<name>.ts, or <area>-api.ts for data access`,
    );
}

/** Literal colours belong to the theme only (docs/design.md: theme tokens only). */
export function noLiteralColours(tree) {
  const literal =
    /['"`](?:#[0-9a-fA-F]{3,8}|(?:rgba?|hsla?)\(\s*\d)[^'"`]*['"`]/;
  return production(tree)
    .filter((file) => !file.path.startsWith('src/app/theme/'))
    .filter((file) => literal.test(file.text))
    .map(
      (file) =>
        `${file.path}: a literal colour; use a theme token (docs/design.md)`,
    );
}

/**
 * One interface language (docs/decisions/0014-ukrainian-interface.md): no locale literal such as `'ru-RU'` or `'uk'`
 * outside `src/platform/client/locale.ts`; `Intl` and `localeCompare` take `locale` from there. (`'en-CA'`, used only to
 * build ISO `YYYY-MM-DD` keys, is not an interface language and is allowed.)
 */
export function oneLocale(tree) {
  const literal = /['"`](?:ru|uk)(?:-[A-Z]{2})?['"`]/;
  return production(tree)
    .filter((file) => file.path !== 'src/platform/client/locale.ts')
    .filter((file) => literal.test(file.text))
    .map(
      (file) =>
        `${file.path}: a locale literal; import \`locale\` from src/platform/client/locale.ts`,
    );
}

/**
 * Logic is tested first (docs/rules.md, "Tests and checks"): a feature module with pure logic — a `.ts` file that is
 * not a hook (`use-*.ts`), not data access (`*-api.ts`) and exports a function — has a test next to it.
 */
export function logicHasTests(tree) {
  const tests = new Set(
    tree.files
      .filter((file) => isTest(file.name))
      .map((file) => file.path.replace(/\.(test|spec)\.tsx?$/, '')),
  );
  return production(tree)
    .filter((file) => file.path.startsWith('src/features/'))
    .filter((file) => file.name.endsWith('.ts'))
    .filter((file) => !/^use-|-api\.ts$/.test(file.name))
    .filter((file) =>
      /^export\s+(?:function\s|const\s+\w+\s*=\s*(?:<[^>]*>)?\()/m.test(
        file.text,
      ),
    )
    .filter((file) => !tests.has(file.path.replace(/\.ts$/, '')))
    .map(
      (file) =>
        `${file.path}: pure logic without a test; add ${file.name.replace(/\.ts$/, '.test.ts')}`,
    );
}
