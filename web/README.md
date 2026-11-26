# Web

The family app: a React single-page app built with Vite, Material UI and React Query, calling the API through a
client generated from the backend contract. It follows the frontend of
[fullstack-modular-template](https://github.com/Weretik/fullstack-modular-template) without Nx.

- [Architecture](docs/architecture.md): folders, the API client, how data flows.
- [Rules](docs/rules.md): the conventions every change follows, each with the check that enforces it in CI.
- [Design](docs/design.md): theme, light/dark/system, the shell and card patterns every screen follows.
- [Decisions](docs/decisions/README.md): why it is built this way.
- [Specifications (SDD)](docs/specs/README.md): each feature's scenarios, design and tasks, written before the code.

## Commands

You need Node.js 22.

| What                                                               | Command                                            |
| ------------------------------------------------------------------ | -------------------------------------------------- |
| Install                                                            | `npm ci`                                           |
| Dev server on http://localhost:5173 (API on http://localhost:5000) | `cp .env.example .env.local && npm run dev`        |
| Regenerate the API client after a backend contract change          | `npm run contracts:generate`                       |
| Check the client matches the contract                              | `npm run contracts:check`                          |
| Type check, lint, tests                                            | `npm run typecheck`, `npm run lint`, `npm test`    |
| Folder, file, naming and import boundary rules                     | `npm run architecture:structure`                   |
| Code style                                                         | `npm run format:check` (`npm run format` fixes it) |
| Production build into `dist/`                                      | `npm run build`                                    |

## Environment variables

| Name           | Meaning                                                                                                                             |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| `VITE_API_URL` | The API origin without a trailing slash, baked in at build time. Public, not a secret. CI sets it to the Azure App Service address. |
