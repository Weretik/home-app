// Import rules (docs/rules.md, "Structure" and "API"; AGENTS.md, "Boundaries"). Each rule takes the tree and returns
// problem lines. Only relative imports are between our own files; packages are checked by ESLint.
import { isTest } from './source-tree.mjs';

/** The area of a path: `main`, `app`, `platform`, `api-client`, or `features/<name>`. */
export function areaOf(path) {
  const parts = path.split('/');
  if (parts[0] !== 'src') return 'outside';
  if (parts[1] === 'main.tsx' || parts[1] === 'main') return 'main';
  return parts[1] === 'features' ? `features/${parts[2]}` : parts[1];
}

/**
 * Which areas each area may import (besides itself): imports go one way, from wiring down to the client.
 * main → app → features → platform → api-client. A test may also import app/ to render with the real theme.
 */
const ALLOWED = {
  main: ['app'],
  app: ['features', 'platform', 'api-client'],
  features: ['platform', 'api-client'],
  platform: ['api-client'],
  'api-client': [],
};
const kind = (area) => (area.startsWith('features/') ? 'features' : area);

const edges = (tree) =>
  tree.files.flatMap((file) =>
    file.imports.map((target) => ({
      file,
      from: areaOf(file.path),
      to: areaOf(target),
      target,
    })),
  );

export function featuresAreIsolated(tree) {
  return edges(tree)
    .filter(
      ({ from, to }) => kind(from) === 'features' && kind(to) === 'features',
    )
    .filter(({ from, to }) => from !== to)
    .map(
      ({ file, to }) =>
        `${file.path}: imports ${to}; features never import each other`,
    );
}

export function importDirection(tree) {
  return edges(tree)
    .filter(({ from, to }) => from !== to)
    .filter(
      ({ from, to }) => !(kind(from) === 'features' && kind(to) === 'features'),
    )
    .filter(({ from, to }) => !ALLOWED[kind(from)]?.includes(kind(to)))
    .filter(({ file, to }) => !(isTest(file.name) && to === 'app'))
    .map(
      ({ file, from, to }) =>
        `${file.path}: ${kind(from)} imports ${to}; imports go main → app → features → platform → api-client`,
    );
}

/** Only platform/ and a feature's `*-api.ts` use the API client (tests may, to build errors and types). */
export function apiClientOnlyFromDataAccess(tree) {
  return edges(tree)
    .filter(({ to, from }) => to === 'api-client' && from !== 'api-client')
    .filter(({ from }) => from !== 'platform')
    .filter(({ file }) => !file.name.endsWith('-api.ts') && !isTest(file.name))
    .map(
      ({ file }) =>
        `${file.path}: imports the API client; only platform/ and *-api.ts may`,
    );
}

/** The client instance (`src/platform/client/api.ts`) is called only from a feature's `*-api.ts`. */
export function clientInstanceOnlyFromDataAccess(tree) {
  return edges(tree)
    .filter(({ target }) => target === 'src/platform/client/api')
    .filter(({ file }) => !file.name.endsWith('-api.ts'))
    .map(
      ({ file }) =>
        `${file.path}: imports src/platform/client/api.ts; call the API from the feature's *-api.ts`,
    );
}

/** Outside src/api-client, the client is imported through its index, never its client/ or generated/ files. */
export function apiClientThroughItsIndex(tree) {
  return edges(tree)
    .filter(({ from, to }) => to === 'api-client' && from !== 'api-client')
    .filter(({ target }) => target !== 'src/api-client')
    .map(
      ({ file, target }) =>
        `${file.path}: imports ${target}; import the API client from src/api-client (its index)`,
    );
}
