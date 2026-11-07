# How to Use — Members and Loans API

## Prerequisites
- Debian/Linux with .NET 9 SDK.
- Docker Engine and Docker Compose.
- Repository dependencies restored (`dotnet restore`).
- Development SQL Server password configured in `.env` and connection string in the Web API's .NET user-secrets.
- SQL Server running in Docker.

## Services That Must Be Running
Start SQL Server from the repository root:

```bash
docker compose up -d
docker compose ps
```

Wait until `campuslibrary-mssql` reports `Up`. The database is bound to `127.0.0.1:1433` and persisted in a Docker named volume.

## Run the API

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/CampusLibrary.WebApi --urls http://127.0.0.1:5102
```

Swagger is available at `http://127.0.0.1:5102/swagger` while running in Development. Keep this local server running while using the examples below.

## Endpoints and Examples

### Members

#### List members
```bash
curl -i http://127.0.0.1:5102/api/members
```
Expected: `200 OK` with a JSON array of member DTOs.

#### Get one member
```bash
curl -i http://127.0.0.1:5102/api/members/1
```
Expected: `200 OK` with that member, or `404 Not Found` when the ID does not exist.

#### Create a member
```bash
curl -i -X POST http://127.0.0.1:5102/api/members \
  -H 'Content-Type: application/json' \
  -d '{"fullName":"Dave Wilson","email":"dave.wilson@university.edu"}'
```
Expected: `201 Created`, a JSON member DTO, and a `Location` response header. Duplicate email returns `409 Conflict`; invalid data returns `400 Bad Request`.

#### Update a member
```bash
curl -i -X PUT http://127.0.0.1:5102/api/members/1 \
  -H 'Content-Type: application/json' \
  -d '{"fullName":"Dave Wilson Updated","email":"dave.wilson@university.edu"}'
```
Expected: `204 No Content`; missing ID returns 404, invalid input returns 400, duplicate email returns 409.

#### Delete a member
```bash
curl -i -X DELETE http://127.0.0.1:5102/api/members/1
```
Expected: `204 No Content`; missing ID returns 404; member with loan history returns 409.

### Loans

#### List all loans
```bash
curl -i http://127.0.0.1:5102/api/loans
```
Expected: `200 OK` with a JSON array of loan DTOs including book title, member name, borrowed timestamp, and returned timestamp (null if still open).

#### Get one loan
```bash
curl -i http://127.0.0.1:5102/api/loans/2
```
Expected: `200 OK` with that loan, or `404 Not Found` when the ID does not exist.

#### Borrow a book
```bash
curl -i -X POST http://127.0.0.1:5102/api/loans \
  -H 'Content-Type: application/json' \
  -d '{"memberId":1,"bookId":1}'
```
Expected: `201 Created` with a loan DTO and `Location` header. Returns `404 Not Found` if member or book does not exist, `409 Conflict` if the book already has an open loan.

#### Return a loan
```bash
curl -i -X POST http://127.0.0.1:5102/api/loans/2/return \
  -H 'Content-Type: application/json' \
  -d '{"loanId":2}'
```
Expected: `204 No Content`. Missing loan returns `404 Not Found`; returning an already-returned loan returns `409 Conflict`.

## Run Tests and Build

```bash
dotnet restore
dotnet build
dotnet test
```

Expected: build succeeds and all tests pass (33 tests including pre-existing Books API tests).

## Common Problems and Troubleshooting

| Problem | What to check |
|---|---|
| API reports an uninitialized connection string | Run with `ASPNETCORE_ENVIRONMENT=Development` so user-secrets load. Check with `dotnet user-secrets list --project src/CampusLibrary.WebApi`. |
| SQL connection refused | Run `docker compose ps` and `docker compose logs mssql`; confirm port 1433 is free and container is ready. |
| HTTP 409 creating a member | Email already exists; choose a different email. |
| HTTP 409 borrowing a book | Book already has an open loan; return it first or borrow a different book. |
| HTTP 409 returning a loan | Loan has already been returned. |
| HTTP 409 deleting a member | Member has borrowing records; loan history is preserved. |
| Swagger returns 404 | Ensure the API environment is Development. |
| Port 5102 unavailable | Choose a free local port in the `--urls` argument and update curl URLs accordingly. |

To stop SQL Server while retaining the persistent database volume:
```bash
docker compose down
```
Do not use `docker compose down -v` unless you explicitly intend to delete the database volume.