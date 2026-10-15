using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos.Common;
using JobTracker.Api.Dtos.CompanyDto;
using JobTracker.Api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.Api.IntegrationTests.Controllers;

public class CompaniesControllerTests : IClassFixture<JobTrackerWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JobTrackerWebApplicationFactory _factory;
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

    public CompaniesControllerTests(JobTrackerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WhenUserIsAuthenticated_ReturnsOk()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/companies");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WhenCompaniesExist_ReturnsCompanies()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
        }

        //Act
        var response = await _client.GetAsync("api/companies");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result =  await response.Content.ReadFromJsonAsync<PagedResponse<CompanyResponseDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, c => c.Name == "Microsoft");
        Assert.Contains(result.Items, c => c.Name == "Santa Monica");
    }
}