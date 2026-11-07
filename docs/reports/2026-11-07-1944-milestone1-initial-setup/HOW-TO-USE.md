# How to Use — CampusLibrary Milestone 1

## Prerequisites

- **.NET 9 SDK** (9.0.320 or later)
- **Docker Engine** and **Docker Compose**
- **Git**
- **Bash** (Linux environment)

## Services That Must Be Running

### SQL Server (Docker)

The database runs in a Docker container managed by Docker Compose.

```bash
# From the repository root
docker compose up -d
```

Verify it's running:
```bash
docker compose ps
```

Expected output:
```
NAME                  IMAGE                                        COMMAND                  SERVICE   CREATED         STATUS         PORTS
campuslibrary-mssql   mcr.microsoft.com/mssql/server:2022-latest   "/opt/mssql/bin/laun…"   mssql     2 minutes ago   Up 2 minutes   127.0.0.1:1433->1433/tcp
```

Check logs if needed:
```bash
docker compose logs -f mssql
```

## Step-by-Step Instructions

### 1. Clone and Setup (First Time Only)

```bash
git clone <repository-url>
cd CampusLibrary
```

### 2. Configure Environment Variables

Copy the example file and set a strong password:
```bash
cp .env.example .env
# Edit .env and replace REPLACE_WITH_STRONG_PASSWORD with a strong password
```

The password must meet SQL Server requirements:
- At least 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character

### 3. Start SQL Server

```bash
docker compose up -d
```

Wait for SQL Server to fully start (check logs: `docker compose logs mssql` — look for "Recovery is complete").

### 4. Restore and Build

```bash
dotnet restore
dotnet build
```

Expected: Build succeeded with 0 errors.

### 5. Run Tests (Optional)

```bash
dotnet test
```

Note: No tests exist yet in Milestone 1 — this will show "No test is available".

### 6. Run Web API (Development)

```bash
cd src/CampusLibrary.WebApi
dotnet run
```

The API will start on `https://localhost:7xxx` and `http://localhost:5xxx` (ports shown in output).
Swagger UI available at `https://localhost:7xxx/swagger` in Development.

### 7. Run Web MVC (Development)

```bash
cd src/CampusLibrary.WebMvc
dotnet run
```

The MVC app will start on `https://localhost:7xxx` and `http://localhost:5xxx`.

## Example Usage

### Verify SQL Server Connection

```bash
docker exec -i campuslibrary-mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'StrongPass123!' -C \
  -Q "SELECT @@VERSION"
```

### Check Databases

```bash
docker exec -i campuslibrary-mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'StrongPass123!' -C \
  -Q "SELECT name FROM sys.databases"
```

## Expected Results

After completing these steps:

1. ✓ Docker container `campuslibrary-mssql` is running
2. ✓ Port 1433 accessible on localhost only
3. ✓ SQL Server 2022 Express responding to queries
4. ✓ All 4 .NET projects build successfully
5. ✓ Solution structure matches CLAUDE.md architecture
6. ✓ User secrets configured for connection strings

## Common Problems

| Problem | Solution |
|---------|----------|
| Port 1433 already in use | Stop existing SQL Server container: `docker stop <container-id>` |
| Docker permission denied | Ensure user is in docker group: `sudo usermod -aG docker $USER` (re-login required) |
| SQL Server fails to start | Check logs: `docker compose logs mssql` — may need more memory (2GB+ recommended) |
| Build fails with CS5001 | Ensure Program.cs exists in WebApi and WebMvc projects |
| Swagger not available | Only available in Development environment (`ASPNETCORE_ENVIRONMENT=Development`) |
| Connection string errors | Verify user-secrets: `dotnet user-secrets list --project <project-path>` |

## Troubleshooting

### SQL Server Won't Start

1. Check Docker logs: `docker compose logs mssql`
2. Ensure at least 2GB RAM available for Docker
3. Verify password meets complexity requirements
4. Try: `docker compose down && docker compose up -d`

### Build Errors

1. Run `dotnet restore` first
2. Check .NET version: `dotnet --version` (should be 9.x)
3. Clear NuGet cache: `dotnet nuget locals all --clear`

### Port Conflicts

If port 1433 is in use:
```bash
# Find what's using it
ss -tlnp | grep 1433

# Stop conflicting container
docker stop <container-name>
```

### Connection Refused

1. Verify container is running: `docker compose ps`
2. Verify port mapping: `docker port campuslibrary-mssql`
3. Test from host: `telnet localhost 1433` or `nc -zv localhost 1433`

## Stopping Services

```bash
# Stop SQL Server (preserves data in volume)
docker compose down

# Stop and remove volume (DELETES ALL DATA)
docker compose down -v
```

**Warning**: `docker compose down -v` permanently deletes the database. Only use when explicitly authorized.