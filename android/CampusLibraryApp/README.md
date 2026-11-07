# CampusLibrary Android

Open this directory in Android Studio. Requires JDK 17+, Android SDK 36 and SDK 26+ device. Gradle wrapper is included. Set `ANDROID_HOME` to your SDK directory or let Android Studio create ignored `local.properties`.

From the repository root, start the existing database and API:

```bash
docker compose up -d
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/CampusLibrary.WebApi --urls http://0.0.0.0:5080
```

Configure `ConnectionStrings:DefaultConnection` with .NET user-secrets as documented in earlier reports. Never commit credentials. The database migration commands are in CLAUDE.md.

```bash
cd android/CampusLibraryApp
./gradlew assembleDebug testDebugUnitTest lintDebug
adb install -r app/build/outputs/apk/debug/app-debug.apk
```

Debug uses `http://10.0.2.2:5080/api/`, the Android emulator's host address. Only debug permits local HTTP. A physical device requires local networking configuration or `adb reverse tcp:5080 tcp:5080` and changing the debug URL to localhost. Release configuration accepts HTTPS only:

```bash
./gradlew assembleRelease -PAPI_BASE_URL=https://your-library.example/api/
```

The default release URL is deliberately nonfunctional (`example.invalid`); supply your actual HTTPS deployment and configure signing before distribution.

Browse Catalog, tap a book, choose a development member, and tap Borrow book. My loans shows only the selected member's current and returned loans. Librarians return books through MVC. Refresh retrieves changes made elsewhere. Network failures show a retry action; conflicts report availability changes. The member selector is development identification and provides no authentication or access control. Selection survives activity/process recreation through SavedStateHandle, but a fresh task starts without a member selected.
