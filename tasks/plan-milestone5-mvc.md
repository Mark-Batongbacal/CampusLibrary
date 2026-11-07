# Implementation Plan: Milestone 5 — MVC Librarian Admin

## Overview
Build the specified server-rendered MVC librarian workflows on the existing EF Core context, with typed view models, server validation, antiforgery protection, a responsive shared UI, and focused tests. No schema or API changes are planned.

## Architecture Decisions
- Use `LibraryDbContext` directly in MVC controllers, as required by `CLAUDE.md`.
- Keep MVC view models separate from EF entities and API DTOs.
- Implement vertical Books, Members, and Loans slices, then shared navigation and verification.
- Use existing dependencies; no extra service layer or frontend framework.
- Present expected database conflicts without leaking exception details.
- Authentication is not defined and remains explicitly out of scope.
- UI direction: Metro Typographic Tiles, with ordinary semantic tables/forms as primary task surfaces and the selected palette/type as restrained hierarchy; user requested table-first catalog.

## Dependency Graph
Existing entities and `LibraryDbContext` → feature view models/controllers → Razor views → focused tests → shared navigation/UI review → full build/test and reports.

## Task List
- [ ] Books MVC list/create/edit/delete with typed models, validation, conflict handling, views, tests.
- [ ] Members list/create/details with typed models, validation, duplicate-email handling, views, tests.
- [ ] Open loans list and return with user-visible failure states, antiforgery, tests.
- [ ] Shared landing page, navigation, responsive accessible presentation.
- [ ] Full verification, review, and required reports.

## Risks and Mitigations
| Risk | Impact | Mitigation |
|---|---|---|
| SQL Server unique/FK behavior differs from EF InMemory | Conflict paths may be untested | Catch only expected codes; report live SQL smoke status accurately. |
| No authentication exists | Admin pages are unsuitable for public production deployment | Disclose limitation; don't add authentication without separate approval. |
| No existing MVC visual system | Inconsistent UI | Follow the approved surface direction and existing project UI requirements without inventing product branding. |
| Concurrent edits from delegated workers | Conflicts or unreviewed modifications | Keep exclusive file ownership; coordinator reviews final tree and tests. |

## Open Questions
None. User approved the spec and selected the Metro Typographic Tiles UI direction; table-first emphasis is confirmed.
