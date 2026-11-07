# Milestone 5 MVC Task List

- [ ] Task 1: Complete Books MVC list/create/edit/delete with typed view models and Razor views.
  - Acceptance: CRUD pages work; invalid input is preserved with validation messages; ISBN/FK conflicts are user-visible without deleting loan history; POST actions validate antiforgery.
  - Verify: Focused Books MVC tests and `dotnet build`.
  - Files: `src/CampusLibrary.WebMvc/Controllers/BooksController.cs`, `Models/Books/`, `Views/Books/`, `tests/CampusLibrary.Tests/BooksMvcControllerTests.cs`
- [ ] Task 2: Implement Members list/create/details.
  - Acceptance: List/add/details work; server-side validation preserves input; duplicate email conflict is explained; POST create validates antiforgery.
  - Verify: Focused Members MVC tests and `dotnet build`.
  - Files: `src/CampusLibrary.WebMvc/Controllers/MembersController.cs`, `Models/Members/`, `Views/Members/`, `tests/CampusLibrary.Tests/MembersMvcControllerTests.cs`
- [x] Task 3: Implement open-loans list and return.
  - Acceptance: Open loan list projects book/member/borrowed data; return POST is antiforgery-protected and updates UTC timestamp; not-found/duplicate cases are currently returned as HTTP 404/409; successful return redirects.
  - Verify: Agent reports 8 focused tests passed as part of full `dotnet test` (40 passed); coordinator review still required.
  - Files: `src/CampusLibrary.WebMvc/Controllers/LoansController.cs`, `Models/Loans/`, `Views/Loans/`, `tests/CampusLibrary.Tests/LoansMvcControllerTests.cs`
- [ ] Task 4: Add default landing/navigation, shared layout and responsive accessible UI states.
  - Acceptance: Navigation reaches all workflows; empty and validation states are clear; controls are labeled and keyboard accessible; layout works at narrow and wide widths.
  - Verify: `dotnet build`, `dotnet test`, run MVC and visually inspect if browser tooling is available.
  - Files: `src/CampusLibrary.WebMvc/Controllers/HomeController.cs`, `Views/Shared/`, `Views/Home/`, `Views/_ViewStart.cshtml`, `Views/_ViewImports.cshtml`, `wwwroot/css/site.css`

## Checkpoint: After Tasks 1–2
- [ ] Books and Members tests/build pass; no API or schema/migration changes.

## Checkpoint: Complete
- [ ] All task acceptance criteria pass; Loans slice reviewed.
- [ ] Full solution build and tests pass.
- [ ] UI reviewed at narrow/wide sizes or visual limitation disclosed.
- [ ] Required implementation reports and index entry are complete.
