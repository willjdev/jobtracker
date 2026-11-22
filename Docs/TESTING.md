# Testing JobTracker API

This guide explains the purpose, organization, execution, and limitations of JobTracker's automated tests. It covers the **service-level test project** and the **HTTP integration test project**.

> This document describes the test code and configuration inspected for the repository and the supplied `JobTracker.Api.IntegrationTests` source archive. Test execution has not been independently verified in this documentation.

## Testing approach

The projects intentionally test different boundaries:

| Layer | What it verifies | What it does not fully verify |
| --- | --- | --- |
| `JobTracker.Api.Tests` | Service behavior, EF Core queries, business rules, filtering, pagination, and persistence against SQLite in-memory | HTTP routing, middleware, request binding, or production SQL Server behavior |
| `JobTracker.Api.IntegrationTests` | API requests and responses through ASP.NET Core, test authentication, services, DI, validation, and persistence | Production JWT cryptographic validation, external hosting, or production SQL Server compatibility |

Although the first project is named `Tests` and its tests are often described as *unit tests*, the use of a real EF Core `DbContext` with SQLite means many tests are best described as **service/component tests with a relational database**.

## Directory layout

```text
JobTracker.Api.Tests/
├── Helpers/
└── Services/

JobTracker.Api.IntegrationTests/
├── Authentication/
│   └── TestAuthHandler.cs
├── Controllers/
│   ├── CompaniesController/
│   │   ├── CompaniesGetTests.cs
│   │   ├── CompaniesPostTests.cs
│   │   ├── CompaniesPutTests.cs
│   │   └── CompaniesDeleteTests.cs
│   ├── JobApplicationController/
│   │   ├── JobApplicationGetTests.cs
│   │   ├── JobApplicationPostTests.cs
│   │   ├── JobApplicationPutTests.cs
│   │   └── JobApplicationDeleteTests.cs
│   └── ApplicationNoteController/
│       ├── ApplicationNoteGetTests.cs
│       ├── ApplicationNotePostTests.cs
│       ├── ApplicationNotePutTests.cs
│       └── ApplicationNoteDeleteTests.cs
├── TestData/
│   ├── CompanyTestData.cs
│   ├── JobApplicationTestData.cs
│   └── ApplicationNoteTestData.cs
├── JobTrackerWebApplicationFactory.cs
└── JobTracker.Api.IntegrationTests.csproj
```

## Integration-test infrastructure

### `JobTrackerWebApplicationFactory`

The custom factory derives from `WebApplicationFactory<Program>` and starts the ASP.NET Core application in a test host, using `TestServer` to process HTTP requests without hosting the API on an external port.

It configures the application for tests by:

1. Replacing the application's original `ApiDbContext` registration with **SQLite in-memory**.
2. Opening a `SqliteConnection` registered as a singleton for the lifetime of that factory.
3. Calling `Database.EnsureCreated()` to initialize the test schema.
4. Creating two test users through `UserManager<ApplicationUser>` (`test-user-id` and `test-user-id2`).
5. Registering `TestAuthHandler` as the default authentication and challenge scheme.

A typical test creates its own factory and client:

```csharp
using var factory = new JobTrackerWebApplicationFactory();
using var client = factory.CreateClient();
```

**Isolation:** each new factory gets its own SQLite connection and database. Tests must not share the same factory or `DbContext` inadvertently. The singleton connection is shared **within one factory**, so separate DI scopes can access the same in-memory data during the same test.

### Seeding test data

`TestData` classes create the entities needed to exercise each resource, including records owned by different users. A test typically seeds a database in a short-lived scope:

```csharp
using (var scope = factory.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    await JobApplicationTestData.SeedDataAsync(db);
}
```

After an HTTP write request, use a **new scope** and a new `ApiDbContext` to verify persisted results. This makes the verification independent of entities tracked by the setup context.

### `TestAuthHandler`

The test handler deliberately bypasses real JWT validation. It builds an authenticated `ClaimsPrincipal` with a `ClaimTypes.NameIdentifier` claim.

| Header | Observed test behavior |
| --- | --- |
| No test headers | Authenticates as `test-user-id` |
| `X-Test-UserId: test-user-id2` | Authenticates as the second test user |
| `X-Test-Unauthenticated: false` | Returns `AuthenticateResult.NoResult()`, enabling `401 Unauthorized` tests |

These headers are used **only inside the integration-test host**. Production authentication uses JWT bearer tokens. The handler does not verify signatures or check whether a supplied header's user ID exists in Identity.

## Covered scenarios

The refactored integration-test archive contains **56 `[Fact]` tests** across three resource areas. That count describes source methods, not a verified pass count.

### Companies (23 tests)

