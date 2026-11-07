# Flowcharts — Milestone 1 Initial Setup

## Feature Logic Flowchart

```mermaid
flowchart TD
    A[Start Milestone 1] --> B{Environment Check}
    B -->|Missing .NET| C[Install .NET 9 SDK]
    B -->|Missing Docker| D[Install Docker Engine]
    B -->|Missing Git| E[Install Git]
    B -->|All Present| F[Create Solution Structure]
    
    F --> G[Create .NET Projects]
    G --> H[Data: Class Library]
    G --> I[WebApi: ASP.NET Core Web API]
    G --> J[WebMvc: ASP.NET Core MVC]
    G --> K[Tests: xUnit Project]
    
    H --> L[Add EF Core Packages]
    I --> L
    J --> L
    K --> L
    
    L --> M[Create LibraryDbContext]
    M --> N[Configure DI in Program.cs]
    N --> O[Add Swagger to WebApi]
    
    O --> P[Create Docker Compose]
    P --> Q[Configure SQL Server 2022 Express]
    Q --> R[Add Persistent Volume]
    R --> S[Bind to Localhost Port]
    S --> T[Environment Variables]
    
    T --> U[Create .env.example]
    U --> V[Create .gitignore]
    V --> W[Setup User Secrets]
    
    W --> X[dotnet restore]
    X --> Y[dotnet build]
    Y --> Z{Build Success?}
    Z -->|No| AA[Fix Build Errors]
    AA --> Y
    Z -->|Yes| BB[Start Docker Container]
    
    BB --> CC{docker compose up -d}
    CC -->|Port Conflict| DD[Stop Conflicting Container]
    DD --> CC
    CC -->|Success| EE[Verify SQL Server]
    
    EE --> FF[sqlcmd Test Connection]
    FF --> GG{Connection OK?}
    GG -->|No| HH[Check Logs / Config]
    HH --> FF
    GG -->|Yes| II[Run Tests]
    
    II --> JJ[dotnet test]
    JJ --> KK[Generate Reports]
    KK --> LL[Milestone 1 Complete]
```

## Sequence Diagram — Component Communication

```mermaid
sequenceDiagram
    participant Dev as Developer
    participant Git as Git Repository
    participant Docker as Docker Compose
    participant SQL as SQL Server Container
    participant Data as CampusLibrary.Data
    participant WebApi as CampusLibrary.WebApi
    participant WebMvc as CampusLibrary.WebMvc
    participant Tests as CampusLibrary.Tests
    participant Secrets as User Secrets

    Dev->>Git: git clone / init
    Dev->>Data: Create Class Library (.NET 9)
    Dev->>WebApi: Create Web API (.NET 9)
    Dev->>WebMvc: Create MVC (.NET 9)
    Dev->>Tests: Create Test Project (.NET 9)
    
    Dev->>Data: Add EF Core 9.0 packages
    Dev->>WebApi: Add EF Core + Swashbuckle
    Dev->>WebMvc: Add EF Core
    Dev->>Tests: Add xUnit + Moq + FluentAssertions
    
    Dev->>Data: Create LibraryDbContext
    Dev->>WebApi: Configure DI: AddDbContext
    Dev->>WebMvc: Configure DI: AddDbContext
    Dev->>Secrets: Set ConnectionStrings:DefaultConnection
    
    Dev->>Docker: Create compose.yaml
    Docker->>SQL: Pull mcr.microsoft.com/mssql/server:2022-latest
    Docker->>SQL: Create volume campuslibrary_sql_data
    Docker->>SQL: Start container (port 127.0.0.1:1433)
    
    SQL-->>Docker: Container healthy
    Dev->>SQL: sqlcmd connection test
    SQL-->>Dev: Version info returned
    
    Dev->>Data: dotnet restore
    Dev->>WebApi: dotnet restore
    Dev->>WebMvc: dotnet restore
    Dev->>Tests: dotnet restore
    
    Dev->>Data: dotnet build
    Dev->>WebApi: dotnet build
    Dev->>WebMvc: dotnet build
    Dev->>Tests: dotnet build
    
    Dev->>Tests: dotnet test
    Tests-->>Dev: No tests found (expected)
    
    Dev->>Git: Add files to staging
    Dev->>Git: Reports generated
```

