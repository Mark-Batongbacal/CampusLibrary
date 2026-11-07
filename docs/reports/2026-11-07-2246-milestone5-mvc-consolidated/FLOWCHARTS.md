# Milestone 5 — MVC Admin: Diagrams

## Feature Logic (flowchart TD)

```mermaid
flowchart TD
    A[User opens /Books] --> B[GET Books/Index]
    B --> C{Books exist?}
    C -- No --> D[Render empty list]
    C -- Yes --> E[Render books table: Title, Author, ISBN, Copies, Add/Edit/Delete]

    E --> F[User clicks Add book]
    F --> G[GET Books/Create]
    G --> H[Render add-book form]
    H --> I[User submits form]
    I --> J[POST Books/Create + antiforgery]
    J --> K{ModelState valid?}
    K -- No --> L[Redisplays form with validation errors, input preserved]
    K -- Yes --> M{ISBN already exists?}
    M -- Yes --> N[Add ISBN model error, redisplays form]
    M -- No --> O[Create book, SaveChanges]
    O --> P{Save succeeded?}
    P -- Unique violation 2601/2627 --> N
    P -- Yes --> Q[Redirect to Index PRG]
    Q --> E

    E --> R[User clicks Edit on a book]
    R --> S[GET Books/Edit/{id}]
    S --> T{Book exists?}
    T -- No --> U[404 Not Found]
    T -- Yes --> V[Render edit form prefilled]
    V --> W[User submits form]
    W --> X[POST Books/Edit/{id} + antiforgery]
    X --> Y{ModelState valid and id matches?}
    Y -- No --> Z[NotFound or redisplays form]
    Y -- Yes --> AA{ISBN duplicated on another book?}
    AA -- Yes --> AB[Add ISBN model error, redisplays form]
    AA -- No --> AC[Update book fields, SaveChanges]
    AC --> AD{Save succeeded?}
    AD -- Unique violation --> AB
    AD -- Yes --> AE[Redirect to Index PRG]
    AE --> E

    E --> AF[User clicks Delete on a book]
    AF --> AG[POST Books/Delete/{id} + antiforgery]
    AG --> AH{Book exists?}
    AH -- No --> AI[404 Not Found]
    AH -- Yes --> AJ{Open loan exists?}
    AJ -- Yes --> AK[Block delete, conflict TempData, redirect to Index]
    AJ -- No --> AL[Remove book, SaveChanges]
    AL --> AM{Save succeeded?}
    AM -- FK violation 547 --> AN[Conflict TempData, redirect to Index]
    AM -- Yes --> AO[Redirect to Index PRG]
    AO --> E

    style L fill:#fff3e0,stroke:#ef6c00
    style N fill:#fff3e0,stroke:#ef6c00
    style U fill:#fff3e0,stroke:#ef6c00
    style Z fill:#fff3e0,stroke:#ef6c00
    style AK fill:#fdecea,stroke:#c62828
    style AN fill:#fdecea,stroke:#c62828
```

```mermaid
flowchart TD
    A[User opens /Members] --> B[GET Members/Index]
    B --> C{Members exist?}
    C -- No --> D[Render empty list]
    C -- Yes --> E[Render members table: Full Name, Email]

    E --> F[User clicks Add member]
    F --> G[GET Members/Create]
    G --> H[Render add-member form]
    H --> I[User submits form]
    I --> J[POST Members/Create + antiforgery]
    J --> K{ModelState valid?}
    K -- No --> L[Redisplays form with validation errors, input preserved]
    K -- Yes --> M{Email already exists?}
    M -- Yes --> N[Add email model error, redisplays form]
    M -- No --> O[Create member, SaveChanges]
    O --> P{Save succeeded?}
    P -- Unique violation 2601/2627 --> N
    P -- Yes --> Q[Redirect to Details PRG]
    Q --> R[Render member details + borrowing history]

    E --> S[User clicks a member]
    S --> T[GET Members/Details/{id}]
    T --> U{Member exists?}
    U -- No --> V[404 Not Found]
    U -- Yes --> W[Render details with loan history]
```

```mermaid
flowchart TD
    A[User opens /Loans] --> B[GET Loans/Index]
    B --> C{Open loans exist?}
    C -- No --> D[Render empty-state card]
    C -- Yes --> E[Render table: Book, Author, Member, Borrowed at, Return button]

    E --> F[User clicks Return on a loan]
    F --> G[POST Loans/Return/{id} + antiforgery token]
    G --> H{Antiforgery valid?}
    H -- No --> I[400 invalid request]
    H -- Yes --> J{Loan exists?}
    J -- No --> K[Error redirect: Loan not found]
    J -- Yes --> L{Already returned?}
    L -- Yes --> M[Error redirect: already returned, no change]
    L -- No --> N[Set ReturnedAt = now where ReturnedAt IS NULL]
    N --> O{Rows affected?}
    O -- 0 --> M
    O -- >0 --> P[Success TempData, redirect to Index PRG]
    P --> E

    style D fill:#e9f7ef,stroke:#2e7d32
    style I fill:#fff3e0,stroke:#ef6c00
    style K fill:#fff3e0,stroke:#ef6c00
    style M fill:#fdecea,stroke:#c62828
```

