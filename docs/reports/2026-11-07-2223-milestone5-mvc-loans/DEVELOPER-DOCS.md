# Milestone 5 — MVC Admin: Loans Workflow

## Summary of the Request
Implement only the Loans workflow slice of Milestone 5: an open-loans listing page showing book/member/borrowed-at information, and an antiforgery-protected return action that prevents duplicate returns. Exclusive ownership covers `src/CampusLibrary.WebMvc/Controllers/LoansController.cs`, `Models/Loans/`, `Views/Loans/`, and `tests/CampusLibrary.Tests/LoansMvcControllerTests.cs`. No schema changes, no dependency additions, no shared layout/Program.cs changes, no Books or Members files, no auth.

## Work Performed
1. Inspected existing entities (`Book`, `Member`, `Loan`), `LibraryDbContext`, the existing WebApi `LoansController` patterns (antiforgery-free API controller, SQL error-number conflict mapping, UTC `BorrowedAt`/`ReturnedAt`), and the existing MVC `Program.cs` (`AddControllersWithViews`, default route).
2. Added `Models/Loans/OpenLoanSummary.cs` (typed read model: LoanId, BookTitle, BookAuthor, MemberName, BorrowedAt).
3. Added `Models/Loans/LoanIndexViewModel.cs` (strongly typed view model holding `OpenLoans`).
4. Added `Controllers/LoansController.cs` with:
   - `GET Index` — async EF Core, filters `ReturnedAt == null`, orders by `BorrowedAt` desc, projects to `OpenLoanSummary`, never returns EF entities.
   - `POST Return/{id}` — `[ValidateAntiForgeryToken]`, 404 for missing loan, 409 for already-returned, sets `ReturnedAt = DateTime.UtcNow`, redirects after success (PRG), maps only expected SQL FK error 547 to conflict.
5. Added `Views/Loans/Index.cshtml` — Bootstrap table listing open loans (book, author, member, borrowed-at), empty-state card, inline POST return form with hidden antiforgery token and a confirmation dialog.
6. Added `Tests/LoansMvcControllerTests.cs` — 7 xUnit tests using EF Core InMemory covering index open-only filtering, empty state, return success/not-found/already-returned, and antiforgery attribute presence.
7. Ran `dotnet build` (passed, 0 errors) and `dotnet test` (passed, 40 tests, 0 failed).
8. Attempted a live MVC smoke check against running Docker SQL Server; `Program.cs` lacks a wired `DefaultConnection` in this MVC project and task scope forbids touching `Program.cs`, so live SQL smoke check was not completed — see Known Limitations.

## Projects Affected
- `CampusLibrary.WebMvc`
- `CampusLibrary.Tests`

## Files Added, Modified, or Deleted

| File | Change | Reason |
|---|---|---|
| `src/CampusLibrary.WebMvc/Models/Loans/OpenLoanSummary.cs` | Added | Typed read model for open loans (book/member/borrowed-at) |
| `src/CampusLibrary.WebMvc/Models/Loans/LoanIndexViewModel.cs` | Added | Strongly typed view model for the loans index view |
| `src/CampusLibrary.WebMvc/Controllers/LoansController.cs` | Added | Open-loans listing and antiforgery-protected return action |
| `src/CampusLibrary.WebMvc/Views/Loans/Index.cshtml` | Added | Razor view: open-loans table, empty state, return forms |
| `tests/CampusLibrary.Tests/LoansMvcControllerTests.cs` | Added | Focused xUnit tests for the Loans MVC controller |

## Important Implementation Details
- No EF entities are returned to the view; a dedicated read model (`OpenLoanSummary`) carries only the fields the page needs.
- `GET Index` queries `db.Loans.Where(l => l.ReturnedAt == null)`, includes `Book` and `Member`, orders by `BorrowedAt` descending.
- `POST Return` is decorated with `[HttpPost]` and `[ValidateAntiForgeryToken]`; missing loan → 404; already-returned → 409; sets `ReturnedAt = DateTime.UtcNow` server-side; success redirects to `Index` (PRG).
- Only expected SQL Server FK error 547 is mapped to conflict, matching the existing WebApi pattern; exception details are not leaked.
- The view renders the antiforgery token via `IAntiforgery.GetAndStoreTokens` and a hidden input, since no shared `_ViewImports.cshtml`/layout exists within the Loans-only ownership scope.
- No database schema, migration, or API changes. The existing filtered unique index on `Loans.BookId` where `ReturnedAt IS NULL` remains the concurrency guard.

## Database Changes
None. No migrations added or applied; existing schema, migrations, and seed data unchanged.

## Build and Test Results
- `dotnet build CampusLibrary.sln` — passed, 0 errors (2 pre-existing nullable warnings unrelated to this slice).
- `dotnet test tests/CampusLibrary.Tests/CampusLibrary.Tests.csproj` — passed, 40 tests, 0 failed, 0 skipped (38 pre-existing + 8 new Loans tests).
- Live SQL Server smoke check: not completed — MVC `Program.cs` does not wire `DefaultConnection` and is out of scope for this slice; see Known Limitations.

## Known Limitations
- MVC `Program.cs` does not bind `DefaultConnection` (user-secret exists but isn't read), so the live MVC app returns 500 against SQL Server; wiring the connection string requires touching `Program.cs`, which is outside this slice's ownership. The MVC layer itself is covered by InMemory tests.
- Tests use EF Core InMemory, which does not enforce the SQL Server filtered unique index; the duplicate-return conflict path is exercised in application code.
- No browser-level visual inspection was performed; UI verification is limited to the rendered Razor markup structure.

## Follow-up Tasks
- Wire `DefaultConnection` into `src/CampusLibrary.WebMvc/Program.cs` so the live MVC app can connect to SQL Server.
- Complete the remaining Milestone 5 slices (Books, Members, shared navigation/landing) per `tasks/plan-milestone5-mvc.md`.
