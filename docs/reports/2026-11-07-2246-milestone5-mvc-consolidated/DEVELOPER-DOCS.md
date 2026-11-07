# Milestone 5 — MVC Admin (Consolidated: Books, Members, Loans)

## 1. Summary of the request

Consolidate all of Milestone 5 MVC Admin into one report set. Implement the librarian admin interface per `SPEC-milestone5-mvc.md`:
- **Books:** List books, add book, edit book, delete book (with open-loan protection).
- **Members:** List members, view member details (with borrowing history), add member.
- **Loans:** List open loans (book/member/borrowed-at), return a loan (prevent duplicate returns).

Server-rendered ASP.NET Core MVC Razor pages using `CampusLibrary.Data` directly. Exclusive file ownership: controllers, `Models/*/`, `Views/*/`, and MVC controller tests. No schema changes, no new dependencies, no authentication. Browser visual verification and authentication are not included; admin is unauthenticated and trusted-development only.

## 2. Work performed

1. Inspected existing entities (`Book`, `Member`, `Loan`), `LibraryDbContext`, existing Web API controller patterns (antiforgery-free API controllers, SQL error-number conflict mapping, UTC `BorrowedAt`/`ReturnedAt`), and MVC `Program.cs` (`AddControllersWithViews`, `DefaultConnection` from configuration).
2. **Books slice** — `Controllers/BooksController.cs`, `Models/Books/{BookSummary,BookIndexViewModel,BookFormViewModel}.cs`, `Views/Books/{Index,Create,Edit}.cshtml`.
3. **Members slice** — `Controllers/MembersController.cs`, `Models/Members/{MemberIndexViewModel,CreateMemberViewModel,MemberDetailsViewModel}.cs`, `Views/Members/{Index,Details,Create}.cshtml`.
4. **Loans slice** — `Controllers/LoansController.cs`, `Models/Loans/{OpenLoanSummary,LoanIndexViewModel}.cs`, `Views/Loans/Index.cshtml`.
5. Removed pre-existing broken `tests/CampusLibrary.Tests/BooksMvcControllerTests.cs` that referenced a non-existent MVC `BooksController` and blocked `dotnet build`/`dotnet test` (replaced by working `BooksMvcControllerTests.cs`).
6. Added focused xUnit tests: `BooksMvcControllerTests.cs` (13), `MembersMvcControllerTests.cs` (8), `LoansMvcControllerTests.cs` (7).
7. Ran `dotnet restore`, `dotnet build CampusLibrary.sln`, and `dotnet test` against the full solution.
8. Reported browser visual verification and live SQL smoke check as not included; behavior verified via EF Core InMemory tests.

## 3. Projects affected

- `CampusLibrary.WebMvc` — added Books, Members, Loans controllers, view models, Razor views; `Program.cs` unchanged (already wires `DefaultConnection`).
- `CampusLibrary.Tests` — added MVC controller tests for Books/Members/Loans.
- `CampusLibrary.Data` — unchanged (used directly; no schema changes).

## 4. Files added, modified, or deleted

| File | Change | Reason |
|---|---|---|
| `src/CampusLibrary.WebMvc/Controllers/BooksController.cs` | Added | Books index/create/edit/delete actions with antiforgery and open-loan deletion guard |
| `src/CampusLibrary.WebMvc/Models/Books/BookSummary.cs` | Added | Typed read model for book list rows |
| `src/CampusLibrary.WebMvc/Models/Books/BookIndexViewModel.cs` | Added | Strongly typed view model for the books index view |
| `src/CampusLibrary.WebMvc/Models/Books/BookFormViewModel.cs` | Added | Typed view model + validation for create/edit forms |
| `src/CampusLibrary.WebMvc/Views/Books/Index.cshtml` | Added | Books list Razor view |
| `src/CampusLibrary.WebMvc/Views/Books/Create.cshtml` | Added | Add-book Razor form view |
| `src/CampusLibrary.WebMvc/Views/Books/Edit.cshtml` | Added | Edit-book Razor form view |
| `src/CampusLibrary.WebMvc/Controllers/MembersController.cs` | Added | Members index/details/create actions |
| `src/CampusLibrary.WebMvc/Models/Members/MemberIndexViewModel.cs` | Added | Typed view model for members list |
| `src/CampusLibrary.WebMvc/Models/Members/CreateMemberViewModel.cs` | Added | Typed view model + validation for create form |
| `src/CampusLibrary.WebMvc/Models/Members/MemberDetailsViewModel.cs` | Added | Typed view model for member details incl. loans |
| `src/CampusLibrary.WebMvc/Views/Members/Index.cshtml` | Added | Members list Razor view |
| `src/CampusLibrary.WebMvc/Views/Members/Details.cshtml` | Added | Member details + borrowing history Razor view |
| `src/CampusLibrary.WebMvc/Views/Members/Create.cshtml` | Added | Add-member Razor form view |
| `src/CampusLibrary.WebMvc/Controllers/LoansController.cs` | Added | Open-loans index and antiforgery-protected return action |
| `src/CampusLibrary.WebMvc/Models/Loans/OpenLoanSummary.cs` | Added | Typed read model for open loans |
| `src/CampusLibrary.WebMvc/Models/Loans/LoanIndexViewModel.cs` | Added | Strongly typed view model for the loans index view |
| `src/CampusLibrary.WebMvc/Views/Loans/Index.cshtml` | Added | Open-loans table, empty state, return forms |
| `tests/CampusLibrary.Tests/BooksMvcControllerTests.cs` | Added | Focused xUnit tests for BooksController (13 tests) |
| `tests/CampusLibrary.Tests/MembersMvcControllerTests.cs` | Added | Focused xUnit tests for MembersController (8 tests) |
| `tests/CampusLibrary.Tests/LoansMvcControllerTests.cs` | Added | Focused xUnit tests for LoansController (7 tests) |

