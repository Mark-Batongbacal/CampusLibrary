# Spec: Books API (Milestone 3)

## Objective
Implement CRUD endpoints for librarians to list, retrieve, create, update, and delete books through `CampusLibrary.WebApi`. Keep EF Core entities behind DTOs, validate inputs, return the status codes and predictable errors required by `CLAUDE.md`, and test book operations.

## Assumptions
1. The existing .NET 9 / EF Core 9 architecture remains authoritative (the repository instructions require .NET 9, despite the earlier request mentioning .NET 8).
2. Books CRUD is the full scope; Members and Loans endpoints are not included.
3. Existing SQL schema and migration remain unchanged unless implementation reveals a required schema change; any such change will be reported before applying it.
4. Unique ISBN conflicts return HTTP 409; malformed/invalid DTO input returns HTTP 400; missing IDs return HTTP 404.
5. Delete conflicts caused by existing loans return HTTP 409; database records will not be removed/reset during implementation.
6. Tests use the existing xUnit project and avoid requiring the live database where possible.

## Tech Stack
- ASP.NET Core Web API, .NET 9
- EF Core SQL Server 9
- Existing Swagger/OpenAPI setup
- Existing xUnit test project

## Commands
- Restore: `dotnet restore`
- Build: `dotnet build`
- Tests: `dotnet test`
- Run API: `dotnet run --project src/CampusLibrary.WebApi`
- Migrations: none expected for Books CRUD; do not apply schema changes without reviewing them.

## Project Structure
- `src/CampusLibrary.WebApi/Controllers/BooksController.cs` — CRUD HTTP endpoints
- `src/CampusLibrary.WebApi/DTOs/` — request/response DTOs
- `src/CampusLibrary.Data/` — existing EF Core model and context; no changes expected
- `tests/CampusLibrary.Tests/` — Books API/controller behavior tests
- `docs/reports/` — required task reports

## Code Style
Use async EF Core calls, constructor-injected `LibraryDbContext`, DTOs rather than entity responses, and normal ASP.NET Core controller attributes. Example:

```csharp
[HttpGet("{id:int}")]
public async Task<ActionResult<BookDto>> GetById(int id)
{
    var book = await db.Books.FindAsync(id);
    return book is null ? NotFound() : Ok(BookDto.From(book));
}
```

## Testing Strategy
- Add focused xUnit tests for valid and invalid create/update input and not-found behavior.
- Verify duplicate ISBN and delete-with-loans produce conflict responses where the database provider permits realistic testing.
- Run `dotnet test` and `dotnet build`.
- Exercise Swagger and CRUD endpoints against local SQL Server only if service availability permits; report what was and was not exercised.

## Boundaries
- **Always:** Validate request data; use DTOs; perform async persistence; preserve database data; return predictable status codes; run build and tests.
- **Ask first:** Any migration/schema changes, database reset/deletion, new package dependency, or endpoint behavior outside Books CRUD.
- **Never:** Return EF entities directly, expose credentials, implement Members/Loans, or claim tests/database checks that were not performed.

## Success Criteria
- `GET /api/books` returns a collection of book DTOs.
- `GET /api/books/{id}` returns a DTO or 404.
- `POST /api/books` validates input, persists a book, and returns 201 with its DTO and location.
- `PUT /api/books/{id}` validates and updates a book; returns 204, 400, or 404 as appropriate.
- `DELETE /api/books/{id}` deletes an existing book; returns 204, 404, or 409 when loans prevent deletion.
- Duplicate ISBN is handled as a conflict (409), including database constraint races.
- Tests cover key success and failure paths, and `dotnet build` and `dotnet test` succeed.
- No Members/Loans endpoints, UI, or migration work is added.

## Open Questions
None currently. Please review this scope and assumptions before planning/implementation.