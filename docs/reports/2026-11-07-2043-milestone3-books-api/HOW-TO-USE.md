# How to Use — Books API

## Prerequisites
- Debian/Linux with .NET 9 SDK.
- Docker Engine and Docker Compose.
- Repository dependencies restored (`dotnet restore`).
- Development SQL Server password configured in `.env` and connection string in the Web API's .NET user-secrets.

## Services That Must Be Running
Start SQL Server from the repository root:

```bash
docker compose up -d
docker compose ps
```

Wait until `campuslibrary-mssql` reports `Up`. The database is bound to `127.0.0.1:1433` and persisted in a Docker named volume.

## Run the API

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/CampusLibrary.WebApi --urls http://127.0.0.1:5080
```

Swagger is available at `http://127.0.0.1:5080/swagger` while running in Development. Keep this local server running while using the examples below.

## Endpoints and Examples

### List books
```bash
curl -i http://127.0.0.1:5080/api/books
```
Expected: `200 OK` and a JSON array of book DTOs.

### Get one book
```bash
curl -i http://127.0.0.1:5080/api/books/1
```
Expected: `200 OK` with that book, or `404 Not Found` when the ID does not exist.

### Create a book
```bash
curl -i -X POST http://127.0.0.1:5080/api/books \
  -H 'Content-Type: application/json' \
  -d '{"title":"Example Book","author":"Example Author","isbn":"9780000000123","availableCopies":2}'
```
Expected: `201 Created`, a JSON DTO, and a `Location` response header. Duplicate ISBN returns `409 Conflict`; invalid data returns `400 Bad Request`.

### Update a book
```bash
curl -i -X PUT http://127.0.0.1:5080/api/books/1 \
  -H 'Content-Type: application/json' \
  -d '{"title":"Updated Title","author":"Example Author","isbn":"9780000000124","availableCopies":3}'
```
Expected: `204 No Content`; a missing ID returns 404, invalid input returns 400, duplicate ISBN returns 409.

### Delete a book
```bash
curl -i -X DELETE http://127.0.0.1:5080/api/books/1
```
Expected: `204 No Content`; a missing ID returns 404, and a book with loan records returns 409.

## Run Tests and Build

```bash
dotnet restore
dotnet build
dotnet test
```

Expected: build succeeds and all 10 tests pass.

## Common Problems and Troubleshooting

| Problem | What to check |
|---|---|
| API reports an uninitialized connection string | Run with `ASPNETCORE_ENVIRONMENT=Development` so user-secrets load. Check with `dotnet user-secrets list --project src/CampusLibrary.WebApi`. |
| SQL connection refused | Run `docker compose ps` and `docker compose logs mssql`; confirm port 1433 is free and container is ready. |
| HTTP 409 for a new ISBN | ISBN is already present; choose a different ISBN. |
| HTTP 409 on delete | A loan references the book; the API does not delete loan history. |
| Swagger returns 404 | Ensure the API environment is Development. |
| Port 5080 unavailable | Choose a free local port in the `--urls` argument and update curl URLs accordingly. |

To stop SQL Server while retaining the persistent database volume:
```bash
docker compose down
```
Do not use `docker compose down -v` unless you explicitly intend to delete the database volume.