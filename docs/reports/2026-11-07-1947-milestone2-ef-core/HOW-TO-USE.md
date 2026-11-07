# How to Use — CampusLibrary Milestone 2

## Prerequisites

- **Milestone 1 completed** (solution, projects, Docker SQL Server running)
- **.NET 9 SDK** with EF Core tools: `dotnet tool install --global dotnet-ef`
- **Docker** with SQL Server container running (`docker compose up -d`)
- **User secrets** configured with connection string (from Milestone 1)

## Step-by-Step Instructions

### 1. Verify SQL Server is Running

```bash
docker compose ps
```

Expected: `campuslibrary-mssql` container status "Up"

### 2. Create Domain Entities (Already Done)

The following files exist in `src/CampusLibrary.Data/`:
- `Book.cs`
- `Member.cs`
- `Loan.cs`
- `LibraryDbContext.cs` (updated with DbSets and configuration)

### 3. Generate Migration

```bash
dotnet ef migrations add InitialCreate \
  -p src/CampusLibrary.Data \
  -s src/CampusLibrary.WebApi
```

Expected output: "Done. To undo this action, use 'ef migrations remove'"

Migration files created in `src/CampusLibrary.Data/Migrations/`:
- `20261107122452_InitialCreate.cs`
- `20261107122452_InitialCreate.Designer.cs`
- `LibraryDbContextModelSnapshot.cs`

### 4. Apply Migration to Database

```bash
dotnet ef database update \
  -p src/CampusLibrary.Data \
  -s src/CampusLibrary.WebApi
```

Expected: Database `CampusLibraryDb` created, tables created, seed data inserted.

### 5. Verify Database

```bash
# Check database exists
docker exec -i campuslibrary-mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'StrongPass123!' -C \
  -Q "SELECT name FROM sys.databases WHERE name = 'CampusLibraryDb'"

# Check tables
docker exec -i campuslibrary-mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'StrongPass123!' -C -d CampusLibraryDb \
  -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"

# Check Books seed data
docker exec -i campuslibrary-mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'StrongPass123!' -C -d CampusLibraryDb \
  -Q "SELECT Id, Title, Author, ISBN, AvailableCopies FROM Books"

# Check Members seed data
docker exec -i campuslibrary-mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'StrongPass123!' -C -d CampusLibraryDb \
  -Q "SELECT Id, FullName, Email FROM Members"

# Check filtered unique index on Loans
docker exec -i campuslibrary-mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'StrongPass123!' -C -d CampusLibraryDb \
  -Q "SELECT name, is_unique, filter_definition FROM sys.indexes WHERE object_id = OBJECT_ID('Loans')"
```

### 6. Build Solution

```bash
dotnet build
```

Expected: Build succeeded with 0 errors.

## Expected Results

After completing these steps:

1. ✓ Domain entities: Book, Member, Loan
2. ✓ LibraryDbContext with 3 DbSets
3. ✓ Migration `InitialCreate` generated
4. ✓ Database `CampusLibraryDb` created in Docker SQL Server
5. ✓ Tables: Books (5 rows), Members (3 rows), Loans (0 rows), __EFMigrationsHistory (1 row)
6. ✓ Constraints verified:
   - Unique ISBN on Books
   - Unique Email on Members
   - Filtered unique index on Loans.BookId WHERE ReturnedAt IS NULL
   - Foreign keys with NO ACTION (Restrict)
7. ✓ Seed data inserted with stable IDs

## Common Problems

| Problem | Solution |
|---------|----------|
| "No DbContext was found" | Ensure `-s` points to WebApi project (startup project) |
| "Login failed for user 'sa'" | Check .env password matches user-secrets connection string |
| "Database 'CampusLibraryDb' already exists" | Migration handles this; or run `dotnet ef database drop` first |
| "Filtered index not supported" | SQL Server 2022 supports filtered indexes; verify version |
| "Cannot drop database" | Ensure no active connections; stop WebApi/WebMvc if running |

## Troubleshooting

### Migration Fails

1. Check EF Core tools installed: `dotnet ef --version`
2. Verify project references: WebApi references Data
3. Check connection string in user-secrets: `dotnet user-secrets list --project src/CampusLibrary.WebApi`

### Database Connection Issues

1. Verify container: `docker compose ps`
2. Check logs: `docker compose logs mssql`
3. Test connectivity: `docker exec -i campuslibrary-mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'StrongPass123!' -C -Q "SELECT 1"`

### Seed Data Not Inserted

1. Check migration file has `InsertData` calls
2. Run `dotnet ef database update` again (idempotent)
3. Check `__EFMigrationsHistory` table for applied migration

## Reset Database (If Needed)

```bash
# Drop database
dotnet ef database drop -p src/CampusLibrary.Data -s src/CampusLibrary.WebApi

# Re-apply
dotnet ef database update -p src/CampusLibrary.Data -s src/CampusLibrary.WebApi
```

**Warning**: This deletes all data. Only use in development.

## Next Steps

Proceed to **Milestone 3**: Implement Books CRUD API endpoints with DTOs and Swagger documentation.