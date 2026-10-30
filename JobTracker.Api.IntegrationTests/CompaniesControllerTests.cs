using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos.Common;
using JobTracker.Api.Dtos.CompanyDto;
using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.Api.IntegrationTests.Controllers;

public class CompaniesControllerTests
{
    private async Task SeedDataAsync(ApiDbContext context)
    {
        var userId = "test-user-id";
        var companies = new List<Company>
        {
            new()
            {
                Id = 1,
                Name = "Microsoft",
                Description = "Big Company",
                Website = "www.microsoft.com",
                Location = "Holand",
                UserId = userId,
                CreatedAt = new DateTime(2026, 7, 10, 6, 10, 0),
            },
            new()
            {
                Id = 2,
                Name = "Santa Monica",
                Description = "Game Company",
                Website = "www.santamonica.com",
                Location = "Remote",
                UserId = userId,
                CreatedAt = new DateTime(2026, 7, 20, 7, 0, 0),
            },
            new()
            {
                Id = 3,
                Name = "Naughty Dog",
                Description = "Game Company",
                Website = "www.naughtydog.com",
                Location = "Remote",
                UserId = "test-user-id2",
                CreatedAt = new DateTime(2026, 8, 28, 8, 0, 0),
            }
        };
        
        await context.Companies.AddRangeAsync(companies);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_WhenUserIsAuthenticated_ReturnsOk()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/companies");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WhenCompaniesExist_ReturnsCompanies()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
        }

