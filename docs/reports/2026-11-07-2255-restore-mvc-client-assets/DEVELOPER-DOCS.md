# Restore MVC Client Assets

## 1. Summary of the request

Complete the Milestone 5 review by fixing the confirmed runtime defect where the shared MVC layout referenced Bootstrap and jQuery files that were absent from `wwwroot`.

## 2. Work performed

- Restored pinned local Bootstrap 5.3.3 CSS and bundle files.
- Restored pinned local jQuery 3.7.1.
- Included both upstream license files.
- Kept the existing `_Layout.cshtml` paths unchanged because the restored files now match them.
- Verified each referenced static file over a running Development HTTP server.
- Rebuilt the solution, ran all tests, and ran the Impeccable detector over the shared layout and custom stylesheet.

## 3. Projects affected

- `CampusLibrary.WebMvc`
- Project documentation under `docs/reports`

## 4. Files added, modified, or deleted

| File | Change | Reason |
|---|---|---|
| `src/CampusLibrary.WebMvc/wwwroot/lib/bootstrap/dist/css/bootstrap.min.css` | Added | Provide the Bootstrap styles referenced by the shared MVC layout |
| `src/CampusLibrary.WebMvc/wwwroot/lib/bootstrap/dist/js/bootstrap.bundle.min.js` | Added | Provide the Bootstrap JavaScript bundle referenced by the layout |
| `src/CampusLibrary.WebMvc/wwwroot/lib/bootstrap/LICENSE` | Added | Preserve Bootstrap's upstream license notice |
| `src/CampusLibrary.WebMvc/wwwroot/lib/jquery/dist/jquery.min.js` | Added | Provide the jQuery script referenced by the shared layout |
| `src/CampusLibrary.WebMvc/wwwroot/lib/jquery/LICENSE.txt` | Added | Preserve jQuery's upstream license notice |
| `docs/reports/2026-11-07-2255-restore-mvc-client-assets/DEVELOPER-DOCS.md` | Added | Document the runtime asset fix and verification |
| `docs/reports/2026-11-07-2255-restore-mvc-client-assets/HOW-TO-USE.md` | Added | Explain how to run and verify the fixed UI |
| `docs/reports/2026-11-07-2255-restore-mvc-client-assets/FLOWCHARTS.md` | Added | Describe static asset request flow and failure handling |
| `docs/reports/INDEX.md` | Modified | Add this report to the report index |

No files were deleted.

## 5. Important implementation details

- Assets remain local, so the librarian UI does not depend on a public CDN or an internet connection at runtime.
- Bootstrap 5.3.3 matches the utility and component classes already used by the Razor views.
- jQuery 3.7.1 satisfies the existing layout reference; no new application script behavior was added.
- The shared layout required no code change because its original asset paths were correct.
- `UseStaticFiles()` in `Program.cs` serves these files from `wwwroot`.

## 6. Relevant code excerpts

The existing layout references now resolve to real local files:

```html
<link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" asp-append-version="true" />
<script src="~/lib/jquery/dist/jquery.min.js" asp-append-version="true"></script>
<script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js" asp-append-version="true"></script>
```

## 7. Database changes

None.

## 8. Build and test results

- `dotnet restore` — passed; all projects were up to date.
- `dotnet build --no-restore` — passed with **0 warnings and 0 errors**.
- `dotnet test --no-build --no-restore` — passed with **61 passed, 0 failed, 0 skipped**.
- Development server static checks — all returned HTTP 200:
  - `/lib/bootstrap/dist/css/bootstrap.min.css` — 232,803 bytes
  - `/lib/bootstrap/dist/js/bootstrap.bundle.min.js` — 80,721 bytes
  - `/lib/jquery/dist/jquery.min.js` — 87,533 bytes
  - `/css/site.css` — 1,889 bytes
- Impeccable detector on `_Layout.cshtml` and `site.css` — returned `[]`.

## 9. Known limitations

- Browser screenshot and manual keyboard/touch visual inspection were not performed.
- Authentication remains outside Milestone 5; the admin interface must remain restricted to trusted development use.

## 10. Follow-up tasks

- Perform browser-level visual and accessibility QA when browser tooling is available.
- Add authentication and authorization before exposing the admin interface beyond trusted development use.
