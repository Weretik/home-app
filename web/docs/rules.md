# Web rules

## Structure

- Code is grouped by feature (`features/shopping/`), then by role inside a feature when it grows (`list/`, `editor/`,
  `data/`). Framework wiring lives in `app/`, environment and clients in `platform/`. **(checked)**
- **A folder holds at most six production files** (tests next to them and `index.ts` do not count; generated code is
  exempt). The seventh file splits the folder by meaning, never one folder per file. **(checked)**
- A folder holds either files or subfolders, not both, and never a single file alone. **(checked)**
- No dump folders or files: `utils`, `helpers`, `common`, `shared`, `types`, `constants`, `lib`. Name things by what
  they do (`format-amount.ts`). **(checked)**
- A feature never imports another feature. **(checked)**
- One component or one responsibility per file; component files are PascalCase, the rest kebab-case.

## API

- Call the API only through the client in `src/platform/api.ts`, from the feature's `<area>-api.ts`; no raw `fetch`
  or Axios. **(checked)**
- Never edit `src/api-client/generated/`; regenerate it with `npm run contracts:generate` after the backend contract
  changes. **(checked in CI)**
- Show the problem response's message to the user and map field errors onto form fields.

## Writes

- Every mutation disables its submit while pending.
- A write the backend protects with `Idempotency-Key` makes one key per attempt, reuses it on retry, and renews it
  after success or a change of the data.

## App

- Server state in React Query, not in component state copies.
- The app is never indexed.
- Mark only problems in lists, with the reason; no "OK" badges.
- UI text in Russian; layouts work on a phone first.
- Every screen follows the [design rules](design.md): theme tokens only, no literal colours, both colour schemes,
  the shell's menus, the card patterns.

## Tests and checks

- Test logic first: pure functions with Vitest, forms and pages with Testing Library.
- During work, run `npm test`, `npm run typecheck`, `npm run lint`; CI runs everything.
- A new app, framework, rendering or hosting approach, or state/transport approach gets an ADR in the same change.
