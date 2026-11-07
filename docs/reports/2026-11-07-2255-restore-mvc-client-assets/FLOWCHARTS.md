# Restore MVC Client Assets — Flowcharts

## Feature logic

```mermaid
flowchart TD
    A[Browser requests an MVC page] --> B[ASP.NET Core renders _Layout.cshtml]
    B --> C[Browser requests Bootstrap CSS, site.css, jQuery, and Bootstrap bundle]
    C --> D{File exists under wwwroot?}
    D -- No --> E[Static file middleware returns 404]
    E --> F[Page loses Bootstrap styling or client behavior]
    D -- Yes --> G[Static file middleware returns 200]
    G --> H[Browser applies Bootstrap and Metro styles]
    H --> I[Responsive librarian UI is displayed]
```

## Component communication

```mermaid
sequenceDiagram
    participant U as Librarian browser
    participant M as MVC application
    participant R as Razor layout
    participant S as Static file middleware
    participant W as wwwroot

    U->>M: GET /Books
    M->>R: Render Books view in _Layout
    R-->>U: HTML with versioned local asset URLs
    U->>S: GET /lib/bootstrap/dist/css/bootstrap.min.css
    S->>W: Read Bootstrap CSS
    W-->>S: File content
    S-->>U: 200 CSS
    U->>S: GET /css/site.css
    S->>W: Read Metro stylesheet
    W-->>S: File content
    S-->>U: 200 CSS
    U->>S: GET /lib/jquery/dist/jquery.min.js
    S->>W: Read jQuery
    W-->>S: File content
    S-->>U: 200 JavaScript
    U->>S: GET /lib/bootstrap/dist/js/bootstrap.bundle.min.js
    S->>W: Read Bootstrap bundle
    W-->>S: File content
    S-->>U: 200 JavaScript
    U->>U: Render styled responsive page
```

## Database changes

No database tables or relationships changed, so no entity-relationship diagram is required for this asset-only fix.
