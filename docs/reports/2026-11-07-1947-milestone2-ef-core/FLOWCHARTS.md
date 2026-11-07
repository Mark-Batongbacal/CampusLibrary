# Flowcharts — Milestone 2 Entity Framework Core

## Feature Logic Flowchart

```mermaid
flowchart TD
    A[Start Milestone 2] --> B[Create Domain Entities]
    B --> C[Book.cs]
    B --> D[Member.cs]
    B --> E[Loan.cs]
    
    C --> F[Update LibraryDbContext]
    D --> F
    E --> F
    
    F --> G[Add DbSets]
    G --> H[Configure Fluent API]
    H --> I[Required Fields & MaxLengths]
    H --> J[Unique Indexes: ISBN, Email]
    H --> K[FK Relationships with Restrict]
    H --> L[Filtered Unique Index: Loans.BookId WHERE ReturnedAt IS NULL]
    H --> M[Seed Data: 5 Books, 3 Members]
    
    M --> N[Generate Migration]
    N --> O[dotnet ef migrations add InitialCreate]
    
    O --> P[Review Migration]
    P --> Q{Correct Schema?}
    Q -->|No| R[Fix Configuration]
    R --> N
    Q -->|Yes| S[Apply Migration]
    
    S --> T[dotnet ef database update]
    T --> U{Database Updated?}
    U -->|No| V[Check Connection / Logs]
    V --> T
    U -->|Yes| W[Verify Database]
    
    W --> X[Check Tables Exist]
    W --> Y[Verify Seed Data]
    W --> Z[Verify Indexes & FKs]
    
    X --> AA[All Verified]
    Y --> AA
    Z --> AA
    
    AA --> BB[Run Build]
    BB --> CC{Build Pass?}
    CC -->|No| DD[Fix Errors]
    DD --> BB
    CC -->|Yes| EE[Milestone 2 Complete]
```

## Sequence Diagram — EF Core Migration & Database Setup

```mermaid
sequenceDiagram
    participant Dev as Developer
    participant Data as CampusLibrary.Data
    participant WebApi as CampusLibrary.WebApi (Startup)
    participant EF as EF Core Tools
    participant Docker as Docker Compose
    participant SQL as SQL Server Container
    participant DB as CampusLibraryDb

    Dev->>Data: Create Book.cs, Member.cs, Loan.cs
    Dev->>Data: Update LibraryDbContext.cs
    Data->>Data: Add DbSet<Book>, DbSet<Member>, DbSet<Loan>
    Data->>Data: Configure Fluent API in OnModelCreating
    Data->>Data: Add Seed Data (HasData)
    
    Dev->>EF: dotnet ef migrations add InitialCreate -p Data -s WebApi
    EF->>Data: Scan entities & DbContext
    EF->>Data: Generate migration C# code
    EF->>Data: Create migration files in Migrations/
    
    Dev->>EF: dotnet ef database update -p Data -s WebApi
    EF->>WebApi: Read connection string from user-secrets
    EF->>Docker: Connect to localhost:1433
    Docker->>SQL: Forward connection
    
    SQL->>DB: CREATE DATABASE CampusLibraryDb
    SQL->>DB: CREATE TABLE Books
    SQL->>DB: CREATE TABLE Members
    SQL->>DB: CREATE TABLE Loans
    SQL->>DB: CREATE UNIQUE INDEX IX_Books_ISBN
    SQL->>DB: CREATE UNIQUE INDEX IX_Members_Email
    SQL->>DB: CREATE FILTERED UNIQUE INDEX IX_Loans_BookId WHERE ReturnedAt IS NULL
    SQL->>DB: CREATE FK FK_Loans_Books_BookId (NO ACTION)
    SQL->>DB: CREATE FK FK_Loans_Members_MemberId (NO ACTION)
    SQL->>DB: INSERT 5 Books (SET IDENTITY_INSERT ON)
    SQL->>DB: INSERT 3 Members (SET IDENTITY_INSERT ON)
    SQL->>DB: INSERT INTO __EFMigrationsHistory
    
    SQL-->>EF: Migration applied successfully
    EF-->>Dev: Done.
    
    Dev->>SQL: sqlcmd verification queries
    SQL-->>Dev: Tables, data, indexes, FKs confirmed
```

