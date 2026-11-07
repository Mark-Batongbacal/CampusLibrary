# MVC UI review — milestones 6–8

## Scope and guidance

Refined the existing metro dark/cyan design, preserving the user-selected direction in `.impeccable/surfaces/src-campuslibrary-webmvc.md`. Read local Impeccable SKILL.md, ran `impeccable context --target src/CampusLibrary.WebMvc`, and applied craft-floor and polish guidance. No structured question tool is available in this child session; the authorized scope and existing surface direction supplied the design brief. No new product capabilities or branding introduced.

## Work

- Added missing `Views/_ViewStart.cshtml`: previously non-home views had no shared layout, navigation, Bootstrap or site CSS. This was confirmed by an actual Firefox screenshot, then fixed.
- Shared dark theme tokens; readable muted/error colors; native focus, selection and caret styling; consistent controls, table hover and semantic alerts.
- Active navigation with `aria-current`, mobile navigation labels, skip link and keyboard-accessible scrolling table regions.
- Constrained form widths, 44px principal controls, wrapped page headers, responsive spacing.
- Home uses direct workflow rows with actual database counts instead of repetitive tiles.
- Loans inherits the same visual system; return uses Razor routing tag helpers, success/error announcements.
- Removed Members/Create duplicate inline CSS that overrode shared colors and made outline button text black.
- Contextual accessible names on edit/delete/member links; consistent borrowing-history heading; form autocomplete for name/email.
- Inventory labels distinguish static stored copies from availability (derived by the API from current open loans).
- Batched screenshot review caught wrapping identifiers and actions; added no-wrap identifiers and grouped actions.

## Verification

- `dotnet build src/CampusLibrary.WebMvc --no-restore` passed three times, 0 warnings, 0 errors (includes Data/API references). Parent will run final full solution checks.
- `impeccable detect --json src/CampusLibrary.WebMvc/Views src/CampusLibrary.WebMvc/wwwroot/css/site.css` returned `[]`.
- Live `Development` MVC GET `/Books` returned 200 using existing user-secrets; `/Books/Create` 200.
- Initial `Production` run `/Books` returned 500 because production has no configured connection string; stopped it and reran under Development. No credential changes were made.
- Firefox headless desktop screenshot 1440×1000 `/Books` and phone 390×844 `/Books/Create` captured and visually inspected. Correct shared layout/CSS, active navigation, readable forms and table were verified. Screenshots are in `/tmp/campus-mvc-desktop.png` and `/tmp/campus-mvc-mobile.png` (review artifacts, not committed project files).
- First Firefox attempt failed because an existing browser owned its profile; a separate temporary profile resolved it.
- No destructive forms submitted during visual verification; no data modified.

## Limits

Full keyboard automation, screen-reader testing and all page/state/browser combinations were not performed. Final identifier/action wrapping and inventory label changes were rebuilt successfully; desktop Books recaptured and inspected after restart, showing readable unbroken ISBNs and actions on one line. Existing design lacks a DESIGN.md; this scoped refinement preserves the established surface brief rather than replacing the global design system.

## Follow-up: MVC form routing verification

Main session removed the unused MVC-to-WebApi project reference that caused discovery of API controllers inside MVC, plus duplicate manually emitted antiforgery inputs. Restarted the owned MVC server at 5081 against the latest build.

Executed `python3 /tmp/check-campus-mvc-forms.py` with actual HTTP, cookie handling and HTML form parsing:

- GET `/Books/Create` renders form action `/Books/Create` and exactly one antiforgery input.
- POST duplicate existing ISBN `9780132350884` with valid antiforgery returns **200 text/html**, visible `A book with this ISBN already exists.`, preserved submitted title, and shared layout.
- POST blank required fields and negative copy count returns **200 text/html**, field validation and shared layout.
- GET `/Members/Create` action `/Members/Create` and GET `/Books/Edit/1` action `/Books/Edit/1`, each exactly one antiforgery input.
- No successful creation, deletion, or other data mutation attempted during these checks.

The separate pre-existing MVC process at port 5003 was left untouched by this child; it may need restart by its owner to load the routing fix.
