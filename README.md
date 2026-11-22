# JobTracker API

A RESTful API built with **ASP.NET Core 10** to organize and track job applications, the companies behind them, and notes recorded during the application process.

JobTracker is a backend portfolio project focused on API design, authentication, user-specific data access, relational persistence, automated testing, and maintainable application code.

## Features

- **Companies:** create, retrieve, update, and delete companies.
- **Job applications:** track positions, application dates, statuses, links, and associated companies.
- **Application notes:** keep notes linked to individual job applications.
- **Accounts and access control:** register, sign in, and authenticate with JWT bearer tokens using ASP.NET Core Identity.
- **User-specific data:** restrict access to companies, applications, and notes according to the authenticated user.
- **Query capabilities:** filtering, sorting, and pagination for company and job-application listings.
- **Validation and API responses:** request DTOs, input validation, HTTP status codes, and pagination metadata.
- **Automated testing and CI:** service-level tests, HTTP integration tests, and a GitHub Actions build-and-test workflow.

## Tech stack

| Area | Technology |
| --- | --- |
| Language and framework | C#, .NET 10, ASP.NET Core Web API |
| Persistence | Entity Framework Core, SQL Server |
| Identity and authentication | ASP.NET Core Identity, JWT bearer authentication |
| API contracts and queries | DTOs, LINQ, filtering, sorting, pagination |
| Automated tests | xUnit, SQLite in-memory, `WebApplicationFactory`, `TestServer`, `HttpClient` |
| Continuous integration | GitHub Actions |
| API specification | ASP.NET Core OpenAPI in Development |

## Architecture

The API uses controllers, services, and Entity Framework Core, with interfaces registered through dependency injection.

```text
HTTP request
    |
    v
ASP.NET Core middleware (authentication / authorization)
    |
    v
Controller -> Service interface / implementation -> ApiDbContext -> SQL Server
    |
    v
HTTP response (DTO / status code)
```

**Core entities and relationships:**

- `ApplicationUser`: the account associated with user-owned records.
- `Company`: a company tracked by a user; can have multiple job applications.
- `JobApplication`: a position/application associated with a company and user; can have multiple notes.
- `ApplicationNote`: a note associated with a job application and user.

The API validates ownership of resources, including when creating applications for companies or notes for job applications.

## Repository structure

```text
jobtracker/
├── .github/
│   └── workflows/
│       └── ci.yml
├── JobTracker.api/
│   ├── Controllers/
│   ├── Data/
│   ├── Dtos/                  # Request/response DTOs (casing may vary)
│   ├── Migrations/
│   ├── Models/
│   ├── Services/
│   └── Program.cs
├── JobTracker.Api.Tests/
│   ├── Helpers/
│   └── Services/
├── JobTracker.Api.IntegrationTests/
│   ├── Authentication/
│   │   └── TestAuthHandler.cs
│   ├── Controllers/
│   │   ├── CompaniesController/
│   │   ├── JobApplicationController/
│   │   └── ApplicationNoteController/
│   ├── TestData/
│   └── JobTrackerWebApplicationFactory.cs
├── docs/
│   └── TESTING.md
└── JobTracker.api.slnx
```

## Getting started

### Requirements

- **.NET SDK 10**
- **SQL Server** or **SQL Server LocalDB**
- Git
- EF Core CLI tools for database migrations (`dotnet-ef`)

### Clone and restore

```bash
git clone https://github.com/willjdev/jobtracker.git
cd jobtracker
dotnet restore
```

### Configure SQL Server

The API reads its connection string from `ConnectionStrings:DefaultConnection`. The repository currently includes a LocalDB configuration in `JobTracker.api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLlocalDB; Database=Jobtracker.apiDB; Trusted_Connection=True; TrustServerCertificate=True;"
  }
}
```

Use a connection string appropriate for your local SQL Server environment. Avoid committing credentials or production secrets.

### Configure JWT settings

