using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Dtos.Common;
using JobTracker.Api.Models;
using JobTracker.Api.Data;
using JobTracker.Api.IntegrationTests.TestData;
using Microsoft.Extensions.DependencyInjection;
using JobTracker.Api.Dtos.CompanyDto;

namespace JobTracker.Api.IntegrationTests.Controllers.Companies;

public class CompaniesControllerGetTests
{
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
            await CompanyTestData.SeedDataAsync(db);
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
            await CompanyTestData.SeedDataAsync(db);
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
            await CompanyTestData.SeedDataAsync(db);
        }

        // Act
        var response = await client.GetAsync("/api/companies?page=2&records=1");
        var responsePageItem1 = await client.GetAsync("/api/companies?page=1&records=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, responsePageItem1.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();
        var resultPageItem1 = await responsePageItem1.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>(); 
        Assert.NotNull(result);
        Assert.NotNull(resultPageItem1);
        Assert.Single(result.Items);
        Assert.Single(resultPageItem1.Items);
        Assert.Equal(2, result.TotalRecords);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Page);
        Assert.Equal("Santa Monica", result.Items[0].Name);
        Assert.Equal("Microsoft", resultPageItem1.Items[0].Name);
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
            await CompanyTestData.SeedDataAsync(db);
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
            await CompanyTestData.SeedDataAsync(db);
        }

        // Act
        var response = await client.GetAsync("/api/companies?name=Xbox");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalRecords);
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
            await CompanyTestData.SeedDataAsync(db);

            var companies = new List<Company>
            {
                new()
                {
                    Id = 4,
                    Name = "Microsoft Games",
                    Description = "Big Company",
                    Website = "www.microsoft.com",
                    Location = "Holand",
                    UserId = "test-user-id",
                    CreatedAt = new DateTime(2026, 7, 10, 6, 10, 0),
                },
                new()
                {
                    Id = 5,
                    Name = "Microsoft Colombia",
                    Description = "Game Company",
                    Website = "www.santamonica.com",
                    Location = "Remote",
                    UserId = "test-user-id2",
                    CreatedAt = new DateTime(2026, 7, 20, 7, 0, 0),
                }
            };

            await db.Companies.AddRangeAsync(companies);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync("/api/companies?name=Microsoft");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalRecords);
        Assert.Contains(result.Items, c => c.Name == "Microsoft");
        Assert.Contains(result.Items, c => c.Name == "Microsoft Games");
        Assert.DoesNotContain(result.Items, c => c.Id == 3 || c.Id == 5);
    }

    [Fact]
    public async Task GetAll_WhenPaginationValuesExceedLimits_UsesAllowedValues()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await CompanyTestData.SeedDataAsync(db);
        }

        // Act
        var response = await client.GetAsync("/api/companies?page=0&records=200");
        var responseRecordsMinimumValue = await client.GetAsync("/api/companies?records=0");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, responseRecordsMinimumValue.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();
        var resultRecordsMinimumValue = await responseRecordsMinimumValue.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();
        Assert.NotNull(result);
        Assert.NotNull(resultRecordsMinimumValue);
        Assert.Equal(1, result.Page);
        Assert.Equal(50, result.Records);
        Assert.Equal(4, resultRecordsMinimumValue.Records);
    }

    [Fact]
    public async Task GetAll_WhenSortingIsSpecified_ReturnsCorrectOrder()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await CompanyTestData.SeedDataAsync(db);
        }   

        // Act
        var response = await client.GetAsync("/api/companies?fieldname=name&sortbytype=desc");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();
        Assert.NotNull(result);
        var names = result.Items.Select(c => c.Name).ToList();
        Assert.Equal(new[] {"Santa Monica", "Microsoft"}, names);
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
            await CompanyTestData.SeedDataAsync(db);
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
            await CompanyTestData.SeedDataAsync(db);
        }
        client.DefaultRequestHeaders.Add("X-Test-UserId", "test-user-id2");

        var companyId = 1;

        // Act
        var response = await client.GetAsync($"/api/companies/{companyId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}