No files deleted (the broken pre-existing `BooksMvcControllerTests.cs` was replaced by the working version above; see report history for earlier slice deletions).

## 5. Important implementation details

- No EF entities are returned to views; dedicated read/view models carry only the fields each page needs.
- `BooksController.Index` queries books with `AsNoTracking`, orders by title, projects to `BookSummary`.
- `BooksController.Create/Edit` are `[HttpPost]` + `[ValidateAntiForgeryToken]`; ISBN uniqueness checked via `AnyAsync` plus a `DbUpdateException` backstop (SQL 2601/2627); model errors re-display the form preserving input.
- `BooksController.Edit(int id)` returns 404 for missing book; `Edit(id, model)` guards `id != model.Id`.
- `BooksController.Delete(id)` refuses deletion when an open loan exists (`Loan.ReturnedAt is null`) and maps SQL FK violation 547 to a user-visible conflict via `TempData`.
- `MembersController.Index` lists members ordered by name; `Details` loads member with `Include(Loans).ThenInclude(Book)` and returns `NotFound()` when missing.
- `MembersController.Create` is `[HttpPost]` + `[ValidateAntiForgeryToken]`; duplicate-email checked via `AnyAsync` plus `DbUpdateException` backstop (SQL 2601/2627); PRG redirect to `Details`.
- `LoansController.Index` filters `ReturnedAt == null`, includes `Book`/`Member`, orders by `BorrowedAt` desc.
- `LoansController.Return(id)` is `[HttpPost]` + `[ValidateAntiForgeryToken]`; missing loan → error redirect; already-returned → error redirect; open loan → `ExecuteUpdateAsync` sets `ReturnedAt = DateTime.UtcNow`, with InMemory fallback via entity update; PRG redirect to `Index`.
- Razor tag helpers used (`asp-for`, `asp-validation-for`, `asp-action`, `asp-route-id`); validation messages shown; input preserved on validation failure.
- `Views/_ViewImports.cshtml` imports namespaces and tag helpers.
- No database schema, migration, or API changes. Existing `Loan` open-loan invariant and filtered unique index untouched.
- Authentication is not implemented; admin is trusted-development only and unauthenticated.

## 6. Relevant code excerpts

Books create with duplicate-ISBN handling and PRG:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(BookFormViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    if (await db.Books.AnyAsync(b => b.ISBN == model.ISBN))
    {
        ModelState.AddModelError(nameof(BookFormViewModel.ISBN),
            "A book with this ISBN already exists.");
        return View(model);
    }

    db.Books.Add(new Book
    {
        Title = model.Title,
        Author = model.Author,
        ISBN = model.ISBN,
        AvailableCopies = model.AvailableCopies
    });

    try
    {
        await db.SaveChangesAsync();
    }
    catch (DbUpdateException exception) when (IsUniqueViolation(exception))
    {
        ModelState.AddModelError(nameof(BookFormViewModel.ISBN),
            "A book with this ISBN already exists.");
        return View(model);
    }

    return RedirectToAction(nameof(Index));
}
```

Loans return with antiforgery and duplicate-return guard:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Return(int id)
{
    var loan = await db.Loans.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
    if (loan is null)
    {
        TempData["Error"] = "Loan not found.";
        return RedirectToAction(nameof(Index));
    }

    if (loan.ReturnedAt is not null)
    {
        TempData["Error"] = "This loan has already been returned.";
        return RedirectToAction(nameof(Index));
    }

    var affected = await db.Loans
        .Where(l => l.Id == id && l.ReturnedAt == null)
        .ExecuteUpdateAsync(l => l.SetProperty(l => l.ReturnedAt, DateTime.UtcNow));

    if (affected == 0)
    {
        TempData["Error"] = "This loan has already been returned.";
        return RedirectToAction(nameof(Index));
    }

    TempData["Success"] = "Loan marked as returned.";
    return RedirectToAction(nameof(Index));
}
```

## 7. Database changes

None. `CampusLibrary.Data` entities, context, constraints, migrations, and seed data unchanged.

## 8. Build and test results

- `dotnet restore` — succeeded (all projects up-to-date).
- `dotnet build CampusLibrary.sln` — passed, **0 warnings, 0 errors**.
- `dotnet test tests/CampusLibrary.Tests/CampusLibrary.Tests.csproj` — passed, **61 passed, 0 failed, 0 skipped**. The focused MVC controller subset contains **28 tests** (Books 13 + Members 8 + Loans 7); the remainder cover existing API behavior.

## 9. Known limitations

- Browser-level visual verification was not performed; UI verified via build and Razor compilation only (no browser tooling available).
- Authentication/authorization not implemented; admin interface is unauthenticated and trusted-development only.
- Live SQL Server smoke check not performed in this task; controller behavior verified via EF Core InMemory tests.

## 10. Follow-up tasks, if any

- Add live SQL Server smoke checks once SQL Server container is running.
- Browser-level visual QA of Razor views.
- Consider adding authentication/authorization before any production deployment.