        //Act
        var response = await client.GetAsync("api/companies");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result =  await response.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, c => c.Name == "Microsoft");
        Assert.Contains(result.Items, c => c.Name == "Santa Monica");
        Assert.DoesNotContain(result.Items, c => c.Name == "Naughty Dog");
    }

    [Fact]
    public async Task GetAll_WhenNoCompaniesExist_ReturnsEmptyList()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/companies");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();

        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetAll_WhenUserDoesNotHasAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Unauthenticated", "false");

        // Act
        var response = await client.GetAsync("/api/companies");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WhenPageSizeIsSpecified_ReturnsLimitedItems()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
        }

        // Act
        var response = await client.GetAsync("/api/companies?page=1&records=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(2, result.TotalRecords);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetAll_WhenSecondPageIsRequested_ReturnsCorrectItems()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
        }

        // Act
        var response = await client.GetAsync("/api/companies?page=2&records=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(2, result.TotalRecords);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Page);
    }

    [Fact]
    public async Task GetAll_WhenNameFilterIsProvided_ReturnsMatchingCompanies()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
        }

        // Act
        var response = await client.GetAsync("/api/companies?name=Microsoft");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Microsoft", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAll_WhenFilterHasNoMatches_ReturnsEmptyList()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        }

        // Act
        var response = await client.GetAsync("/api/companies?name=Xbox");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetAll_WhenFiltering_ReturnsOnlyCurrentUserCompanies()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
        }

        // Act
        var response = await client.GetAsync("/api/companies");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.DoesNotContain(result.Items, c => c.Id == 3);
    }

    [Fact]
    public async Task Get_WhenCompanyExists_ReturnsCompany()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
        }

        var companyId = 1;

        // Act
        var response = await client.GetAsync($"/api/companies/{companyId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CompanyResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("Microsoft", result.Name);
    }

    [Fact]
    public async Task Get_WhenCompanyDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();

        var companyId = 99;

        // Act
        var response = await client.GetAsync($"api/companies/{companyId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenCompanyBelongsToAnotherUser_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
        }
        client.DefaultRequestHeaders.Add("X-Test-UserId", "test-user-id2");

        var companyId = 1;

        // Act
        var response = await client.GetAsync($"/api/companies/{companyId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenDataIsValid_CreatesCompany()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        

        var company = new CompanyCreateDto
        {
            Name = "Nintendo",
            Description = "Good Game Company",
            Website = "http://www.nintendo.com",
            Location = "Remote"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/companies", company);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        var companyDb = await db.Companies.SingleOrDefaultAsync(c => c.Name == "Nintendo");
        Assert.NotNull(companyDb);
        Assert.Equal("Nintendo", companyDb.Name);
        Assert.Equal("test-user-id", companyDb.UserId);
    }

    [Fact]
    public async Task Create_WhenWebsiteIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        

        var company = new CompanyCreateDto
        {
            Name = "Nintendo",
            Description = "Good Game Company",
            Website = "www.nintendo.com",
            Location = "Remote"
        };


        // Act
        var response =  await client.PostAsJsonAsync("/api/companies", company);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Contains("Website", problem.Errors.Keys);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        var companiesCount = await db.Companies.CountAsync();
        Assert.Equal(0, companiesCount);
    }

    [Fact]
    public async Task Create_WhenUserDoesNotHasAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Unauthenticated", "false");

        var company = new CompanyCreateDto
        {
            Name = "Nintendo",
            Description = "Good Game Company",
            Website = "http://www.nintendo.com",
            Location = "Remote"
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/companies", company);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var companies = await db.Companies.AsNoTracking().ToListAsync();
            Assert.Empty(companies);
        }
    }

    [Fact]
    public async Task Update_WhenDataIsValid_ReturnsNoContent()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
        }

        var companyId = 1;
        var companyData = new CompanyUpdateDto
        {
            Name = "Microsoft Colombia",
            Description = "Good company in Colombia"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/companies/{companyId}", companyData);
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scopeResponse = factory.Services.CreateScope();
        var dbResponse = scopeResponse.ServiceProvider.GetRequiredService<ApiDbContext>();
        var companyInDb = await dbResponse.Companies.SingleOrDefaultAsync(c => c.Id == companyId);
        Assert.NotNull(companyInDb);
        Assert.Equal(companyData.Name, companyInDb.Name);
    }

    [Fact]
    public async Task Update_WhenCompanyDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();

        var companyId = 1;
        var companyData = new CompanyUpdateDto
        {
            Name = "Microsoft Colombia",
            Description = "Good company in Colombia"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/companies/{companyId}", companyData);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WhenCompanyBelongsToAnotherUser_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
        }

        client.DefaultRequestHeaders.Add("X-Test-UserId", "test-user-id2");

        var companyId = 1;
        var companyData = new CompanyUpdateDto
        {
            Name = "Microsoft Colombia",
            Description = "Good company in Colombia"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/companies/{companyId}", companyData);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using (var verifyScope = factory.Services.CreateScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var company = await db.Companies.AsNoTracking().SingleAsync(c => c.Id == companyId);
            Assert.Equal("Microsoft", company.Name);
            Assert.Equal("Big Company", company.Description);
            Assert.Equal("test-user-id", company.UserId);
        }
    }

    [Fact]
    public async Task Delete_WhenDataIsValid_ReturnsNoContent()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
        }

        var companyId = 1;

        // Act
        var response = await client.DeleteAsync($"/api/companies/{companyId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var companies = await db.Companies.AsNoTracking().ToListAsync();
            Assert.Equal(2, companies.Count);
            Assert.DoesNotContain(companies, c => c.Id == 1);
            Assert.Contains(companies, c => c.Id == 2);
            Assert.Contains(companies, c => c.Id == 3);
        }
    }

    [Fact]
    public async Task Delete_WhenCompanyDoesNotExit_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();

        var companyId = 1;

        // Act
        var response = await client.DeleteAsync($"/api/companies/{companyId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenCompanyBelongsToAnotherUser_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        await SeedDataAsync(db);

        client.DefaultRequestHeaders.Add("X-Test-UserId", "test-user-id2");

        var companyId = 1;

        // Act
        var response = await client.DeleteAsync($"/api/companies/{companyId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var verifyScope = factory.Services.CreateScope())
        {
            var dbVerify = verifyScope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var companyCount = await dbVerify.Companies.CountAsync();
            var companyInDb = await dbVerify.Companies.SingleOrDefaultAsync(c => c.Id == companyId);
            Assert.NotNull(companyInDb);
            Assert.Equal(3, companyCount);
        }
    }
}