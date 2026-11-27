# Web rules

Every rule below is either checked by a tool in CI or marked review only, with the reason; the
[table at the end](#rules-and-their-checks) maps each rule to its check. A new rule gets its check (or its reason) in
the same change.

## Process

- Every feature, above all a new one, goes SDD first, then the code, then an architecture audit
  ([repository ADR 0004](../../docs/decisions/0004-sdd-first-feature-delivery.md)): the specification in
  [specs](specs/README.md) (`frontend-feature-spec`), the implementation from it (`frontend-feature-delivery`), then
  `frontend-code-audit` against the SDD, these rules and the checklists; every finding is fixed or recorded.

## Structure

- Code is grouped by feature (`features/shopping/`), then by role inside a feature when it grows (`list/`, `editor/`,
  `data/`). Framework wiring lives in `app/`, environment and clients in `platform/`; `src/` holds only `main.tsx`,
  `app/`, `platform/`, `api-client/` and `features/`.
- **A folder holds at most six production files** (tests next to them and `index.ts` do not count; generated code is
  exempt). The seventh file splits the folder by meaning, never one folder per file.
- A folder holds either files or subfolders, not both, and never a single file alone; no empty folders and no wrapper
  folders around a single subfolder.
- No dump folders or files: `utils`, `helpers`, `common`, `shared`, `types`, `constants`, `lib`, `misc`. Name things
  by what they do (`format-amount.ts`).
- Imports go one way: `main.tsx` → `app/` → `features/` → `platform/` → `api-client/`. A feature never imports another
  feature; a rule both need keeps one copy on each side with the same test cases, or moves to `platform/` when it is
  wiring (like the query key prefixes).
- One component per file, named after it; component files are PascalCase `.tsx`, the rest kebab-case. A module that
  exports a hook is `use-<name>.ts`, or the feature's `<area>-api.ts` for data access.
- **A component (`.tsx`) has at most 150 lines, a hook, model or other module (`.ts`) at most 200** (tests and
  generated code are exempt). Over the limit, split it by responsibility: one component per visual block (a computer
  table and a phone list are two components), state and actions in a controller hook (`use-<name>.ts`), pure
  calculations in a model file. Never split into one-file wrapper folders; the folder rules above still hold.
- One responsibility per component even under the limit: a component that renders several independent blocks with
  their own data, or computes captions and lines itself, splits into a component per block and a model file.

## API

- Call the API only through the client in `src/platform/client/api.ts`, from the feature's `<area>-api.ts`; no raw `fetch`,
  Axios or a second `openapi-fetch` client. Outside `src/api-client`, import the client from its index only.
- Never edit `src/api-client/generated/`; regenerate it with `npm run contracts:generate` after the backend contract
  changes.
- Show the problem response's message to the user and map field errors onto form fields.

## Writes

- Every mutation disables its submit while pending (a queued write while it is `isSaving`: offline it waits without
  locking the screen).
- A write that must work offline is a queued write (`useQueuedWrite` from `src/platform/offline`, web ADR 0022): a
  serialisable request with its module, a Ukrainian label and, for a versioned record, the path to read its version
  again. Every other write fails at once offline (`networkMode: 'always'`).
- A write the backend protects with `Idempotency-Key` makes one key per attempt, reuses it on retry, and renews it
  after success or a change of the data.
- Every read of a backend module starts its query key with the module's prefix (`src/platform/client/query-client.ts`); every
  write to the module invalidates that prefix, so every page and summary showing the module refreshes
  ([ADR 0009](decisions/0009-writes-refresh-their-backend-module.md)).
- A write's type allows only what the API offers: a record that is only changed has no create or delete
  (`useChange`).

## App

- Server state in React Query, not in component state copies.
- The app is never indexed: `noindex` in `index.html`, `Disallow: /` in `public/robots.txt`.
- Mark only problems in lists, with the reason; no "OK" badges.
- UI text in Ukrainian, words from the [glossary](specs/platform/001-ukrainian-interface/glossary.md); layouts work on a phone first.
- Every screen follows the [design rules](design.md): theme tokens only, no literal colours outside `app/theme/`,
  both colour schemes, the shell's menus, the card patterns.
- Nothing secret in `web/`: every `VITE_*` value ends up in the build.

## Tests and checks

- Test logic first: pure functions with Vitest, forms and pages with Testing Library. Every feature module with pure
  logic (a `.ts` that is not a hook or `*-api.ts` and exports a function) has a test next to it.
- During work, run `npm test`, `npm run typecheck`, `npm run lint`, `npm run architecture:structure`; CI runs
  everything.
- A new app, framework, rendering or hosting approach, or state/transport approach gets an ADR in the same change.

## Rules and their checks

`npm run architecture:structure` runs `tools/check-structure.mjs`, whose rules live in `tools/architecture/` (folders,
files, imports, app); `tools/architecture/rules.test.mjs` (part of `npm test`) proves each rule catches a breach. CI
(`.github/workflows/web.yml`) runs every check below on each push and pull request
([ADR 0010](decisions/0010-every-rule-has-a-check.md)).

| Rule                                                                               | Check                                                                                                                                   |
| ---------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| SDD first, then code, then audit                                                   | review only: a process, not code; the PR shows the SDD and the audit                                                                    |
| `src/` holds only the entry and the four areas                                     | structure: `srcRootHoldsOnlyTheAreas`                                                                                                   |
| At most six production files per folder (tests, `index.ts`, `generated/` exempt)   | structure: `folderShape`                                                                                                                |
| Files or subfolders, no one-file, empty or wrapper folders                         | structure: `folderShape`                                                                                                                |
| No dump folder or file names                                                       | structure: `noDumpNames`                                                                                                                |
| Imports go main → app → features → platform → api-client                           | structure: `importDirection`                                                                                                            |
| A feature never imports another feature                                            | structure: `featuresAreIsolated`                                                                                                        |
| One component per file, named after it                                             | structure: `oneComponentPerFile`                                                                                                        |
| PascalCase components, kebab-case modules                                          | structure: `fileNames`                                                                                                                  |
| A hook lives in `use-*.ts` or `*-api.ts`                                           | structure: `hookFileNames`                                                                                                              |
| Component ≤ 150 lines, module ≤ 200                                                | structure: `lineLimits`                                                                                                                 |
| One responsibility per component under the limit                                   | review only: needs judgement (`frontend-code-audit`, signals: several data hooks plus several blocks, logic in a component)             |
| API client only from `platform/` and `*-api.ts`                                    | structure: `apiClientOnlyFromDataAccess`                                                                                                |
| The client instance only from `*-api.ts`                                           | structure: `clientInstanceOnlyFromDataAccess`                                                                                           |
| The API client through its index only                                              | structure: `apiClientThroughItsIndex`                                                                                                   |
| No raw `fetch`, Axios or second `openapi-fetch` client                             | ESLint: `no-restricted-globals` (`fetch`), `no-restricted-imports` (`axios`, `openapi-fetch` outside `src/api-client`)                  |
| `generated/` never edited                                                          | CI: `npm run contracts:check` (regenerates and diffs); Prettier and ESLint ignore it                                                    |
| Problem message shown, field errors under fields                                   | tests: `api-error.spec.ts`, form and page tests (`ExpensesPage.test.tsx`, `SettingsPage.test.tsx`)                                      |
| Submit disabled while pending                                                      | review only: a UI state; covered where a test retries (`ExpensesPage.test.tsx`), not provable for every form by a static rule           |
| One `Idempotency-Key` per attempt, reused on retry                                 | tests: `ExpensesPage.test.tsx` (key kept on retry), create tests of every page                                                          |
| Reads under the module prefix, writes refresh the prefix                           | tests: `write-hooks-api.test.tsx`, `summary-api.test.tsx`                                                                               |
| Queued writes: serialisable request, last write wins, deleted stays deleted        | tests: `queued-write-api.test.ts`, `use-queued-write.test.tsx`, the offline tests of `ListPage.test.tsx` and `ActiveTasksPage.test.tsx` |
| A change-only write has no create or delete                                        | TypeScript (`npm run typecheck`): `useChange` takes no create or delete                                                                 |
| Server state in React Query                                                        | review only: copies of server data in component state are not mechanically distinguishable from form state                              |
| Never indexed                                                                      | structure: `neverIndexed` (`index.html`, `public/robots.txt`)                                                                           |
| Only problems marked in lists                                                      | review only: wording and design judgement                                                                                               |
| Ukrainian UI text, phone first                                                     | review only: language and layout judgement; page tests query Ukrainian names                                                            |
| Theme tokens, no literal colours outside `app/theme/`                              | structure: `noLiteralColours`; the rest of [design.md](design.md) is review only                                                        |
| One interface language: locale only from `src/platform/client/locale.ts` (`uk-UA`) | structure: `oneLocale`                                                                                                                  |
| Nothing secret in `web/`                                                           | review only: secrets are not recognisable by shape; `.env` and `.env.local` are git-ignored                                             |
| Pure logic has a test next to it                                                   | structure: `logicHasTests`                                                                                                              |
| Type check, lint, format, tests, build                                             | CI: `npm run typecheck`, `npm run lint`, `npm run format:check`, `npm test`, `npm run build`                                            |
| A new lasting decision gets an ADR                                                 | review only: whether a decision is lasting needs judgement                                                                              |
