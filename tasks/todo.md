# Milestone 4 — Members and Loans API Tasks

## Task 1: Add Member request/response DTOs and validation
**Description:** Define request and response DTOs for Members CRUD with required field validation and duplicate email detection.

**Acceptance criteria:**
- [x] Create/update request DTOs validate title/name and email lengths and required values.
- [x] Response DTO contains only public member fields, not EF navigation properties.
- [x] Duplicate email returns 409 Conflict.
- [x] No database schema changes.

**Verification:** Build the Data/API projects and add DTO validation assertions in the test project.

**Dependencies:** None.

**Files likely touched:** `src/CampusLibrary.WebApi/DTOs/`

**Estimated scope:** Small.

---

## Task 2: Implement MembersController
**Description:** Add all five Members CRUD routes using the existing pattern from BooksController.

**Acceptance criteria:**
- [x] GET collection/item, POST, PUT, DELETE routes follow the spec.
- [x] Return DTOs only; invalid requests are 400, missing resources 404, duplicate email 409.
- [x] DELETE with loan history returns 409.
- [x] No schema changes, data resets, or unrelated endpoints.

**Verification:** Controller tests cover endpoint outcomes; review SQL Server-specific conflict handling.

**Dependencies:** Task 1.

**Files likely touched:** `src/CampusLibrary.WebApi/Controllers/MembersController.cs`

**Estimated scope:** Medium.

---

## Task 3: Add Loan request/response DTOs
**Description:** Define request and response DTOs for Loan operations including borrow and return request models.

**Acceptance criteria:**
- [x] Loan response DTO exposes IDs, borrowed/returned timestamps, and book/member summary details.
- [x] Borrow request contains member ID and book ID.
- [x] Return request model is minimal (loan ID only).
- [x] No database schema changes.

**Verification:** Build the project and verify DTO shapes are correct.

**Dependencies:** Task 1 (Member DTOs).

**Files likely touched:** `src/CampusLibrary.WebApi/DTOs/`

**Estimated scope:** Small.

---

## Task 4: Implement LoansController
**Description:** Implement LoansController with list, get, borrow, and return routes enforcing borrowing rules.

**Acceptance criteria:**
- [x] GET collection/item routes return loan DTOs.
- [x] POST /api/loans verifies member and book existence, rejects book with open loan using 409, returns 201 with created loan DTO.
- [x] POST /api/loans/{id}/return sets ReturnedAt for an open loan and returns 204; missing loans return 404 and repeated returns return 409.
- [x] Concurrent borrow attempts map to SQL Server unique constraint violation 2601/2627 → 409.
- [x] No schema changes, data resets, or unrelated endpoints.

**Verification:** Controller tests cover success/failure paths; SQL Server constraint tests if safe setup available.

**Dependencies:** Task 2 (MembersController), Task 3 (Loan DTOs).

**Files likely touched:** `src/CampusLibrary.WebApi/Controllers/LoansController.cs`

**Estimated scope:** Medium.

---

## Task 5: Add xUnit Tests
**Description:** Add focused tests covering Members CRUD success, validation, not-found, duplicate-email, and delete-with-loan outcomes, plus Loan list/get/borrow/return outcomes.

**Acceptance criteria:**
- [x] Tests exercise core success and failure paths for Members and Loans.
- [x] Tests do not rely on deleting/resetting the shared development database.
- [x] Existing test infrastructure remains compatible.

**Dependencies:** Tasks 1-4.

**Files likely touched:** `tests/CampusLibrary.Tests/`.

**Estimated scope:** Medium.

---

## Task 6: Final verification and review
**Description:** Run restore/build/test, inspect diff, and perform safe API/database smoke tests where services are available.

**Acceptance criteria:**
- [x] Actual command outcomes recorded; no unsupported claims.
- [x] No schema/migration changes; original seed data restored, no loan rows remain (or test isolation).
- [x] Required reports created and index updated.

**Verification:** `dotnet restore`, `dotnet build`, `dotnet test`; optional API/SQL smoke test.

**Dependencies:** Tasks 1-5.

**Files likely touched:** Reports under `docs/reports/` and `docs/reports/INDEX.md`.

**Estimated scope:** Small.

---

## Checkpoint: Complete
- [x] Spec success criteria satisfied.
- [x] Final code review complete.