## ER Diagram — Database Schema

```mermaid
erDiagram
    BOOK ||--o{ LOAN : "has"
    MEMBER ||--o{ LOAN : "borrows"
    
    BOOK {
        int Id PK "Identity(1,1)"
        string Title "Required, MaxLength(200)"
        string Author "Required, MaxLength(200)"
        string ISBN "Required, MaxLength(13), Unique"
        int AvailableCopies "Required, >=0"
    }
    
    MEMBER {
        int Id PK "Identity(1,1)"
        string FullName "Required, MaxLength(200)"
        string Email "Required, MaxLength(200), Unique"
    }
    
    LOAN {
        int Id PK "Identity(1,1)"
        int BookId FK "Required"
        int MemberId FK "Required"
        datetime BorrowedAt "Required"
        datetime ReturnedAt "Nullable"
    }
    
    %% Key Constraints
    %% 1. Unique ISBN on Books
    %% 2. Unique Email on Members
    %% 3. FK Loans.BookId -> Books.Id (ON DELETE NO ACTION / RESTRICT)
    %% 4. FK Loans.MemberId -> Members.Id (ON DELETE NO ACTION / RESTRICT)
    %% 5. Filtered Unique Index on Loans.BookId WHERE ReturnedAt IS NULL
    %%    → Enforces: at most one open (unreturned) loan per book
    
    %% Seed Data (Deterministic IDs)
    %% Books: 1-5 with known ISBNs and AvailableCopies
    %% Members: 1-3 with university emails
```

## Migration Operations Flow

```mermaid
flowchart TD
    A[dotnet ef migrations add InitialCreate] --> B[EF Core Scans Assembly]
    B --> C[Discovers LibraryDbContext]
    C --> D[Analyzes DbSets: Books, Members, Loans]
    D --> E[Processes OnModelCreating]
    
    E --> F[Book Config]
    F --> F1[PK: Id]
    F --> F2[Required: Title, Author, ISBN, AvailableCopies]
    F --> F3[MaxLength: Title(200), Author(200), ISBN(13)]
    F --> F4[Unique Index: IX_Books_ISBN]
    F --> F5[Relationship: Book 1:0..1 Loan]
    
    E --> G[Member Config]
    G --> G1[PK: Id]
    G --> G2[Required: FullName, Email]
    G --> G3[MaxLength: FullName(200), Email(200)]
    G --> G4[Unique Index: IX_Members_Email]
    G --> G5[Relationship: Member 1:* Loans]
    
    E --> H[Loan Config]
    H --> H1[PK: Id]
    H --> H2[Required: BorrowedAt]
    H --> H3[Optional: ReturnedAt]
    H --> H4[FK: BookId -> Books.Id (Restrict)]
    H --> H5[FK: MemberId -> Members.Id (Restrict)]
    H --> H6[Filtered Unique Index: IX_Loans_BookId WHERE ReturnedAt IS NULL]
    H --> H7[Index: IX_Loans_MemberId]
    
    E --> I[Seed Data]
    I --> I1[5 Books with IDs 1-5]
    I --> I2[3 Members with IDs 1-3]
    
    F1 & F2 & F3 & F4 & F5 & G1 & G2 & G3 & G4 & G5 & H1 & H2 & H3 & H4 & H5 & H6 & H7 & I1 & I2 --> J[Generate Up/Down Methods]
    J --> K[Create Migration Files]
    
    K --> L[dotnet ef database update]
    L --> M[Connect to SQL Server]
    M --> N[Create Database]
    N --> O[Execute Up Migration]
    O --> O1[CREATE TABLE Books]
    O --> O2[CREATE TABLE Members]
    O --> O3[CREATE TABLE Loans]
    O --> O4[CREATE INDEXES]
    O --> O5[CREATE FOREIGN KEYS]
    O --> O6[INSERT SEED DATA]
    O --> O7[INSERT MIGRATION HISTORY]
    
    O1 & O2 & O3 & O4 & O5 & O6 & O7 --> P[Migration Complete]
```

