# 0014. The interface is Ukrainian, strings in place, one locale constant

Status: accepted, 2026-11-27

## Context

The site was written with Russian texts, the language of the old sheets and bots. Vitaliy asked for the whole interface
in Ukrainian (2026-11-27). Two people use the site; nobody asked for a language switch.

## Decision

- The interface language is Ukrainian. The texts are replaced in place, in the components and the `labels.ts` modules
  where they already live; no i18n library and no string catalogue.
- Every `Intl` formatter, plural rule and `localeCompare` takes its locale from `src/platform/locale.ts`
  (`uk-UA`); the structure rule `oneLocale` fails on any other locale literal in production code.
- The family's data (categories, departments, items, notes) is not translated. Texts the API writes come in Ukrainian
  from the API (api ADR 0037).
- The words come from one [glossary](../specs/platform/001-ukrainian-interface/glossary.md).

## Consequences

- No new dependency, no keys to keep in sync, tests keep querying visible names.
- A second language later would need a catalogue: the texts would move out of the components first. The locale
  constant is already in one place.
- Old Russian data stays Russian next to Ukrainian labels until the family renames it.
