# Audit — 001 Ukrainian texts (API)

Final audit against this SDD, [rules](../../../rules.md) and the obsidian `backend-code-audit` checklist, 2026-11-27.

| Area | Result |
| --- | --- |
| Bot (FR-001, FR-006, FR-007) | Digest texts, gift reminders, `/start` replies and buttons, test message Ukrainian; money and months `uk-UA`; callback data and commands unchanged (new `BotTextsTests` checks it). Names addressed in the vocative («Доброго ранку, Віталію!») through `TextFormat.Address`. |
| Schedule (FR-002) | `DigestSchedule` titles and times Ukrainian. |
| Problems (FR-003) | Domain `*Errors`, validators, shared problem titles and the rate-limit text Ukrainian (they were English before); FluentValidation's built-in messages set to `uk`. Error codes unchanged. Developer-facing import messages stay English. |
| Data (FR-004, FR-005) | Gift to-do title «Купити подарунок»; seeds, migrations, sheet import and shopping parser untouched. |
| Layers | Texts stay in the classes that held them; no new dependency, no layer change (architecture tests 77/77). |
| Contract | `openapi.json` regenerated: three description strings, no shape change. |
| Tests | Unit 373/373, architecture 77/77, integration 105/105 (run locally with Docker). |

Findings: none open.