## ER Diagram — Initial Database Schema (Planned for Milestone 2)

```mermaid
erDiagram
    %% These entities will be created in Milestone 2
    %% Shown here as planned architecture
    
    BOOK ||--o{ LOAN : "has"
    MEMBER ||--o{ LOAN : "borrows"
    
    BOOK {
        int Id PK
        string Title "required"
        string Author "required"
        string ISBN UK "unique"
        int AvailableCopies ">=0"
    }
    
    MEMBER {
        int Id PK
        string FullName "required"
        string Email "required, unique"
    }
    
    LOAN {
        int Id PK
        int BookId FK
        int MemberId FK
        datetime BorrowedAt "required"
        datetime ReturnedAt "nullable"
    }
    
    %% Constraint: At most one open loan per book
    %% Enforced via filtered unique index on Loan.BookId WHERE ReturnedAt IS NULL
```

## Project Dependency Diagram

```mermaid
flowchart LR
    subgraph Solution[CampusLibrary.sln]
        Data[CampusLibrary.Data]
        WebApi[CampusLibrary.WebApi]
        WebMvc[CampusLibrary.WebMvc]
        Tests[CampusLibrary.Tests]
    end
    
    Data -->|NuGet| EF[Microsoft.EntityFrameworkCore.SqlServer 9.0.0]
    Data -->|NuGet| EFDesign[Microsoft.EntityFrameworkCore.Design 9.0.0]
    
    WebApi -->|ProjectReference| Data
    WebApi -->|NuGet| EF
    WebApi -->|NuGet| EFDesign
    WebApi -->|NuGet| Swagger[Swashbuckle.AspNetCore 7.2.0]
    
    WebMvc -->|ProjectReference| Data
    WebMvc -->|ProjectReference| WebApi
    WebMvc -->|NuGet| EF
    WebMvc -->|NuGet| EFDesign
    
    Tests -->|ProjectReference| Data
    Tests -->|ProjectReference| WebApi
    Tests -->|ProjectReference| WebMvc
    Tests -->|NuGet| xUnit[xunit 2.9.2]
    Tests -->|NuGet| Moq[Moq 4.20.72]
    Tests -->|NuGet| FluentAssertions[FluentAssertions 6.12.1]
    Tests -->|NuGet| TestSDK[Microsoft.NET.Test.Sdk 17.11.1]
    
    Docker[Docker Compose] --> SQL[SQL Server 2022 Express]
    SQL --> Volume[(campuslibrary_sql_data Volume)]
    
    WebApi -->|ConnectionString| Secrets[User Secrets]
    WebMvc -->|ConnectionString| Secrets
    Docker -->|Env Var| EnvFile[.env file]
    
    style Data fill:#e1f5fe
    style WebApi fill:#fff3e0
    style WebMvc fill:#f3e5f5
    style Tests fill:#e8f5e9
    style SQL fill:#ffebee
    style Volume fill:#f5f5f5
```

## Build and Verification Flow

```mermaid
flowchart TD
    A[dotnet restore] --> B{Restore Success?}
    B -->|No| C[Check NuGet Sources / Network]
    C --> A
    B -->|Yes| D[dotnet build]
    
    D --> E{Build Success?}
    E -->|No| F[Check Compiler Errors]
    F --> D
    E -->|Yes| G[dotnet test]
    
    G --> H{Tests Pass?}
    H -->|N/A| I[No Tests Yet - Expected]
    H -->|Yes| J[All Green]
    H -->|No| K[Fix Failing Tests]
    K --> G
    
    I --> L[docker compose config]
    L --> M{Config Valid?}
    M -->|No| N[Fix YAML / Env Vars]
    N --> L
    M -->|Yes| O[docker compose up -d]
    
    O --> P{Container Running?}
    P -->|No| Q[Check Logs / Port Conflicts]
    Q --> O
    P -->|Yes| R[sqlcmd Connection Test]
    
    R --> S{SQL Responds?}
    S -->|No| T[Wait / Check Container Logs]
    T --> R
    S -->|Yes| U[Milestone 1 Verified]
    
    U --> V[Generate Reports]
```