| Operation | Coverage |
| --- | --- |
| GET/list | Authenticated listing, empty results, user ownership, filters, pagination, invalid pagination values, sorting |
| GET/by ID | Existing record, nonexistent record, another user's record |
| POST | Valid creation and persistence, invalid website, unauthenticated request |
| PUT | Valid update, nonexistent company, another user's company |
| DELETE | Valid deletion, nonexistent company, another user's company |

### Job applications (17 tests)

| Operation | Coverage |
| --- | --- |
| GET/list | Authenticated and unauthenticated access, empty and populated results, user ownership, pagination |
| GET/filters | Position with sorting and `CompanyId` filtering |
| GET/by ID | Existing record, nonexistent record, another user's record |
| POST | Valid creation and association, invalid URL, company owned by another user |
| PUT | Valid update and unauthorized ownership of an existing resource |
| DELETE | Valid deletion and unauthorized ownership of an existing resource |

### Application notes (16 tests)

| Operation | Coverage |
| --- | --- |
| GET/list | Existing notes, access restricted by user, unauthenticated request |
| GET/by ID | Existing note, nonexistent note, another user's note |
| POST | Valid creation and association, nonexistent parent application, another user's application, unauthenticated request |
| PUT | Valid update, invalid request data, nonexistent note, another user's note |
| DELETE | Valid deletion, nonexistent note, another user's note |

These tests cover representative risks; they are **not** an exhaustive proof of every parameter combination or possible failure mode. Auth controller scenarios and real JWT token verification are not part of the supplied integration-test suite.

## Selecting representative tests

The test suite emphasizes scenarios that can catch distinct classes of regressions:

- **HTTP contracts:** expected status codes, request binding, and response serialization.
- **Relational persistence:** writes appear in SQLite and rejected writes do not create or alter records.
- **Resource ownership:** one user cannot read or modify another user's companies, job applications, or notes.
- **Parent ownership:** an application cannot be created for someone else's company; a note cannot be created for someone else's application.
- **Query composition:** filters, sorting, and paginated responses reach the API correctly.
- **Validation:** malformed or invalid requests receive appropriate responses and do not persist unintended records.

Detailed query edge cases can remain in service-level tests, while representative HTTP tests verify the full pipeline.

## Running tests

Run from the **repository root**, where `JobTracker.api.slnx` is located:

```bash
# Entire solution
dotnet test

# Service-level tests only
dotnet test JobTracker.Api.Tests/JobTracker.Api.Tests.csproj

# HTTP integration tests only
dotnet test JobTracker.Api.IntegrationTests/JobTracker.Api.IntegrationTests.csproj

# Filter by fully qualified test name
dotnet test JobTracker.Api.IntegrationTests/JobTracker.Api.IntegrationTests.csproj --filter "FullyQualifiedName~CompaniesGetTests"

# Collect coverage with the installed Coverlet collector
dotnet test --collect:"XPlat Code Coverage"
```

The tests use SQLite in-memory, so **they do not require starting SQL Server or running `dotnet run`**. The API application itself still needs the necessary configuration to initialize its host, including development JWT configuration when applicable.

## CI on GitHub Actions

The `.github/workflows/ci.yml` workflow runs on pushes and pull requests to `main` and executes:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-restore --verbosity normal
```

The workflow sets up .NET **10**. Because it runs the solution's tests, it is intended to execute both service and HTTP integration tests as long as both projects remain included in the solution.

**Configuration note:** `Program.cs` reads `Jwt:Key` during startup. Ensure the CI workflow can supply suitable **test-only configuration**, without storing a secret in the repository. Verify this on an actual GitHub Actions run; the current workflow file does not explicitly set JWT environment variables.

## Test design and maintenance guidelines

1. Use explicit **Arrange / Act / Assert** sections so the scenario is easy to follow.
2. Seed only the entities required to expose the behavior under test.
3. For ownership checks, make the foreign resource **exist** and belong to a different user; an arbitrary missing ID would not prove authorization.
4. For rejected writes, check both the HTTP response and that the database remains unchanged.
5. When testing pagination, use a stable ordering and compare page contents, not only the page metadata.
6. Avoid exact assertions on database-generated IDs unless the ID itself is part of the behavior being tested.
7. Keep `TestData` helpers deterministic and do not use shared mutable state across tests.
8. Use SQLite-in-memory test results as evidence for the tested relational behavior, but do not equate them with proof of SQL Server-specific compatibility.

## Known boundaries and next steps

- The HTTP integration suite **simulates authentication**. Separate tests would be needed to exercise real token issuance and JWT bearer validation end to end.
- Testing with SQLite may not catch SQL Server-specific differences in SQL translation, constraints, or migrations.
- The integration fixture uses `EnsureCreated()` rather than testing the migration process.
- Coverage collection is available, but no coverage percentage is claimed here.
- The supplied archive and repository sources were inspected, but **no pass/fail results have been asserted without executing the suite**.
