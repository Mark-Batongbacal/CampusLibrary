# Milestone 4 — Members and Loans API

## Summary of the Request
Implement Members CRUD and Loans read/borrow/return endpoints in `CampusLibrary.WebApi`. Enforce member/book existence, prevent borrowing a book that already has an open loan, prevent returning a loan twice, and preserve the existing SQL Server filtered unique index as the final guard against concurrent borrow attempts.

## Work Performed
1. Added Member DTOs (`MemberDto`, `CreateMemberRequest`, `UpdateMemberRequest`) with required field and email validation.
2. Added Loan DTOs (`LoanDto`, `BookSummaryDto`, `MemberSummaryDto`, `BorrowBookRequest`, `ReturnBookRequest`).
3. Implemented `MembersController` with GET collection/item, POST, PUT, DELETE routes including duplicate email (409), missing resource (404), delete-with-loan-history (409), and FK conflict handling.
4. Implemented `LoansController` with GET collection/item, POST borrow, POST return routes enforcing member/book existence checks, open-loan prevention (409), already-returned rejection (409), and server-side UTC timestamps.
5. Added `MembersLoansControllerTests.cs` covering all Members CRUD success/failure paths and Loan list/get/borrow/return outcomes.
6. Ran the focused test project and full solution build after narrowing persistence exception handling; both passed (details below).
7. The implementation-stage report records live smoke tests against Docker SQL Server through API on port 5102. Those smoke tests were not rerun during the review-fix pass.

## Projects Affected
- `CampusLibrary.WebApi`
- `CampusLibrary.Tests`

## Files Added, Modified, or Deleted

| File | Change | Reason |
|---|---|---|
| `src/CampusLibrary.WebApi/DTOs/MemberDto.cs` | Added | Safe response DTO for members |
| `src/CampusLibrary.WebApi/DTOs/CreateMemberRequest.cs` | Added | Validated member creation input |
| `src/CampusLibrary.WebApi/DTOs/UpdateMemberRequest.cs` | Added | Validated member update input |
| `src/CampusLibrary.WebApi/Controllers/MembersController.cs` | Added, then narrowed persistence conflict handling | Implements all five Members CRUD routes; maps only expected SQL constraint violations to 409 |
| `src/CampusLibrary.WebApi/DTOs/LoanDto.cs` | Added | Loan response DTO with book/member summary details |
| `src/CampusLibrary.WebApi/DTOs/BorrowBookRequest.cs` | Added | Validated borrow input |
| `src/CampusLibrary.WebApi/DTOs/ReturnBookRequest.cs` | Added | Validated return input |
| `src/CampusLibrary.WebApi/Controllers/LoansController.cs` | Added, then narrowed persistence conflict handling | Implements loan list/get/borrow/return routes; maps only expected SQL constraint violations to 409 |
| `tests/CampusLibrary.Tests/MembersLoansControllerTests.cs` | Added | Tests covering Members and Loans endpoint behavior |
| `tasks/plan.md` | Modified | Replaced Milestone 3 plan with Milestone 4 plan |
| `tasks/todo.md` | Modified | Updated task list for Milestone 4 |
| `docs/reports/INDEX.md` | Modified | Appends this report entry |
| `docs/reports/2026-11-07-2116-milestone4-members-loans-api/DEVELOPER-DOCS.md` | Modified | Corrected conflict handling and final verification details after review |
| `docs/reports/2026-11-07-2116-milestone4-members-loans-api/FLOWCHARTS.md` | Modified | Updated persistence failure and SQL constraint paths to match implementation |

## Important Implementation Details
- `GET /api/members` and `GET /api/members/{id}` return `MemberDto`; missing ID returns 404.
- `POST /api/members` validates input, checks duplicate email via `AnyAsync`, catches SQL unique-violation (2601/2627) returning 409, returns 201 with `Location`.
- `PUT /api/members/{id}` returns 204 on success, 400 on invalid input, 404 when missing, 409 on duplicate email.
- `DELETE /api/members/{id}` returns 204, 404 when missing, 409 when loan history exists. Loan history is preserved; only SQL Server FK error 547 is mapped to a constraint conflict.
- `GET /api/loans` and `GET /api/loans/{id}` return `LoanDto` including IDs, timestamps, and book/member summary fields; no EF entities returned.
- `POST /api/loans` checks member exists, book exists, no open loan for book; creates loan with `DateTime.UtcNow` for `BorrowedAt`; maps SQL Server unique-violation (2601/2627) on borrow race to 409.
- `POST /api/loans/{id}/return` sets `ReturnedAt = DateTime.UtcNow`; missing loan returns 404; already-returned returns 409. Only SQL Server FK error 547 is mapped to a constraint conflict.
- Timestamps are set server-side in UTC; clients do not submit them.
- The existing filtered unique index `IX_Loans_BookId` where `ReturnedAt IS NULL` remains the final concurrency guard; no schema changes were made.
- EF Core InMemory is used for isolated tests; tests do not delete or reset shared database data.

## Database Changes
None. No migrations were added or applied. The existing schema, migrations, and seed data remain unchanged.

## Build and Test Results
- `dotnet test tests/CampusLibrary.Tests/CampusLibrary.Tests.csproj --no-restore` — passed, 33 tests, 0 failed, 0 skipped (after narrowing persistence conflict handling).
- `dotnet build CampusLibrary.sln --no-restore` — passed, 0 warnings, 0 errors (after narrowing persistence conflict handling).
- The implementation-stage report recorded live API smoke tests against Docker SQL Server for the endpoints and conflicts listed there. They were not rerun independently during this review-fix pass; see the implementation notes above.

## Known Limitations
- Unit tests use EF Core InMemory, which does not enforce the filtered unique index; concurrent borrow race mapping to 409 was verified via SQL Server live requests.
- List endpoint is unpaginated.
- No known build limitations remain from the final verification; EF Core InMemory tests do not enforce SQL Server filtered-index behavior.
- SQL Server-specific conflict mapping (2601/2627 for unique constraints and 547 for foreign keys) is filtered by SQL error number, following the Books API pattern. EF Core InMemory does not raise SqlException, so those SQL-specific branches are not covered by unit tests. Unexpected database failures are not translated to 409 by these handlers.

## Follow-up Tasks
- Milestone 5: MVC Admin interface for Books, Members, Loans.
- Consider pagination for list endpoints when catalogue grows.
- Consider adding dedicated SQL-backed integration tests for the filtered unique index concurrency guard.