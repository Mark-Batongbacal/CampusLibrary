# How to Use the Restored MVC Client Assets

## Prerequisites

- Debian Linux or another Linux environment
- .NET 9 SDK
- Docker Engine and Docker Compose for the SQL Server-backed MVC pages
- A configured `ConnectionStrings:DefaultConnection` user-secret for `CampusLibrary.WebMvc`

## Services that must be running

Start SQL Server from the repository root:

```bash
docker compose up -d
docker compose ps
```

Do not use `docker compose down -v`; that would delete the persistent database volume.

## Step-by-step instructions

1. Restore and build:

   ```bash
   dotnet restore
   dotnet build --no-restore
   ```

2. Run the tests:

   ```bash
   dotnet test --no-build --no-restore
   ```

3. Start the MVC application:

   ```bash
   ASPNETCORE_ENVIRONMENT=Development \
     dotnet run --project src/CampusLibrary.WebMvc
   ```

4. Open the URL printed by `dotnet run`, then visit `/`, `/Books`, `/Members`, and `/Loans`.

## Commands to verify the assets

Replace `<mvc-url>` with the HTTP address printed by `dotnet run`:

```bash
curl -I <mvc-url>/lib/bootstrap/dist/css/bootstrap.min.css
curl -I <mvc-url>/lib/bootstrap/dist/js/bootstrap.bundle.min.js
curl -I <mvc-url>/lib/jquery/dist/jquery.min.js
curl -I <mvc-url>/css/site.css
```

Each request should return `HTTP/1.1 200 OK`.

## Example usage

Open `/Books`. The page should show the shared black Metro shell, responsive navigation, a styled table, and Bootstrap-formatted controls. The same shell and responsive utilities apply to Members and Loans.

## Expected results

- The solution builds with no warnings or errors.
- All 61 tests pass.
- Bootstrap CSS, the Bootstrap bundle, jQuery, and `site.css` return HTTP 200.
- Razor pages retain their intended Bootstrap layout and Metro styling.

## Common problems

- **Static file returns 404:** Ensure the command is running `src/CampusLibrary.WebMvc`, not the Web API project.
- **Page returns 500:** Start SQL Server and verify the MVC `DefaultConnection` user-secret.
- **Browser shows old styles:** Perform a normal reload first; the layout uses `asp-append-version`, so changed assets receive a new version query string.
- **HTTPS redirect warning:** Use the exact URL printed by `dotnet run`.

## Troubleshooting

Check the local files:

```bash
find src/CampusLibrary.WebMvc/wwwroot/lib -type f -print
```

Then rebuild and restart the MVC app:

```bash
dotnet build src/CampusLibrary.WebMvc/CampusLibrary.WebMvc.csproj
dotnet run --project src/CampusLibrary.WebMvc
```

Do not commit `.env`, passwords, user-secrets, or connection strings containing credentials.
