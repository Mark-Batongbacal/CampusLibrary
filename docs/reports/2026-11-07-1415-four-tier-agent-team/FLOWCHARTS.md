# Four-Tier AI Engineering Team — Flowcharts

## Request Handling
```mermaid
flowchart TD
    A[User request] --> B[Main session: Senior classifies request]
    B --> C{Simple question, search, research, documentation explanation?}
    C -->|Yes| D[Delegate read-only work to Intern]
    C -->|No| E{Routine bounded implementation/debugging?}
    E -->|Yes| F[Assign exclusive file scope to Developer]
    E -->|No| G{Difficult bug, refactor, UI design?}
    G -->|Yes| H[Senior handles and applies relevant skills]
    G -->|No| I[Consult Principal for architecture/security/concurrency/critical escalation]
    D --> J[Senior checks evidence and completeness]
    F --> K[Developer reports changes and verification]
    K --> L[Senior reviews diff, tests, docs, CLAUDE.md compliance]
    H --> L
    I --> M[Senior incorporates recommendation; delegate bounded implementation if needed]
    M --> L
    L --> N{Required checks and reports complete?}
    N -->|No| O[Resolve gaps or disclose blockers]
    O --> L
    N -->|Yes| P[Respond with verified results and limitations]
```

## Claude Code and OmniRoute Sequence
```mermaid
sequenceDiagram
    participant User
    participant Main as Main Claude Code session (Senior)
    participant Skills as Installed skill loader
    participant Intern as Intern subagent
    participant Developer as Developer subagent
    participant Principal as Principal subagent
    participant Route as OmniRoute

    User->>Main: Request
    Main->>Skills: Explicitly invoke Impeccable and relevant skills
    Skills-->>Main: Skill instructions/context
    alt Read-only research or explanation
        Main->>Intern: Bounded research request
        Intern->>Route: Request via configured haiku alias
        Route-->>Intern: Priority target response
        Intern-->>Main: Findings with paths/evidence
    else Routine implementation
        Main->>Developer: Assign files and acceptance criteria
        Developer->>Route: Request via configured sonnet alias
        Route-->>Developer: Priority target response
        Developer-->>Main: Diff summary and test/build results
        Main->>Main: Review code and documentation
    else Critical architecture escalation
        Main->>Principal: Request bounded design/review
        Principal->>Route: Request via configured opus alias
        Route-->>Principal: Priority target response
        Principal-->>Main: Recommendation, risks, rationale
    end
    Main->>Main: Verify requirements, tests, and report accuracy
    Main-->>User: Results and any unverified routing limitations
```

## Routing Facts
- OmniRoute aliases are priority combos; the selected backend can vary with fallback availability.
- `free-smart` has GPT-6 Luna Medium first and Nemotron Ultra second, so it is not a guaranteed Ultra route.
- The main-session model cannot be forced to Nemotron Ultra by the current project instruction file. A direct verified route is a follow-up requirement.
