# 0015. Expenses are readable and editable without sign-in until the Identity module

Status: superseded by [0037](0037-google-sign-in-with-server-sessions.md) on 2026-11-27 (was temporary; amended 2026-11-26 to allow writes)

## Context

The site has no authentication yet ([0009](README.md) is still open). The Expenses module serves the family's real
incomes, expenses and savings. Vitaliy decided to build the module and import the data first and to add sign-in as
a later, separate feature. The first version of this record kept writes closed; to stop using the sheet, the family
needs to add and correct entries on the site now, and Vitaliy, as the owner, accepted open writes until Google
sign-in exists.

## Decision

- All Expenses endpoints are anonymous for now: reads, and creating, editing and deleting expenses, incomes and
  savings movements. The site is not indexed and its addresses are not published.
- Mitigation while it is open:
  - every write uses the `PublicWrite` rate limit ([0012](0012-rate-limits-on-anonymous-writes.md)), 60 requests a
    minute per client address;
  - the API runs on a long, unguessable App Service host name that appears nowhere public; CORS admits only the web
    app;
  - creates are idempotent ([0007](0007-idempotency-key-for-creates.md)) and edits are version-checked
    ([0017](0017-row-version-via-xmin-for-edits.md)), so retries and two phones do not corrupt data;
  - the imported history can be loaded again from the import scripts, which skip rows already stored
    ([0014](0014-one-time-sheet-import.md)); entries added on the site have no backup beyond the database host's own.
- The keyed import stays closed; its key is unset in production.

## Consequences

- A known, accepted risk until sign-in exists: anyone who finds the API address can read, change or delete the
  family's financial records, at the rate limit's pace. The rate limit only slows damage, it does not prevent it.
- Sign-in with Google (the Identity module) is the next step. It must put every Expenses endpoint behind
  authorization in the same change that introduces it, and this record then becomes `superseded`.
- Entries carry no author yet; who created or changed a row can be recorded once there is a signed-in user.