The API requires configuration for `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, and `Jwt:ExpireInMinutes`. **Do not commit a real signing key.** For local development, you can use .NET user secrets:

```bash
dotnet user-secrets init --project JobTracker.api
dotnet user-secrets set "Jwt:Key" "REPLACE_WITH_A_LONG_RANDOM_DEVELOPMENT_ONLY_SECRET" --project JobTracker.api
dotnet user-secrets set "Jwt:Issuer" "JobTracker" --project JobTracker.api
dotnet user-secrets set "Jwt:Audience" "JobTrackerClients" --project JobTracker.api
dotnet user-secrets set "Jwt:ExpireInMinutes" "60" --project JobTracker.api
```

Replace the example key with a securely generated, sufficiently long random value. User secrets are intended for local development, **not** production secret storage. Set `ASPNETCORE_ENVIRONMENT=Development` when using development user secrets.

### Apply migrations

```bash
dotnet ef database update --project JobTracker.api
```

If `dotnet ef` is not available, install the EF Core CLI tool compatible with the project's EF Core version.

### Run the API

```bash
dotnet run --project JobTracker.api
```

Use the local URLs displayed by the application or defined in its launch settings. In Development, ASP.NET Core exposes an OpenAPI document at `/openapi/v1.json`.

## Authentication

The authentication controller exposes:

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `POST` | `/api/auth/register` | Register an account |
| `POST` | `/api/auth/login` | Sign in and receive a JWT |

Example registration request:

```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "Test",
  "lastname": "User",
  "email": "test@example.com",
  "password": "ExamplePassword123"
}
```

Example login request:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "ExamplePassword123"
}
```

A successful authentication response includes a token. Send it to protected endpoints using the `Authorization` header:

```http
Authorization: Bearer <YOUR_JWT_TOKEN>
```

The test-only headers described in `docs/TESTING.md` are **not** production authentication mechanisms.

## API endpoints

| Resource | Method | Endpoint |
| --- | --- | --- |
| Companies | `GET` | `/api/companies` |
| Companies | `GET` | `/api/companies/{id}` |
| Companies | `POST` | `/api/companies` |
| Companies | `PUT` | `/api/companies/{id}` |
| Companies | `DELETE` | `/api/companies/{id}` |
| Job applications | `GET` | `/api/applications` |
| Job applications | `GET` | `/api/applications/{id}` |
| Job applications | `POST` | `/api/applications` |
| Job applications | `PUT` | `/api/applications/{id}` |
| Job applications | `DELETE` | `/api/applications/{id}` |
| Application notes | `GET` | `/api/application-notes` |
| Application notes | `GET` | `/api/application-notes/{id}` |
| Application notes | `POST` | `/api/application-notes` |
| Application notes | `PUT` | `/api/application-notes/{id}` |
| Application notes | `DELETE` | `/api/application-notes/{id}` |

### Filtering, sorting, and pagination

The company listing supports query parameters such as `name`, `location`, `createdAt`, `jobApplicationPosition`, `fieldName`, `sortByType`, `page`, and `records`.

The job-application listing supports `position`, `status`, `appliedAt`, `companyId`, `fieldName`, `sortByType`, `page`, and `records`.

Example:

```http
GET /api/applications?position=Developer&fieldName=position&sortByType=desc&page=1&records=4
Authorization: Bearer <YOUR_JWT_TOKEN>
```

Paginated endpoints return a response shaped like:

```json
{
  "items": [],
  "page": 1,
  "records": 4,
  "totalRecords": 0,
  "totalPages": 0
}
```

This is a **structure example**, not the response to the preceding request. The application-notes listing is read as a list in the current integration tests rather than a paginated response.

## Testing

JobTracker has two complementary test projects:

| Project | Scope | Infrastructure |
| --- | --- | --- |
| `JobTracker.Api.Tests` | Service behavior, querying, CRUD rules, and persistence with EF Core | xUnit + SQLite in-memory |
| `JobTracker.Api.IntegrationTests` | Real HTTP endpoints, routing, DTOs, authentication/authorization behavior, services, and persistence | xUnit + `WebApplicationFactory` / `TestServer` + SQLite in-memory |

Run the complete solution's tests from the repository root:

```bash
dotnet test
```

Run one project:

```bash
dotnet test JobTracker.Api.Tests/JobTracker.Api.Tests.csproj
dotnet test JobTracker.Api.IntegrationTests/JobTracker.Api.IntegrationTests.csproj
```

Read **[Testing documentation](docs/TESTING.md)** for the test strategy, database isolation, authentication simulation, covered scenarios, and useful commands.

## Continuous integration

The workflow in `.github/workflows/ci.yml` runs on pushes and pull requests targeting `main`. It uses .NET SDK **10**, restores dependencies, builds, and executes the solution's tests:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-restore --verbosity normal
```

CI verifies the build and tests; a deployment pipeline is not documented here.

## Engineering focus

This project provides practical experience with API design, EF Core relationships, dependency injection, JWT-based authentication, per-user access checks, validation, paginated querying, isolated relational tests, and continuous integration.

## Future improvements

Potential next steps include deployment, richer request/response examples, global exception-handling improvements, and additional end-to-end coverage for the **real JWT registration/login flow**. The current HTTP integration suite uses a dedicated test authentication handler rather than testing JWT validation itself.
