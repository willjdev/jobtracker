using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Data;
using Microsoft.Extensions.DependencyInjection;
using JobTracker.Api.Dtos.CompanyDto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.IntegrationTests.Controllers.Companies;

public class CompaniesControllerPostTests
{
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
}