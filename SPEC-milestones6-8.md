# Milestones 6–8: Android and integration

Implement the Android scope already specified in CLAUDE.md: Kotlin, Compose, SDK 26+, Retrofit, repository, ViewModel and StateFlow; catalog, detail, explicit development member selection, borrowing and member-filtered loan history. Preserve the .NET architecture and database history. No authentication is implied.

Acceptance: loading/error/empty/retry states; duplicate borrow disabled; cancellation propagated; selection survives recreation; actual API availability used; debug emulator HTTP only, release requires HTTPS configuration. Run Android assembleDebug, unit tests and lint; .NET restore/build/test; safe live integration when available. Record missing emulator or service checks honestly.

Implementation order: Gradle foundation → API/repository → state and tests → Material UI → integration review → reports. Android lives in android/CampusLibraryApp, with local unit tests in app/src/test. No secrets, resets, commits or production deployment.