## Verification Flowchart

```mermaid
flowchart TD
    A[Start Verification] --> B[Check Database Exists]
    B --> C{SELECT name FROM sys.databases WHERE name = 'CampusLibraryDb'}
    C -->|Not Found| D[FAIL: Database not created]
    C -->|Found| E[Check Tables]
    
    E --> F[SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES]
    F --> G{4 Tables? Books, Members, Loans, __EFMigrationsHistory}
    G -->|No| H[FAIL: Missing tables]
    G -->|Yes| I[Check Books Seed Data]
    
    I --> J[SELECT * FROM Books]
    J --> K{5 Rows? Correct ISBNs? AvailableCopies?}
    K -->|No| L[FAIL: Books seed data incorrect]
    K -->|Yes| M[Check Members Seed Data]
    
    M --> N[SELECT * FROM Members]
    N --> O{3 Rows? Correct Emails?}
    O -->|No| P[FAIL: Members seed data incorrect]
    O -->|Yes| Q[Check Loans Table]
    
    Q --> R[SELECT * FROM Loans]
    R --> S{0 Rows Initially?}
    S -->|No| T[FAIL: Loans should be empty]
    S -->|Yes| U[Check Indexes]
    
    U --> V[Check Unique ISBN Index]
    U --> W[Check Unique Email Index]
    U --> X[Check Filtered Unique Index on Loans.BookId]
    U --> Y[Check FK Constraints]
    
    V --> Z{IX_Books_ISBN Unique?}
    W --> Z
    X --> Z{IX_Loans_BookId Unique WHERE ReturnedAt IS NULL?}
    Y --> Z{FKs NO ACTION?}
    
    Z -->|All Pass| AA[VERIFICATION PASSED]
    Z -->|Any Fail| BB[FAIL: Constraint verification failed]
    
    AA --> CC[Run dotnet build]
    CC --> DD{Build Success?}
    DD -->|Yes| EE[Milestone 2 Verified Complete]
    DD -->|No| FF[FAIL: Build errors]
```

## Project Structure After Milestone 2

```mermaid
flowchart LR
    subgraph Data[CampusLibrary.Data]
        Entities[Entities/]
        Entities --> Book[Book.cs]
        Entities --> Member[Member.cs]
        Entities --> Loan[Loan.cs]
        Context[LibraryDbContext.cs]
        Migrations[Migrations/]
        Migrations --> Mig1[20261107122452_InitialCreate.cs]
        Migrations --> Mig2[20261107122452_InitialCreate.Designer.cs]
        Migrations --> Snapshot[LibraryDbContextModelSnapshot.cs]
    end
    
    subgraph WebApi[CampusLibrary.WebApi]
        Program[Program.cs]
        DI[DI: AddDbContext<LibraryDbContext>]
    end
    
    subgraph WebMvc[CampusLibrary.WebMvc]
        Program2[Program.cs]
        DI2[DI: AddDbContext<LibraryDbContext>]
    end
    
    subgraph DB[(SQL Server in Docker)]
        Tables[Tables]
        Tables --> Books[Books]
        Tables --> Members[Members]
        Tables --> Loans[Loans]
        Tables --> History[__EFMigrationsHistory]
    end
    
    Data -->|ProjectReference| WebApi
    Data -->|ProjectReference| WebMvc
    WebApi -->|Connection String| UserSecrets[User Secrets]
    WebMvc -->|Connection String| UserSecrets
    UserSecrets -->|localhost:1433| DB
    EF[EF Core Tools] -->|Migrations| Data
    EF -->|database update| DB
```