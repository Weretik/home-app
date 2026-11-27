# Notes — tasks and traceability

Every behaviour task follows the cycle of the template: smallest focused test → red → minimal code → green →
refactor → affected regression run. Evidence is the CI run of the pull request.

| ID | Responsibility | Paths | Test level | Covers |
| --- | --- | --- | --- | --- |
| EN-001 | Five projects, solution folders, `NotesModule`, registration in `Program.cs` and `MediatorSetup` | `src/Modules/Notes/*`, `src/Host/Host.Api` | architecture tests | — |
| EN-002 | `ICurrentMember` in `Identity.Contracts`, implemented in `Identity.Api` | `src/Modules/Identity/*` | unit + integration | R-008 |
| TS-001 | `Note` aggregate: create, update, checks | `Notes.Domain/Notes/` | unit | R-001…R-005, R-007, R-008 |
| TS-002 | `NoteCategory`: create, rename, emoji | `Notes.Domain/Categories/` | unit | R-011 |
| TS-003 | Persistence: `NotesDbContext`, configurations, seed, `Initial` migration, indexes | `Notes.Infrastructure/` | integration | R-006 |
| TS-004 | Create / update / delete note, with idempotency, versions and the author from the session | `Notes.Application/Features/Notes/Commands/` + endpoints | unit (validators) + integration | R-008…R-010, SC-001, SC-003, SC-004 |
| TS-005 | List with filters, search and order | `Notes.Application/Features/Notes/Queries/` | unit (validator) + integration | R-012, R-013, SC-002, SC-006 |
| TS-006 | Categories: list (with note counts), create, update, delete | `Notes.Application/Features/Categories/` | unit + integration | R-006, R-011, SC-005 |
| TS-007 | Access: 401 without a session, scheduler key reads only | endpoints | integration (the identity sweeps pick up the new reads) | R-014, SC-007 |
| TS-008 | Contract: `WithName`, `WithSummary`, `Produces…`, regenerated `openapi.json` | `Notes.Api/Endpoints/` | architecture (OpenAPI) tests | — |
| TS-009 | Deploy: migrations bundle and smoke reads in `.github/workflows/api.yml` | workflow | CI | R-014 |
| TS-010 | Docs: module page, ADRs 0039 and 0040, C4, README and AGENTS.md module lists | `api/docs/`, `AGENTS.md`, `README.md` | review | — |

## Readiness

- [x] Rules and scenarios have ids and are testable.
- [x] Contract listed per operation with errors.
- [x] Schema and migration order known; the migration only adds a schema, so the previous release keeps working.
- [x] Out of scope written down (private notes, photos, summary, import).

## Delivery readiness (filled before merge)

- [ ] Unit, architecture, integration tests green.
- [ ] `openapi.json` regenerated, web client regenerated.
- [ ] Backend code audit done, findings fixed or recorded.
