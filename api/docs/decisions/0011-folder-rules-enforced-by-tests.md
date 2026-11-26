# 0011. Folder rules enforced by tests

Status: accepted, 2026-11-16

## Context

Projects grow folders named `Services`, `Helpers` or `Shared` that collect unrelated code, and folders with dozens
of files where nothing can be found.

## Decision

Folders are named by feature or aggregate, never by kind of class. A folder holds at most six code files; a
seventh splits it by meaning into subfolders of at least two files. No empty, wrapper or new one-file folders.
Generated migrations are exempt. Architecture tests check all of it in `src/` and `tests/`.

## Consequences

- Code for one feature stays together and is easy to find.
- Growing features must be reorganized early, while it is still cheap.
