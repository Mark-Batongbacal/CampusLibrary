# Spec: Members and Loans API (Milestone 4)

## Objective
Add Members CRUD and Loans read/borrow/return endpoints to `CampusLibrary.WebApi`. Enforce member/book existence, prevent borrowing a book that already has an open loan, prevent returning a loan twice, and preserve the existing SQL Server constraint as the final guard against concurrent borrow attempts.

## Assumptions
1. The existing .NET 9, ASP.NET Core controller, EF Core 9, and SQL Server architecture remains authoritative.
2. Members get the CRUD route set documented in `CLAUDE.md`: list, get by ID, create, update, delete. Loans get list, get by ID, create/borrow, and return routes.
3. Deleting a member with any loan history is rejected with HTTP 409 because the configured foreign key uses `DeleteBehavior.Restrict`; loans are not implicitly deleted.
4. `GET /api/loans` and `GET /api/loans/{id}` include both open and returned loans. Loan DTOs expose IDs, borrowed/returned timestamps, and book/member summary details, but not EF entities.
5. Borrowing uses the existing rule of at most one open loan per book. `AvailableCopies` is not decremented: this follows the explicit Milestone 4 borrowing rules and the existing filtered unique index, even though copy counts may suggest a broader inventory model. Revisit that model in a separate approved change if the intended behavior is multiple simultaneous copies.
6. Borrowed/returned timestamps are set by the server in UTC; clients do not submit them when borrowing or returning.
7. Existing schema/migrations should suffice. If implementation demonstrates a necessary schema change or new package is needed, pause and request approval before applying it.
8. Automated tests should use isolated data and must not delete/reset the shared development database. SQL Server-specific concurrency/constraint behavior should be integration-tested only if it can be done safely with isolated test data.

## Tech Stack
- ASP.NET Core Web API, .NET 9
- EF Core SQL Server 9
- Existing controller and DTO conventions
- Existing xUnit test project and EF Core InMemory provider

## Commands
- Restore: `dotnet restore`
- Build: `dotnet build`
- Tests: `dotnet test`
- Run API: `ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/CampusLibrary.WebApi`
- Database service, when needed: `docker compose up -d`
- No migration command is expected unless an approved schema change becomes necessary.

## Project Structure
- `src/CampusLibrary.WebApi/Controllers/MembersController.cs` — Members CRUD routes
- `src/CampusLibrary.WebApi/Controllers/LoansController.cs` — loan list/get/borrow/return routes
- `src/CampusLibrary.WebApi/DTOs/` — member/loan request and response DTOs
- `src/CampusLibrary.Data/` — existing entities, context, and constraints; unchanged unless approved otherwise
- `tests/CampusLibrary.Tests/` — focused API behavior tests
- `docs/reports/` — required implementation reports

## Code Style
Follow `BooksController`: controller attributes, constructor-injected `LibraryDbContext`, asynchronous EF operations, DTO-only HTTP responses, and narrow handling of expected SQL constraint errors. Example borrowing response mapping:

```csharp
var loan = await db.Loans
    .AsNoTracking()
    .Where(loan => loan.Id == id)
    .Select(loan => new LoanDto(/* public fields only */))
    .FirstOrDefaultAsync();
```

## Testing Strategy
- Add isolated xUnit tests for Members CRUD success, validation, not-found, duplicate-email, and delete-with-loan outcomes.
- Add loan tests for list/get, unknown member/book, borrowing an available book, conflict when the book already has an open loan, returning an open loan, returning an already-returned loan, and unknown loan.
- Verify status codes and persisted state, not implementation call sequences.
- Test SQL Server filtered-unique-index behavior against an isolated development fixture only if safe setup is available; otherwise document that limitation and retain the database constraint as the concurrency guard.
- Run `dotnet restore`, `dotnet build`, and `dotnet test`; record actual outcomes.
- Do not reset or delete shared database data.

## Boundaries
- **Always:** Use DTOs; validate inputs; make EF operations asynchronous; return predictable 400/404/409 responses; check both member and book before borrowing; use the existing filtered unique index to protect concurrent borrowing; preserve database records; report actual test results.
- **Ask first:** Any schema/migration change, database reset/deletion, additional dependency, authentication, member-specific loan filtering, or behavior beyond the listed routes.
- **Never:** Return EF entities directly; delete loan history as a side effect of member/book deletion; accept client-supplied loan timestamps; claim a database or integration check ran when it did not.

## Success Criteria
- `GET /api/members` and `GET /api/members/{id}` return member DTOs or 404 for a missing ID.
- `POST /api/members` validates and creates a member, returning 201; duplicate email returns 409.
- `PUT /api/members/{id}` validates and updates, returning 204, 400, or 404; duplicate email returns 409.
- `DELETE /api/members/{id}` returns 204, 404, or 409 when loan history prevents deletion.
- `GET /api/loans` and `GET /api/loans/{id}` return loan DTOs or 404 for a missing ID.
- `POST /api/loans` verifies member and book existence, rejects a book with an open loan using 409, and returns 201 with the created loan DTO and resource location.
- Concurrent attempts cannot create multiple open loans for one book; the database filtered unique index remains the final enforcement and its violation maps to 409.
- `POST /api/loans/{id}/return` sets `ReturnedAt` for an open loan and returns 204; missing loans return 404 and repeated returns return 409.
- Tests cover the specified success/failure behavior; build and test commands succeed.
- No schema change, UI, authentication, or unrelated endpoints are added without approval.

## Open Questions
None blocking. The `AvailableCopies` versus one-open-loan-per-book modeling limitation is explicitly treated as a scope assumption above, not changed in this milestone.