## Communication (sequenceDiagram)

```mermaid
sequenceDiagram
    participant U as User (librarian)
    participant B as Browser
    participant C as MVC Controller (Books/Members/Loans)
    participant D as LibraryDbContext (EF Core)
    participant S as SQL Server (CampusLibraryDb)

    note over U,S: Books - list/add/edit/delete
    U->>B: Open /Books
    B->>C: GET Index
    C->>D: Query books, order by title
    D->>S: SELECT books
    S-->>D: Rows
    D-->>C: Entities projected to BookSummary
    C-->>B: Render books table
    B-->>U: Show list

    U->>B: Submit Add/Edit book form
    B->>C: POST Create/Edit + antiforgery
    C->>D: Check ISBN uniqueness (AnyAsync)
    D->>S: SELECT by ISBN
    S-->>D: Row / none
    D-->>C: Result
    alt duplicate
        C-->>B: Redisplays form + ISBN error
    else unique
        C->>D: Add/Update book
        C->>S: INSERT / UPDATE (SaveChanges)
        S-->>C: Success
        C-->>B: Redirect to Index PRG
        B-->>U: Updated list
    end

    U->>B: Click Delete on a book with open loan
    B->>C: POST Delete/{id} + antiforgery
    C->>D: Find book, include Loan
    D->>S: SELECT book + loan
    S-->>D: Book row
    D-->>C: Book entity
    alt open loan
        C-->>B: Conflict TempData, redirect Index
        B-->>U: "Cannot delete — open loan"
    else no open loan
        C->>D: Remove book
        C->>S: DELETE (SaveChanges)
        S-->>C: Success
        C-->>B: Redirect Index
        B-->>U: Book removed
    end

    note over U,S: Members - list/details/add
    U->>B: Open /Members
    B->>C: GET Index
    C->>D: Query members, order by name
    D->>S: SELECT members
    S-->>D: Rows
    D-->>C: Projected MemberSummary
    C-->>B: Render members list
    B-->>U: Show list

    U->>B: Click a member
    B->>C: GET Details/{id}
    C->>D: Find member Include(Loans).ThenInclude(Book)
    D->>S: SELECT member + loans + books
    S-->>D: Rows
    alt missing
        D-->>C: null
        C-->>B: 404 Not Found
    else found
        D-->>C: Member + loans
        C-->>B: Render details + borrowing history
        B-->>U: Show member details
    end

    U->>B: Submit Add member form
    B->>C: POST Create + antiforgery
    C->>D: Check email uniqueness (AnyAsync)
    D->>S: SELECT by email
    S-->>D: Row / none
    D-->>C: Result
    alt duplicate
        C-->>B: Redisplays form + email error
    else unique
        C->>D: Add member
        C->>S: INSERT (SaveChanges)
        S-->>C: Success
        C-->>B: Redirect to Details PRG
        B-->>U: Member details
    end

    note over U,S: Loans - list/return
    U->>B: Open /Loans
    B->>C: GET Index
    C->>D: Query loans where ReturnedAt IS NULL, include Book/Member
    D->>S: SELECT open loans
    S-->>D: Rows
    D-->>C: Projected OpenLoanSummary
    C-->>B: Render open-loans table + empty state
    B-->>U: Show list

    U->>B: Click Return on a loan
    B->>C: POST Return/{id} + antiforgery
    C->>D: Find loan by id
    D->>S: SELECT loan
    S-->>D: Loan row
    alt missing
        C-->>B: Error redirect "Loan not found"
    else already returned
        C-->>B: Error redirect "already returned"
    else open loan
        C->>D: ExecuteUpdateAsync Set ReturnedAt = now where id and ReturnedAt IS NULL
        C->>S: UPDATE loan
        S-->>C: Rows affected
        C-->>B: Success TempData, redirect Index PRG
        B-->>U: Open-loans list, returned loan removed
    end
```

## Entity Relationship (erDiagram)

```mermaid
erDiagram
    Book ||--o{ Loan : contains
    Member ||--o{ Loan : has
    Loan }o--|| Book : linked_to
    Loan }o--|| Member : borrowed_by

    Book {
        int Id PK
        string Title
        string Author UK
        string ISBN UK
        int AvailableCopies
    }

    Member {
        int Id PK
        string FullName
        string Email UK
    }

    Loan {
        int Id PK
        int BookId FK
        int MemberId FK
        datetime BorrowedAt
        datetime? ReturnedAt
    }

    %% Constraint: at most one open loan per book
    %% (filtered unique index on Loan.BookId where ReturnedAt IS NULL)
```