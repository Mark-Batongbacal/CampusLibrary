# How to Use — Members MVC Slice

## 1. Prerequisites
- .NET 9 SDK installed.
- The CampusLibrary solution restored/built.
- Microsoft SQL Server 2022 running in Docker (for database-backed operation).

## 2. Services that must be running
Start the database (only needed for real database operation; tests use an in-memory database):

```bash
docker compose up -d
docker compose ps
```

The API/MVC read `DefaultConnection` from configuration/user-secrets (example: `Server=localhost,1433;Database=CampusLibraryDb;User Id=sa;Password=<secret>;Encrypt=True;TrustServerCertificate=True;`).

## 3. Step-by-step instructions

### Run the MVC app
From the repository root:

```bash
dotnet run --project src/CampusLibrary.WebMvc
```

The app listens on the configured Kestrel ports (default HTTP/HTTPS). Open the Members page:

```
http://localhost:<port>/Members
```

### View members
- Navigate to `/Members` to see the members list (name, email, View action).
- Empty state: "There are no members." when no members exist.

### View member details + borrowing history
- Click "View" on a member row, or navigate to `/Members/Details/<id>`.
- Shows name, email, and borrowing history (book, borrowed at, returned at, open/returned status badge).
- Empty state: "This member has no borrowing history."
- Unknown id returns a 404 Not Found.

### Add a member
- Click "Add member" on the list, or navigate to `/Members/Create`.
- Enter Full name and Email address, then submit.
- On success: redirected to that member's Details page.
- On invalid input: form redisplays with your input preserved and validation messages shown.
- Duplicate email: a clear error ("A member with this email already exists.") is shown and no member is created.

## 4. Commands to run the feature
```bash
# Restore and build
dotnet restore
dotnet build

# Run the MVC app
dotnet run --project src/CampusLibrary.WebMvc

# Run tests (includes the 8 new Members MVC controller tests)
dotnet test
```

## 5. Example usage
- Browse to `/Members` → list of seeded members (Alice Johnson, Bob Smith, Carol Williams plus any added).
- `/Members/Create` → POST `{ FullName: "Dana Lee", Email: "dana.lee@university.edu" }` → creates member, redirects to `/Members/Details/<new id>`.
- Re-submitting the same email → conflict error, no duplicate created.

## 6. Expected results
- Members list renders with a table of members and a "View" link per row.
- Details page renders member info and borrowing history with Open/Returned badges.
- Create form accepts valid input, rejects invalid/duplicate input with visible messages, preserves input on error.
- `dotnet build` and `dotnet test` pass.

## 7. Common problems
- **Build references a missing `BooksController`**: this task did not implement Books MVC; if `BooksMvcControllerTests.cs` exists and blocks the build, remove it (it is pre-existing broken code outside this slice).
- **Connection errors**: ensure SQL Server container is running (`docker compose ps`) and the connection string/password in user-secrets is correct.
- **Duplicate email not caught**: both a pre-save `AnyAsync` check and a `DbUpdateException` (SQL 2601/2627) catch handle it, including concurrent inserts.

## 8. Troubleshooting instructions
- Start MSSQL: `docker compose up -d`; check `docker compose logs -f mssql`.
- Verify migrations: `dotnet ef migrations list -p src/CampusLibrary.Data -s src/CampusLibrary.WebApi`.
- Tests use `Microsoft.EntityFrameworkCore.InMemory`, so no database is needed for `dotnet test`.
- Do not run `docker compose down -v` unless you explicitly intend to delete the database volume.