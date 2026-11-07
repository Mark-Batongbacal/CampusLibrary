# Using the CampusLibrary AI Engineering Team

## Prerequisites
- Claude Code 2.1.319 or compatible, in the CampusLibrary repository.
- Enabled project skills: `impeccable`, `andrej-karpathy-skills`, `ponytail`, and `agent-skills`.
- OmniRoute running at the configured local endpoint if using these model aliases.

No database or application service is required for agent configuration itself.

## Services That Must Be Running
- OmniRoute should be running for model requests through its aliases.
- MSSQL / CampusLibrary services are only needed for delegated work that actually requires database/application execution.

## Step-by-Step Instructions
1. Open a **new Claude Code session** in the repository so it loads `.claude/agents/` and updated `CLAUDE.md`.
2. Use the main conversation as Senior Engineer/coordinator. Before each task, explicitly invoke Impeccable using `/impeccable` (or the Skill tool selecting `impeccable:impeccable`).
3. Delegate read-only search or research to `intern`.
4. Delegate bounded implementation to `developer`; list exact files so agents do not edit the same files concurrently.
5. Call `principal` for architectural or critical escalations. It is not intended for routine implementation.
6. Senior reviews code, tests, documentation, and project instruction compliance before completion.

## Commands to Run the Feature
This is Claude Code configuration rather than an application endpoint. Verify the CLI's agent flags with:

```bash
claude --help | grep -E -- '--agent|--agents'
```

To inspect active/running sessions:

```bash
claude agents --json
```

This does not necessarily enumerate saved subagent definitions. Invoke a configured agent from the Claude Code interface by its name after opening a fresh session.

## Example Usage
- “Intern, search for where the MVC project configures the database; report file and line references only.”
- “Developer, implement the assigned feature only in `src/...` and `tests/...`; run the focused tests and report commands/results.”
- “Principal, evaluate whether the proposed concurrency strategy preserves the filtered unique-index invariant; do not modify files.”
- Main session: invoke Impeccable, classify the request, delegate or handle, review outcomes, and report any unverified model route.

## Expected Results
- Intern can inspect/search but its frontmatter allowlist omits write and shell tools.
- Developer and Principal use the configured model-family aliases if those aliases are honored by the gateway.
- OmniRoute priority combos can route to a fallback target; an alias/model family label alone does not prove which backend handled a specific request.
- The main session acts as Senior by project instructions; no Senior subagent is required.

## Common Problems
- **Agent not recognized:** restart/open a fresh Claude Code session in the repository.
- **Wrong backend appears in logs:** aliases are priority combos; inspect OmniRoute routing evidence and do not call them fixed model assignments.
- **Senior not on Nemotron Ultra:** direct Ultra selection was not verified in this configuration. Do not assume `opus` or `free-smart` guarantees Ultra.
- **Impeccable not used:** instructions do not auto-run skills. Invoke `/impeccable` or the `Skill` tool for every main-session task.
- **Intern appears able to write:** stop relying on it, inspect the live tool list/config in a fresh session, and do not treat prompt-only read-only wording as enforcement.

## Troubleshooting Instructions
- Use `/agents` or the agent selection interface to inspect/invoke a role; CLI `claude agents --json` primarily reports active sessions.
- For route verification, monitor `/home/mark/.omniroute/logs/application/app.log` around a harmless agent request. Avoid dumping call logs because they may include sensitive prompt/input data.
- Do not alter OmniRoute mappings or global model configuration without an explicit decision about the intended direct Ultra target.
- For database-dependent tasks only, start MSSQL from repository root with `docker compose up -d`; inspect status with `docker compose ps`. Never use `docker compose down -v` unless database volume deletion is explicitly authorized.
