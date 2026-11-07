# Milestones 6–8 implementation and UI update

## Request and work
Implemented Android foundation and borrowing from CLAUDE.md, reviewed API/database integration and updated MVC UI. Applied local Impeccable context/craft/native guidance, Karpathy Guidelines and Ponytail. Existing milestone scope supplied requirements; SPEC-milestones6-8.md records acceptance and implementation order. Claude-specific model routing is unavailable in Codex; no route/backend claims are made.

## Projects affected and files
| File | Change | Reason |
|---|---|---|
| `android/CampusLibraryApp/settings.gradle.kts`, root/app build files, gradle.properties | Added | Reproducible Android Kotlin/Compose build |
| `android/CampusLibraryApp/gradlew`, gradlew.bat, gradle/wrapper/* | Added | Generated Gradle wrapper |
| `android/CampusLibraryApp/app/src/main/AndroidManifest.xml` | Added | Internet permission, activity, release cleartext disabled |
| `android/CampusLibraryApp/app/src/debug/AndroidManifest.xml`, res/xml/network_security_config.xml | Added | Local emulator HTTP only in debug |
| `android/CampusLibraryApp/app/src/main/java/edu/campus/library/LibraryRepository.kt` | Added | Retrofit contract, repository, timeout policy |
| `android/CampusLibraryApp/app/src/main/java/edu/campus/library/LibraryViewModel.kt` | Added | StateFlow, saved member selection, error/retry/borrow state |
| `android/CampusLibraryApp/app/src/main/java/edu/campus/library/MainActivity.kt` | Added | Material catalog/details/member selection/loan history |
| `android/CampusLibraryApp/app/src/test/java/edu/campus/library/*Test.kt` | Added | State transitions, isolation, duplicate submit, network recovery, HTTP serialization |
| `android/CampusLibraryApp/app/src/main/res/drawable/ic_library.xml`, res/xml/data_extraction_rules.xml | Added | Native vector launcher icon and explicit backup policy |
| `android/CampusLibraryApp/README.md`, `SPEC-milestones6-8.md`, `tasks/plan-milestones6-8.md` | Added | Setup and acceptance criteria |
| `.gitignore` | Updated | Exclude Gradle/local SDK/build/keystore files |
| `src/CampusLibrary.Data/Book.cs`, LibraryDbContext.cs, Migrations/* | Updated/added | Correct one-to-many loan history relationship; metadata migration |
| `src/CampusLibrary.WebApi/Controllers/BooksController.cs`, DTOs/BookDto.cs | Updated | Derive isAvailable from inventory and open loans |
| `src/CampusLibrary.WebApi/Controllers/LoansController.cs`, DTOs/BorrowBookRequest.cs, ReturnBookRequest.cs | Updated | Zero-inventory guard, positive IDs, atomic return, matching route/body IDs |
| `src/CampusLibrary.WebApi/Program.cs` | Updated | Support local Development HTTP for emulator |
| `src/CampusLibrary.WebMvc/Controllers/BooksController.cs` | Updated | Use collection-compatible open-loan query |
| `src/CampusLibrary.WebMvc/CampusLibrary.WebMvc.csproj` | Updated | Remove unused API reference causing Add book form to target API and show JSON |
| `src/CampusLibrary.WebMvc/Views/_ViewStart.cshtml`, shared layout, Home/Books/Members/Loans views, wwwroot/css/site.css | Added/updated | Shared styling/navigation, responsive accessible refinement; remove duplicate manual antiforgery tokens |
| `tests/CampusLibrary.Tests/BooksControllerTests.cs`, MembersLoansControllerTests.cs, LoansMvcControllerTests.cs | Updated | Availability/history/validation/return tests, nullable corrections |
| `tasks/integration-review.md`, `tasks/ui-review.md` | Added | Detailed actual verification evidence |

## Implementation details
Android uses Compose → ViewModel → repository → Retrofit → API. It never performs networking inside composables. Member selection is explicit development identification, not authentication; SavedStateHandle survives recreation, not a fresh task. My loans filters the current unauthenticated API collection locally; this is not authorization. Borrow submission is disabled while loading/busy and immediately updates availability/history after success. Cancellation propagates. User-facing errors do not expose exception text. API availability is derived; stored AvailableCopies remains inventory with one open loan maximum. Release defaults to a nonfunctional HTTPS placeholder and must be configured before distribution.

```kotlin
val memberId = current.memberId ?: return
if (current.loading || current.borrowing) return
```

The MVC bug was reproduced in rendered HTML: form action `/api/books` with duplicate antiforgery inputs. Removing the API project reference prevents discovery of API controllers inside MVC; Razor form tag helpers supply one antiforgery token.

## Database changes
Reviewed/applied `CorrectBookLoanRelationship`: empty Up/Down, model metadata and migration history only. No table/index/data changes. Existing filtered unique open-loan index remains. SQL transaction probes rolled back; identity counters may advance. No existing records deleted/reset.

## Verification
.NET restore/build passed, final build 0 warnings/errors; tests 69 passed after MVC route fix. Live API GET books/detail/members/loans/Swagger 200; invalid IDs 400, missing resources 404. SQL filtered index and return/reborrow history probes passed. Full exact commands and exploratory failures/resolutions are in tasks/integration-review.md.

Live rendered MVC Add book targets `/Books/Create` with exactly one antiforgery field; duplicate ISBN and blank/negative submissions return 200 HTML validation with input preserved. Members/Create and Books/Edit targets and token counts verified. No records changed by these form checks. An existing user MVC process on port 5003 must restart to load rebuilt code; latest reviewed server is port 5081.

MVC Impeccable detector returned []; Firefox screenshots inspected at 1440×1000 Books and 390×844 Create. Missing shared _ViewStart was found and fixed. Initial Production database GET 500 had no connection configuration; Development with existing user-secrets returned 200. No authentication was added. Screenshot artifacts are temporary /tmp/campus-mvc-desktop.png and /tmp/campus-mvc-mobile.png.

Android commands executed from `android/CampusLibraryApp` with `ANDROID_HOME=/home/mark/Android/Sdk`:

- Generated wrapper with installed Gradle 9.1.0: initial distribution URL validation failed; wrapper generation with `--no-validate-url` passed. The generated wrapper uses the official Gradle distribution URL.
- Initial `--offline assembleDebug testDebugUnitTest lintDebug` failed on uncached Compose dependencies; online resolution downloaded them.
- Initial AAR validation failed because lifecycle 2.11.0 needs SDK 37/AGP 9.1+. Pinned lifecycle 2.10.0 for SDK 36/AGP 9.0.1; validation then passed.
- Initial complete run assembled APK and passed 4 tests but lint failed with 2 missing includeSubdomains errors, plus 8 warnings. Explicit false attributes fixed the errors; added native icon and explicit backup policies. Replaced deprecated TabRow with PrimaryTabRow.
- `./gradlew assembleDebug testDebugUnitTest lintDebug --offline --console plain`: passed after fixes. Four tests passed (3 ViewModel, 1 Retrofit MockWebServer). Lint has 0 errors; dependency/target-version notices remain (pinned compatibility choices, not suppressed).
- SDK tooling prints an XML version mismatch warning (command-line SDK parser versus installed SDK metadata); build succeeds.
- Final APK: `android/CampusLibraryApp/app/build/outputs/apk/debug/app-debug.apk` (~11 MB, ignored build artifact). Unit-test XML and lint HTML/text are under app/build.
- `adb devices`: no devices; `emulator -list-avds`: no configured AVDs; no SDK system images installed. No emulator connectivity or rendered Android review could be performed.
- `git diff --check`: passed. No commit/push performed.

Two redundant Android build processes were stopped to avoid memory pressure while the single final verification ran. Incidental setup commands using `python` (rather than installed `python3`) or invoking gradlew from the repository root failed and were rerun from the correct environment; these did not change verification outcomes.

## Limits and follow-up
No Android emulator/device configured; no Android rendered or emulator-host connectivity review claimed. Android minimum SDK is 26. Screen-reader/keyboard automation and every browser/state combination remain unverified. Authentication and production hosting/signing remain outside scope. Claude-specific slash-command tools were unavailable; local instruction files and executable Impeccable tooling were used.

## Toolchain references
AGP 9 uses built-in Kotlin per the [official Android migration guide](https://developer.android.com/build/migrate-to-built-in-kotlin). Compose uses the [official compiler plugin setup](https://developer.android.com/develop/ui/compose/setup-compose-dependencies-and-compiler). Versions are pinned; lint may report newer releases.
