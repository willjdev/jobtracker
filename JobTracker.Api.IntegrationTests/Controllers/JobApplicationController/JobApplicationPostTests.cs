using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos.JobApplicationDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JobTracker.Api.IntegrationTests.TestData;

namespace JobTracker.Api.IntegrationTests.Controllers.JobApplications;

public class JobApplicationControllerPostTests
{
    [Fact]
    public async Task Create_WhenDataIsValid_CreatesJobApplication()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scopeDb = factory.Services.CreateScope())
        {
            var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
            await JobApplicationTestData.SeedDataAsync(dbData);
        }
        
        var application = new JobApplicationCreateDto
        {
            Position = "Game Programmer",
            JobUrl = "https://www.job.com",
            CompanyId = 2,
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/applications", application);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        var applicationInDb = await db.Applications.SingleOrDefaultAsync(j => j.Position == "Game Programmer");
        Assert.NotNull(applicationInDb);
        var result = await response.Content.ReadFromJsonAsync<JobApplicationResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("Santa Monica", result.Company);
        Assert.Equal("test-user-id", applicationInDb.UserId);
        Assert.Equal(2, applicationInDb.CompanyId);
        Assert.Equal(2, result.CompanyId);
    }

    [Fact]
    public async Task Create_WhenCompanyBelongsToAnotherUser_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await JobApplicationTestData.SeedDataAsync(db);
        }

        var application = new JobApplicationCreateDto
        {
            Position = "Game Programmer",
            JobUrl = "https://www.job.com",
            CompanyId = 3,
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/applications", application);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var applicationInDb = await dbData.Applications.SingleOrDefaultAsync(j => j.Position == application.Position);
        var applicationInDbCount = await dbData.Applications.CountAsync();
        Assert.Null(applicationInDb);
        Assert.Equal(3, applicationInDbCount);
    }

    [Fact]
    public async Task Create_WhenDataIsNotValid_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await JobApplicationTestData.SeedDataAsync(db);
        }

        var application = new JobApplicationCreateDto
        {
            Position = "Game Programmer",
            JobUrl = "ht//www.job.com",
            CompanyId = 1,
        };

        // Act
        var response =  await client.PostAsJsonAsync("/api/applications", application);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var applicationInDb = await dbData.Applications.SingleOrDefaultAsync(j => j.Position == application.Position);
        Assert.Null(applicationInDb);
    }
}