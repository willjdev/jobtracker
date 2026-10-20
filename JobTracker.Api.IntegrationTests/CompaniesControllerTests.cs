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
    public async Task Create_WhenDataIsValid_CreatesCompany()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        

        var company = new CompanyCreateDto
        {
            Name = "Naughty Dog",
            Description = "Good Game Company",
            Website = "http://www.naughtydog.com",
            Location = "Remote"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/companies", company);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        var companyDb = await db.Companies.SingleOrDefaultAsync(c => c.Name == "Naughty Dog");
        Assert.NotNull(companyDb);
        Assert.Equal("Naughty Dog", companyDb.Name);
        Assert.Equal("test-user-id", companyDb.UserId);
    }

    [Fact]
    public async Task Create_WheenWebsiteIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();

        var company = new CompanyCreateDto
        {
            Name = "Naughty Dog",
            Description = "Good Game Company",
            Website = "www.naughtydog.com",
            Location = "Remote"
        };

        // Act
        var response =  await client.PostAsJsonAsync("/api/companies", company);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var companiesCount = await db.Companies.CountAsync();
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.True(problem.Errors.ContainsKey("Website"));
        Assert.Equal(0, companiesCount);
    }
}