# Milestone 2 — Entity Framework Core

## Summary of Request

Create the complete Entity Foundation for CampusLibrary as specified in CLAUDE.md Milestone 2, including:
- Book, Member, and Loan domain entities
- LibraryDbContext with DbSets, relationships, and constraints
- Deterministic seed data (5 books, 3 members)
- EF Core migration generation and application to Docker MSSQL
- Database verification

## Work Performed

1. **Created Domain Entities**
   - `Book.cs` — Id, Title, Author, ISBN, AvailableCopies, navigation to Loan
   - `Member.cs` — Id, FullName, Email, navigation to Loans collection
   - `Loan.cs` — Id, BookId, MemberId, BorrowedAt, ReturnedAt (nullable), navigation to Book and Member

2. **Updated LibraryDbContext**
   - Added DbSets for Books, Members, Loans
   - Configured entity relationships and constraints via Fluent API:
     - Required fields with max lengths
     - Unique indexes on ISBN (Book) and Email (Member)
     - Foreign keys with `DeleteBehavior.Restrict` to prevent cascade deletes
     - **Filtered unique index** on `Loans.BookId` WHERE `ReturnedAt IS NULL` to enforce at most one open loan per book
   - Added deterministic seed data using `HasData`:
     - 5 example books with stable IDs
     - 3 example members with stable IDs

3. **Generated Migration**
   - `dotnet ef migrations add InitialCreate` — created migration with all tables, indexes, foreign keys, and seed data

4. **Applied Migration**
   - `dotnet ef database update` — created `CampusLibraryDb`, applied schema, inserted seed data

5. **Verified Database**
   - Confirmed database creation
   - Verified all 4 tables exist (Books, Members, Loans, __EFMigrationsHistory)
   - Verified seed data (5 books, 3 members)
   - Verified filtered unique index on Loans.BookId with filter `[ReturnedAt] IS NULL`
   - Verified foreign key constraints with NO ACTION (Restrict)
   - Verified unique indexes on Books.ISBN and Members.Email

## Projects Affected

| Project | Path |
|---------|------|
| CampusLibrary.Data | src/CampusLibrary.Data/ |

## Files Added/Modified

| File | Change | Reason |
|------|--------|--------|
| src/CampusLibrary.Data/Book.cs | Added | Book entity with properties and navigation |
| src/CampusLibrary.Data/Member.cs | Added | Member entity with properties and navigation |
| src/CampusLibrary.Data/Loan.cs | Added | Loan entity with properties and navigation |
| src/CampusLibrary.Data/LibraryDbContext.cs | Modified | Added DbSets, Fluent API config, seed data |
| src/CampusLibrary.Data/Migrations/20261107122452_InitialCreate.cs | Added | Initial migration |
| src/CampusLibrary.Data/Migrations/20261107122452_InitialCreate.Designer.cs | Added | Migration designer file |
| src/CampusLibrary.Data/Migrations/LibraryDbContextModelSnapshot.cs | Added | Model snapshot |

## Important Implementation Details

### Availability Model
- **Book.AvailableCopies** tracks total copies available
- **Loan** entity tracks individual borrow/return events
- **Filtered unique index** on `Loans.BookId` WHERE `ReturnedAt IS NULL` enforces: a book can have at most one open (unreturned) loan at a time
- This ensures database-level enforcement — application checks alone are insufficient for concurrency

### Constraints
- `DeleteBehavior.Restrict` on all FKs prevents accidental deletion of books/members with existing loans
- Unique ISBN (max 13 chars) and Email (max 200 chars)
- Required Title, Author, FullName, Email
- Loan.BorrowedAt required, ReturnedAt nullable

### Seed Data
- 5 books with known ISBNs and AvailableCopies
- 3 members with university emails
- Stable IDs for deterministic migrations

## Database Changes

| Table | Columns | Indexes/Constraints |
|-------|---------|---------------------|
| Books | Id (PK), Title, Author, ISBN, AvailableCopies | PK_Books, IX_Books_ISBN (unique) |
| Members | Id (PK), FullName, Email | PK_Members, IX_Members_Email (unique) |
| Loans | Id (PK), BookId (FK), MemberId (FK), BorrowedAt, ReturnedAt | PK_Loans, IX_Loans_BookId (unique, filtered), IX_Loans_MemberId, FK_Loans_Books_BookId (NO ACTION), FK_Loans_Members_MemberId (NO ACTION) |
| __EFMigrationsHistory | MigrationId (PK), ProductVersion | PK___EFMigrationsHistory |

## Build and Test Results

| Command | Result |
|---------|--------|
| dotnet build | ✅ Passed (0 warnings, 0 errors) |
| dotnet ef migrations add InitialCreate | ✅ Passed |
| dotnet ef database update | ✅ Passed |
| Database verification (sqlcmd) | ✅ All tables, seed data, indexes, FKs confirmed |

## Known Limitations

1. No API endpoints yet — Milestone 3 (Books API) and Milestone 4 (Members/Loans API)
2. No MVC UI — Milestone 5
3. No Android app — Milestone 6-7
4. Test project exists but no tests written yet

## Follow-up Tasks

1. **Milestone 3**: Implement Books CRUD API endpoints with DTOs and Swagger
2. **Milestone 4**: Implement Members and Loans API endpoints with borrowing/returning logic
3. **Milestone 5**: Create MVC admin pages for Books, Members, Loans
4. **Milestone 6**: Create Android app foundation
5. **Milestone 7**: Implement Android borrowing features