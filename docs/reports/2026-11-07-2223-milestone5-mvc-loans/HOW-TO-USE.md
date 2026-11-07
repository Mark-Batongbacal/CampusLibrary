# How to Use — MVC Loans Workflow

A student or classmate can run the open-loans listing and return feature locally.

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

2. The database connection string is configured via .NET user-secrets on the MVC project (already present). The MVC `Program.cs` does not yet read `DefaultConnection`, so the live MVC app needs that wiring before it can connect to SQL Server. See "Common Problems".

## Step-by-Step Instructions
1. Restore and build the solution:
   ```bash
   dotnet restore
   dotnet build CampusLibrary.sln
   ```
2. Run the focused tests:
   ```bash
   dotnet test tests/CampusLibrary.Tests/CampusLibrary.Tests.csproj
   ```
   All 40 tests pass, including the 8 new Loans MVC tests.

3. (Once `Program.cs` is wired to read `DefaultConnection`) Start the MVC app:
   ```bash
   dotnet run --project src/CampusLibrary.WebMvc
   ```
   Then open `http://localhost:5000/Loans` to view open loans.

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
- Open `http://localhost:5000/Loans` → see a table of open loans with Book, Author, Member, and Borrowed at columns.
- Click "Return" on a row → confirms via browser dialog, submits an antiforgery-protected POST, marks the loan returned, and redirects back to the open-loans list (the returned loan no longer appears).
- If there are no open loans → an empty-state card says "There are no open loans."

## Expected Results
- `dotnet build` completes with 0 errors.
- `dotnet test` reports 40 passed, 0 failed.
- The open-loans page lists only loans where `ReturnedAt IS NULL`.
- Returning an open loan sets `ReturnedAt` and redirects to the list.
- Returning an already-returned loan returns 409 (conflict) and leaves `ReturnedAt` unchanged.
- A missing loan id returns 404.

## Common Problems
- **500 on `/Loans` when running the MVC app**: The MVC `Program.cs` does not read `DefaultConnection` from user-secrets, so EF Core cannot connect to SQL Server. This requires wiring the connection string into `Program.cs`, which is outside this Loans-only slice. The controller behavior is fully verified by InMemory tests.
- **Build warnings**: two nullable-reference warnings exist in the test file; they are pre-existing patterns and do not fail the build.
- **EF Core InMemory tests**: they do not enforce the SQL Server filtered unique index; the duplicate-return conflict is validated in application code.

## Troubleshooting Instructions
- If SQL Server isn't reachable: run `docker compose up -d` and `docker compose ps` to confirm `campuslibrary-mssql` is `Up` on `127.0.0.1:1433`.
- If tests fail: run `dotnet restore` then `dotnet test` again; ensure .NET 9 SDK is active.
- Never run `docker compose down -v` — that deletes the persistent database volume.
- Do not commit `.env` or real passwords; they belong in .NET user-secrets only.
