# Spec: Milestone 5 — MVC Librarian Admin

## Objective
Implement a server-rendered ASP.NET Core MVC admin interface for librarians to manage the CampusLibrary catalog and membership and process returns. The project brief defines these workflows; no authentication or product-specific branding is established. The MVC project uses `CampusLibrary.Data` directly and must preserve the existing data model and API behavior.

## Tech Stack
- ASP.NET Core MVC and Razor Views on .NET 9
- Existing EF Core SQL Server `LibraryDbContext` and entities
- Existing Bootstrap approach; no new frontend framework or package
- xUnit and the existing `CampusLibrary.Tests` project/provider for focused tests

## Commands
- Restore: `dotnet restore`
- Build: `dotnet build`
- Tests: `dotnet test`
- Run MVC: `dotnet run --project src/CampusLibrary.WebMvc`
- Database (when needed): `docker compose up -d`

## Project Structure
- `src/CampusLibrary.WebMvc/Controllers/` — Books, Members, Loans, and default landing controller as needed
- `src/CampusLibrary.WebMvc/Models/` — strongly typed MVC view models
- `src/CampusLibrary.WebMvc/Views/` — Razor pages and shared layout
- `src/CampusLibrary.WebMvc/wwwroot/` — project-owned styles only if needed
- `tests/CampusLibrary.Tests/` — focused MVC controller/action tests
- `docs/reports/` — required implementation reports

## Code Style
Use async EF Core calls, strongly typed view models, data annotations and ModelState validation, Razor tag helpers, and redirect-after-POST. Every state-changing POST must include `[HttpPost]` and `[ValidateAntiForgeryToken]`. Preserve submitted values on validation errors. Example:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateBookViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    // Persist using LibraryDbContext, then redirect after success.
    return RedirectToAction(nameof(Index));
}
```

Handle expected SQL Server unique-key and FK conflicts as user-visible conflict feedback (409-style UX), without deleting related data or leaking exception details. Use existing patterns and avoid extra services or dependencies.

## Testing Strategy
- Add focused xUnit tests for each controller's important success, not-found, validation, and conflict paths using the existing test setup.
- Verify anti-forgery attributes and server-side validation behavior through action/metadata tests where practical.
- Run `dotnet restore`, `dotnet build`, and `dotnet test`.
- If the local SQL Server is available, perform non-destructive MVC smoke checks against existing development data; do not reset or delete the database volume.
- Review the rendered UI for keyboard access, labels, responsive behavior, validation and empty states. State clearly if browser-level visual inspection is unavailable.

## Boundaries
- **Always:** Use the existing EF Core context and entities; validate on the server; use anti-forgery tokens on mutations; show validation messages; preserve submitted input; filter the Loans page to open loans; use Post/Redirect/Get after successful mutations; keep UI responsive and accessible.
- **Ask first:** Database schema/migration changes, new packages/frameworks, authentication, destructive data operations, or materially expanding the specified workflows.
- **Never:** Return EF entities as MVC view models, trust client-only validation, expose credentials or internal exception details, silently delete loans/related records, modify OmniRoute/global agent settings, or commit/push without request.

## Success Criteria
1. Books pages list, add, edit, and delete books; invalid submissions retain input and show validation; referenced-book deletion is handled without deleting loans.
2. Members pages list, add, and show member details; invalid submissions retain input; duplicate email conflicts are reported clearly.
3. Loans page lists open loans with book/member/borrowed-at information; return action is antiforgery-protected, prevents duplicate returns, and redirects after success.
4. All MVC state-changing POST actions validate antiforgery tokens and use strongly typed view models and Razor tag helpers.
5. MVC has a usable default landing/navigation experience; layouts and pages work at narrow and wide viewport sizes and have understandable empty/validation/error states.
6. Existing API, data model, database schema/migrations, and loan invariants remain unchanged.
7. `dotnet build` and `dotnet test` pass, and the required reports accurately record verification.

## Open Questions
- No authentication/authorization system is defined; the admin interface is not a secure production deployment until authentication is separately designed.
- Product-specific brand assets, visual identity, and custom accessibility accommodations are not established; use a restrained Bootstrap-compatible admin design and standard accessible patterns.
