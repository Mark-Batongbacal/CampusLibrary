# How to Use — MVC Admin (Books, Members, Loans)

A student or classmate can run the Milestone 5 librarian admin interface locally.

## Prerequisites

- .NET 9 SDK installed (`dotnet --version` shows 9.x).
- Docker Engine and Docker Compose installed and running.
- A terminal at the repository root: `/home/mark/Documents/CampusLibrary`.

## Services That Must Be Running

1. Microsoft SQL Server 2022 in Docker (required for the real database):

   ```bash
   docker compose up -d
   docker compose ps
   ```

   Confirm `campuslibrary-mssql` is `Up` and bound to `127.0.0.1:1433`.

2. The connection string is configured via .NET user-secrets on the MVC project (`DefaultConnection`, read by `Program.cs`).

## Step-by-Step Instructions

1. Restore and build the solution:

   ```bash
   dotnet restore
   dotnet build CampusLibrary.sln
   ```

2. Run all tests:

   ```bash
   dotnet test tests/CampusLibrary.Tests/CampusLibrary.Tests.csproj
   ```

   Expected: 61 passed, 0 failed.

3. Start SQL Server (for the live database):

   ```bash
   docker compose up -d
   ```

4. Start the MVC app:

   ```bash
   dotnet run --project src/CampusLibrary.WebMvc
   ```

5. Open the admin pages in a browser:
   - Books: `http://localhost:5000/Books`
   - Members: `http://localhost:5000/Members`
   - Loans: `http://localhost:5000/Loans`

## Commands to Run the Feature

| Purpose | Command |
|---|---|
| Restore packages | `dotnet restore` |
| Build solution | `dotnet build CampusLibrary.sln` |
| Run all tests | `dotnet test tests/CampusLibrary.Tests/CampusLibrary.Tests.csproj` |
| Start SQL Server | `docker compose up -d` |
| Stop SQL Server (keeps data) | `docker compose down` |
| Start MVC app | `dotnet run --project src/CampusLibrary.WebMvc` |

## Example Usage

- **Books list:** Open `/Books` → see a table of books with Title, Author, ISBN, Available copies, plus Add/Edit/Delete actions.
- **Add a book:** Click "Add book" → fill Title, Author, ISBN, Available copies → submit. Duplicate ISBN shows a validation error and re-displays the form.
- **Edit a book:** Click "Edit" on a row → change fields → submit → redirected back to the list.
- **Delete a book:** Click "Delete" → if the book has an open loan, a conflict message appears and the book is not deleted; otherwise it is removed.
- **Members list:** Open `/Members` → see all members with Full Name and Email.
- **Member details:** Click a member → see their borrowing history (book, borrowed at, returned at).
- **Add a member:** Open `/Members/Create` → fill Full Name and Email → submit → redirected to the new member's details. Duplicate email shows an error.
- **Loans list:** Open `/Loans` → see only open loans (`ReturnedAt IS NULL`) with Book, Author, Member, Borrowed at, and a Return button.
- **Return a loan:** Click "Return" on a row → confirms via browser dialog → submits an antiforgery-protected POST → sets `ReturnedAt` → redirects back to the list; the returned loan no longer appears.
- **Already returned / missing loan:** Returns to the list with an error message ("This loan has already been returned." / "Loan not found.").

## Expected Results

- `dotnet build` completes with 0 warnings and 0 errors.
- `dotnet test` reports 61 passed, 0 failed.
- The books page lists books with Add/Edit/Delete.
- Adding/editing a book with a duplicate ISBN shows a validation error and does not create a duplicate.
- Deleting a book with an open loan is blocked with a conflict message.
- The members page lists members; details show borrowing history.
- Adding a member with a duplicate email shows a validation error.
- The loans page lists only open loans.
- Returning an open loan sets `ReturnedAt` and removes it from the open-loans list.
- Returning an already-returned loan leaves `ReturnedAt` unchanged and shows an error.

## Common Problems

- **500 on MVC pages when running the app:** SQL Server may not be running or `DefaultConnection` may not be configured. Run `docker compose up -d` and confirm `campuslibrary-mssql` is `Up` on `127.0.0.1:1433`.
- **Build warnings/errors:** run `dotnet restore` then `dotnet build` again; ensure .NET 9 SDK is active.
- **Tests fail:** run `dotnet restore` then `dotnet test` again; ensure .NET 9 SDK is active.
- Never run `docker compose down -v` — that deletes the persistent database volume.
- Do not commit `.env` or real passwords; they belong in .NET user-secrets only.
- Browser visual QA and live SQL smoke checks were not performed in this task; controller behavior is verified by InMemory tests.

## Troubleshooting Instructions

- If SQL Server isn't reachable: run `docker compose up -d` and `docker compose ps` to confirm `campuslibrary-mssql` is `Up` on `127.0.0.1:1433`.
- If tests fail: run `dotnet restore` then `dotnet test` again; ensure .NET 9 SDK is active.
- Never run `docker compose down -v` — that deletes the persistent database volume.
- Do not commit `.env` or real passwords; they belong in .NET user-secrets only.