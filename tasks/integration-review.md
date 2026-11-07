# Milestone 8 integration review — API and data

## Scope and findings

Reviewed the existing API controllers, DTOs, Data relationship mapping, migration snapshot, and test suite using local Karpathy Guidelines, Ponytail, and Agent Skills specification guidance. Existing milestone requirements authorize this focused repair work; no extra approval gate was introduced.

Confirmed and repaired:

- Book availability was inventory-only, so a borrowed book appeared available. `BookDto.IsAvailable` now derives from positive inventory and absence of an open loan in both list and detail. `AvailableCopies` retains the existing stored inventory meaning; at most one open loan is allowed regardless of that value. Borrowing zero inventory returns 409.
- A one-to-one Book/Loan EF relationship conflicted with returned-loan history and reborrowing. `Book.Loans` is now a collection with a one-to-many relationship. The SQL filtered unique index remains the authority for one open loan per book.
- MVC book deletion's singular navigation reference now queries open loans directly.
- Returning now rejects a body/route ID mismatch and uses a conditional atomic update in relational databases, preventing concurrent returns from overwriting the first timestamp. The existing InMemory test provider uses entity updates.
- Positive integer ID validation uses Range rather than Required (Required accepts zero on non-nullable integers).
- Development API HTTP no longer attempts HTTPS redirection, supporting emulator local networking. Non-development HTTPS redirection remains enabled.
- Nullable test/API warnings encountered during verification were corrected.

## Changed files

| File | Change | Reason |
|---|---|---|
| src/CampusLibrary.Data/Book.cs | Singular Loan changed to Loans collection | Preserve returned history during reborrowing |
| src/CampusLibrary.Data/LibraryDbContext.cs | One-to-many mapping | Align EF with existing SQL invariant |
| src/CampusLibrary.Data/Migrations/20261107152619_CorrectBookLoanRelationship.cs | Added empty migration | Record metadata correction; no SQL schema operations |
| src/CampusLibrary.Data/Migrations/20261107152619_CorrectBookLoanRelationship.Designer.cs | Added generated model | Snapshot of corrected relationship |
| src/CampusLibrary.Data/Migrations/LibraryDbContextModelSnapshot.cs | Updated | Keep EF model consistent |
| src/CampusLibrary.WebApi/DTOs/BookDto.cs | Added IsAvailable | Correct Android availability |
| src/CampusLibrary.WebApi/Controllers/BooksController.cs | Derived availability queries | Account for open loans |
| src/CampusLibrary.WebApi/Controllers/LoansController.cs | Inventory guard, atomic returns, ID guard, warning fixes | Integration correctness |
| src/CampusLibrary.WebApi/DTOs/BorrowBookRequest.cs | Positive ID validation | Reject invalid input with 400 |
| src/CampusLibrary.WebApi/DTOs/ReturnBookRequest.cs | Positive ID validation | Reject invalid input with 400 |
| src/CampusLibrary.WebApi/Program.cs | Development HTTP behavior | Emulator connectivity |
| src/CampusLibrary.WebMvc/Controllers/BooksController.cs | Removed singular navigation usage | Match Data relationship |
| tests/CampusLibrary.Tests/BooksControllerTests.cs | Availability tests | Verify open-loan and return behavior |
| tests/CampusLibrary.Tests/MembersLoansControllerTests.cs | Borrow, validation, reborrow/history, return tests | Verify repaired behavior |
| tests/CampusLibrary.Tests/LoansMvcControllerTests.cs | Nullable warning correction | Clean build |

## Executed verification

- `dotnet restore`: passed.
- `dotnet build`: final run passed, 0 warnings, 0 errors. Earlier build had 8 nullable warnings; these were resolved.
- `dotnet test`: final run 69 passed, 0 failed, 0 skipped.
- `docker compose ps`: existing campuslibrary-mssql container up; host binding 127.0.0.1:1433.
- `dotnet ef migrations add CorrectBookLoanRelationship -p src/CampusLibrary.Data -s src/CampusLibrary.WebApi`: passed; reviewed generated Up/Down, both empty.
- `dotnet ef database update -p src/CampusLibrary.Data -s src/CampusLibrary.WebApi`: passed; added migration history entry only.
- `dotnet ef migrations has-pending-model-changes -p src/CampusLibrary.Data -s src/CampusLibrary.WebApi`: passed, no changes since last migration.
- API launched with `ASPNETCORE_ENVIRONMENT=Development dotnet run --no-build --project src/CampusLibrary.WebApi --urls http://0.0.0.0:5080`.
- Python urllib HTTP probes: GET /api/books (5), /api/books/1, /api/members (3), /api/loans (2), /swagger/v1/swagger.json all 200. POST invalid zero IDs 400, unknown resource IDs 404, mismatched return IDs 400. No API probe modified records.
- SQL Server `sys.indexes` query verified IX_Loans_BookId is unique and filtered by ReturnedAt IS NULL.
- SQL transactional insert/update probe verified duplicate open loans rejected with SQL 2601/2627, and returned history plus a new open loan accepted. All probe data rolled back. SQL identity counters may advance after rollback; existing records remain unchanged.

## Failed exploratory checks and resolution

- Initial HTTP probe expected zero IDs to yield 400 but received 404; this exposed the Required/int bug. Range validation added, tests and repeated HTTP probe now pass.
- Initial sqlcmd probe failed because QUOTED_IDENTIFIER was disabled by its default session. Repeated with sqlcmd `-I`; transaction probe passed. No persisted probe records.

## Limits

Controller tests primarily use EF InMemory; live SQL checks additionally verified translated read queries and the filtered index. A simultaneous HTTP borrow race was not executed against the shared development database to avoid persisting test loans. Authentication remains outside scope, so member selection is development identification, not authorization. MVC browser and Android emulator visual review belong to the main task. No destructive operations or data resets performed.
