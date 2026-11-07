# Milestone 3 — Books API

## Summary of the Request
Implement Books CRUD for CampusLibrary: DTOs, request validation, API endpoints, expected status/error handling, and tests. Stay within Milestone 3; do not implement Members or Loans APIs or change the schema.

## Work Performed
- Added create/update request DTOs and a public response DTO.
- Added five async API routes: list, get by ID, create, update, and delete.
- Used projection/DTOs so EF entities are not returned.
- Added model validation and mapped SQL Server unique-key violations to 409; FK violations during delete map to 409.
- Added xUnit tests using EF Core InMemory for endpoint outcomes.
- Performed live API requests against the existing Docker SQL Server to verify CRUD, invalid request response, duplicate ISBN conflict, and delete conflict.
- Restored original five seeded books and confirmed no test loan rows remain.

## Projects Affected
- `CampusLibrary.WebApi`
- `CampusLibrary.Tests`

## Files Added or Modified

| File | Change | Reason |
|---|---|---|
| `src/CampusLibrary.WebApi/Controllers/BooksController.cs` | Added | Implements the five Books CRUD endpoints and maps expected SQL Server constraints to 409. |
| `src/CampusLibrary.WebApi/DTOs/BookDto.cs` | Added | Defines the safe response representation. |
| `src/CampusLibrary.WebApi/DTOs/CreateBookRequest.cs` | Added | Validates book creation input. |
| `src/CampusLibrary.WebApi/DTOs/UpdateBookRequest.cs` | Added | Validates book update input. |
| `tests/CampusLibrary.Tests/BooksControllerTests.cs` | Added | Exercises list/get/create/update/delete outcomes, validation and missing resources. |
| `tests/CampusLibrary.Tests/Usings.cs` | Added | Imports xUnit test attributes/assertions. |
| `tests/CampusLibrary.Tests/CampusLibrary.Tests.csproj` | Modified | Adds EF Core InMemory provider for isolated controller tests. |
| `tasks/plan.md` | Added | Approved implementation plan. |
| `tasks/todo.md` | Added/updated | Task list and completion tracking. |
| `SPEC-books-api.md` | Added | Approved Books API specification. |
| `docs/reports/INDEX.md` | Modified | Adds this report to the index. |

## Important Implementation Details
- `GET /api/books` and `GET /api/books/{id}` return DTOs and use no-tracking reads.
- `POST /api/books` returns 201 and a `Location` pointing at the new resource.
- `PUT /api/books/{id}` returns 204 after update.
- `DELETE /api/books/{id}` returns 204 when removed.
- Input errors return 400, missing books return 404, duplicate ISBN returns 409, and foreign-key delete conflicts return 409.
- SQL Server exception number mapping: 2601/2627 (unique-key violation), 547 (foreign-key violation).
- The API runs with `ASPNETCORE_ENVIRONMENT=Development` to load the development user-secrets connection string.

Example DTO shape:
```csharp
public record BookDto(int Id, string Title, string Author, string ISBN, int AvailableCopies);
```

## Database Changes
None. No migrations were added or applied for this milestone. During smoke tests, one temporary loan was inserted and removed, one temporary test book was inserted and removed, and a seeded book was restored after mistakenly deleting it during a delete smoke test. Final verification confirmed the five original seed books exist and there are no loan rows.

## Build and Test Results
- `dotnet restore CampusLibrary.sln` — passed.
- `dotnet build CampusLibrary.sln` — passed, 0 warnings and 0 errors.
- `dotnet test CampusLibrary.sln` — passed, 10 tests, 0 failed, 0 skipped.
- `git diff --check` — passed.
- HTTP smoke checks against SQL Server-backed API:
  - List books: 200.
  - Get seeded book: 200.
  - Invalid create input: 400.
  - Valid create then delete: 201 then 204; temporary row confirmed removed.
  - Duplicate seeded ISBN: 409.
  - Valid update: 204.
  - Delete book with an open loan: 409; temporary loan was then removed.
  - Delete after removing loan: 204; the deleted seeded book was restored.
- Final SQL verification: 5 seeded books present, 0 loans.

## Known Limitations
- List endpoint is unpaginated, matching the current milestone contract.
- ISBN validation enforces non-empty length up to 13 characters; it does not validate ISBN checksum/format.
- Unit tests use EF Core InMemory and do not independently emulate SQL Server constraint exceptions; the relevant uniqueness and FK conflict paths were verified against local SQL Server through live requests.
- No authentication/authorization is added in this milestone.
- SQL Server conflict mappings are verified with live requests. The unit suite uses EF Core InMemory and does not separately synthesize wrapped SQL exception chains; detection traverses the inner-exception chain now.

## Follow-up Tasks
- Milestone 4: Members and Loans APIs, including borrow/return rules and concurrency tests.
- Consider pagination when the catalogue needs bounded list responses.
- Consider adding tests for SQL Server conflict mappings with a dedicated integration test fixture if SQL-backed automated tests are adopted.