# Run CampusLibrary

Prerequisites: .NET 9, Docker Compose, JDK 17+, Android Studio/SDK 36. Keep database credentials in user-secrets; follow CLAUDE.md and earlier setup reports.

```bash
docker compose up -d
dotnet ef database update -p src/CampusLibrary.Data -s src/CampusLibrary.WebApi
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/CampusLibrary.WebApi --urls http://0.0.0.0:5080
```

In another terminal:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/CampusLibrary.WebMvc --urls http://127.0.0.1:5081
```

Open http://127.0.0.1:5081. Books lets librarians add/edit/delete; Members manages member records; Loans returns books. Add book should submit to /Books/Create and redirect back to Books, not show JSON. Duplicate ISBN/invalid input retains the form with readable errors. A book with history cannot be deleted.

```bash
cd android/CampusLibraryApp
export ANDROID_HOME="$HOME/Android/Sdk"
./gradlew assembleDebug testDebugUnitTest lintDebug
adb install -r app/build/outputs/apk/debug/app-debug.apk
```

Create/start an Android Studio emulator (SDK 26+). Catalog shows titles/authors/availability; open a book, choose a development member, Borrow book, then My loans to see the record. Return through MVC and refresh Android to see returned history and restored availability. Member selection is not sign-in.

Troubleshooting: verify Docker is healthy and both .NET projects have Development connection strings. Emulator uses 10.0.2.2:5080, not localhost. Verify API Swagger on the host, then emulator networking. Refresh after edits elsewhere. Duplicate ISBN gets validation; an already borrowed book gets a conflict. If SDK dependencies fail to download, check network and rerun Gradle. Set API_BASE_URL to your HTTPS API for release; see android/CampusLibraryApp/README.md. Never expose the unauthenticated development system publicly.
