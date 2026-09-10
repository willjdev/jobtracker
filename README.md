# JobTracker API

[![.NET Core CI](https://github.com/willjdev/jobtracker/actions/workflows/ci.yml/badge.svg)](https://github.com/willjdev/jobtracker/actions/workflows/ci.yml)

JobTracker API is a RESTful Web API built with ASP.NET Core for managing companies, job applications, and application notes.

This project was created as a backend portfolio project to practice real-world API development with C#, Entity Framework Core, SQL Server, service-based architecture, DTOs, dynamic filtering, sorting, pagination, automated tests, and continuous integration with GitHub Actions.

## Features

* Manage companies.
* Manage job applications.
* Add notes to job applications.
* Filter companies by name, location, creation date, and related job application position.
* Filter job applications by position, status, application date, and company.
* Sort companies and job applications by selected fields.
* Paginate list responses.
* Return pagination metadata such as current page, records per page, total records, and total pages.
* Use DTOs to separate API contracts from database entities.
* Use services and interfaces to separate business logic from controllers.
* Use Entity Framework Core with SQL Server for data persistence.
* Use SQLite in-memory database for service tests.
* Run automated tests with xUnit.
* Run build and tests automatically with GitHub Actions.

## Tech Stack

* C#
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* SQLite in-memory for tests
* xUnit
* GitHub Actions

## Project Structure

```text
jobtracker
├── .github
│   └── workflows
│       └── ci.yml
│
├── JobTracker.api
│   ├── Controllers
│   ├── Data
│   ├── DTOs
│   ├── Migrations
│   ├── Models
│   ├── Services
│   ├── Program.cs
│   └── appsettings.json
│
├── JobTracker.Api.Tests
│   ├── Helpers
│   └── Services
│
└── JobTracker.api.slnx
```

## Architecture Overview

The project follows a simple layered structure:

```text
Controller -> Service Interface -> Service -> DbContext -> Database
```

### Controllers

Controllers handle HTTP requests and responses. They receive input from the API, call the corresponding service, and return the appropriate HTTP response.

### Services

Services contain the application logic, including filtering, sorting, pagination, creation, update, deletion, and mapping entities to DTOs.

### Data Access

Entity Framework Core is used to interact with the database through `ApiDbContext`.

## Main Entities

### Company

Represents a company where a job application can be submitted.

Main fields:

```text
Id
Name
Description
Website
Location
CreatedAt
JobApplications
```

### JobApplication

Represents a job application submitted to a company.

Main fields:

```text
Id
Position
Status
AppliedAt
JobUrl
CompanyId
Company
ApplicationNotes
```

### ApplicationNote

Represents a note related to a specific job application.

Main fields:

```text
Id
Content
CreatedAt
JobApplicationId
JobApplication
```

## Getting Started

### Prerequisites

Make sure you have installed:

* .NET SDK 10 or later
* SQL Server or SQL Server LocalDB
* Git

### Clone the Repository

```bash
git clone https://github.com/willjdev/jobtracker.git
cd jobtracker
```

### Restore Dependencies

```bash
dotnet restore
```

### Configure the Database

The API uses SQL Server through the `DefaultConnection` connection string in `appsettings.json`.

Default local configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLlocalDB; Database=Jobtracker.apiDB; Trusted_Connection=True; TrustServerCertificate=True;"
  }
}
```

Update the connection string if your SQL Server setup is different.

### Apply Migrations

```bash
dotnet ef database update --project JobTracker.api
```

### Run the API

```bash
dotnet run --project JobTracker.api
```

The API runs locally using the launch settings defined in the project.

HTTP:

```text
http://localhost:5400
```

HTTPS:

```text
https://localhost:7300
```

In development, the OpenAPI document is available at:

```text
https://localhost:7300/openapi/v1.json
```

## API Endpoints

### Companies

```http
GET    /api/companies
GET    /api/companies/{id}
POST   /api/companies
PUT    /api/companies/{id}
DELETE /api/companies/{id}
```

Example query with filters, sorting, and pagination:

```http
GET /api/companies?name=Microsoft&fieldName=name&sortByType=asc&page=1&records=4
```

Available query parameters:

```text
name
location
createdAt
jobApplicationPosition
fieldName
sortByType
page
records
```

### Job Applications

```http
GET    /api/applications
GET    /api/applications/{id}
POST   /api/applications
PUT    /api/applications/{id}
DELETE /api/applications/{id}
```

Example query with filters, sorting, and pagination:

```http
GET /api/applications?status=Applied&fieldName=appliedAt&sortByType=desc&page=1&records=4
```

Available query parameters:

```text
position
status
appliedAt
companyId
fieldName
sortByType
page
records
```

### Application Notes

```http
GET    /api/application-notes
GET    /api/application-notes/{id}
POST   /api/application-notes
PUT    /api/application-notes/{id}
DELETE /api/application-notes/{id}
```

## Paginated Response Example

List endpoints return paginated responses using this structure:

```json
{
  "items": [],
  "page": 1,
  "records": 4,
  "totalRecords": 20,
  "totalPages": 5
}
```

## Testing

The project includes automated tests for the service layer.

Tested services:

```text
CompanyService
JobApplicationService
ApplicationNoteService
```

The tests use:

* xUnit
* Entity Framework Core
* SQLite in-memory database

Run all tests:

```bash
dotnet test
```

## Continuous Integration

The repository uses GitHub Actions to automatically restore, build, and test the solution on every push or pull request to the `main` branch.

Workflow file:

```text
.github/workflows/ci.yml
```

The workflow runs:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-restore --verbosity normal
```

## What I Practiced

Through this project, I practiced:

* Building RESTful APIs with ASP.NET Core.
* Creating controllers and route-based endpoints.
* Using Entity Framework Core with SQL Server.
* Modeling one-to-many relationships.
* Creating request and response DTOs.
* Separating business logic into services.
* Using interfaces and dependency injection.
* Building dynamic queries with LINQ.
* Implementing filtering, sorting, and pagination.
* Returning paginated API responses.
* Writing automated tests for the service layer.
* Using SQLite in-memory for relational tests.
* Setting up continuous integration with GitHub Actions.

## Next Steps

Planned improvements:

* Add authentication and authorization.
* Add user-specific job applications.
* Add global exception handling middleware.
* Add integration tests for controllers.
* Improve API documentation with request and response examples.
* Deploy the API to a cloud provider.
