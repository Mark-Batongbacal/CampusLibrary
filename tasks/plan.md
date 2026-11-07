# Implementation Plan: Members and Loans API (Milestone 4)

## Overview
Add Members CRUD and Loans read/borrow/return endpoints to CampusLibrary.WebApi. Enforce member/book existence, prevent borrowing a book that already has an open loan, prevent returning a loan twice, and preserve the existing SQL Server constraint as the final guard against concurrent borrow attempts.

## Architecture Decisions
- Add MembersController and LoansController using the existing constructor-injected EF Core LibraryDbContext and standard controller patterns
- Use request and response DTOs; never expose EF Core entities from HTTP endpoints
- Use ModelState/data annotations for request validation and map persistence conflicts to HTTP 409
- Keep existing database schema and migrations unchanged; do not drop, reset, or otherwise clear data
- Map SQL Server constraint violations (2601/2627 for unique, 547 for FK) to appropriate HTTP status codes
- Server-side UTC timestamps for borrowed/returned; clients do not supply them
- Filtered unique index on Loan.BookId where ReturnedAt IS NULL enforces at most one open loan per book

## Dependency Graph
Existing entities (Book, Member, Loan) + LibraryDbContext → DTO validation and mapping → controller endpoints → focused API tests → live SQL smoke checks (when database is available)

## Task List

### Phase 1: Members Controller and DTOs
- [ ] Task 1: Add Member request/response DTOs and validation (duplicate email 409)
- [ ] Task 2: Implement MembersController with all CRUD routes and status codes

### Phase 2: Loans Controller and DTOs
- [ ] Task 3: Add Loan request/response DTOs and borrowing/return request models
- [ ] Task 4: Implement LoansController with list, get, borrow, and return routes

### Phase 3: Tests and Verification
- [ ] Task 5: Add xUnit tests covering Members CRUD and Loan borrow/return outcomes
- [ ] Task 6: Run restore, build, tests, inspect the diff, and smoke-test the running API/database if available

### Checkpoint: Complete
- [ ] All acceptance criteria in SPEC-milestone4-members-loans-api.md are met.
- [ ] Final review completed and reports written.

## Architecture Decisions
- Add one `BooksController` using the existing constructor-injected EF Core `LibraryDbContext` and standard controller patterns.
- Use request and response DTOs; never expose EF Core entities from HTTP endpoints.
- Use `ModelState`/data annotations for request validation and map persistence conflicts to HTTP 409.
- Keep existing database schema and migrations unchanged; do not drop, reset, or otherwise clear data.
- Test controller behavior with a provider-independent approach if already available; avoid new dependencies unless existing providers cannot provide meaningful checks.

## Dependency Graph
Existing `Book` entity + `LibraryDbContext` → DTO validation and mapping → controller endpoints → focused API tests → live SQL smoke checks (when database is available).

## Task List

### Phase 1: API contract and endpoints
- [x] Task 1: Add book request/response DTOs and validation.
- [x] Task 2: Implement Books CRUD controller with status codes and conflict handling.

### Checkpoint: API contract
- [ ] Controller exposes all five specified routes and does not return EF entities.
- [ ] Build succeeds; no schema or migration changes.

### Phase 2: Tests and verification
- [x] Task 3: Add focused tests covering validation and CRUD success/not-found/conflict behavior.
- [x] Task 4: Run restore, build, tests, inspect the diff, and smoke-test the running API/database if available.

### Checkpoint: Complete
- [ ] All acceptance criteria in `SPEC-books-api.md` are met.
- [ ] Final review completed and reports written.

## Risks and Mitigations
| Risk | Impact | Mitigation |
|---|---|---|
| Unique ISBN race reaches SQL unique index | Duplicate create/update could return 500 | Catch only expected SQL Server unique-key violations and return 409; preserve unrelated exceptions. |
| Delete blocked by existing loan FK | Client could receive 500 | Map the relevant FK constraint violation to 409. Do not delete related loans. |
| Controller tests use in-memory EF behavior unlike SQL Server | Filter/provider-specific constraints may not be exercised | Test controller-level behavior and separately verify constraint conflicts against the existing local SQL Server if available. |
| Prior setup differs from the spec assumptions | References/configuration might be inconsistent | Keep changes limited to API/tests and report any discovered unrelated issue rather than broad refactoring. |

## Open Questions
None. The user approved the spec by asking to proceed; implementation will begin after plan review.