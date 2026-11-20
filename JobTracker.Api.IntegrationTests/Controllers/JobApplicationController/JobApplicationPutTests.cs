using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos.JobApplicationDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JobTracker.Api.IntegrationTests.TestData;

namespace JobTracker.Api.IntegrationTests.Controllers.JobApplications;

public class JobApplicationControllerPutTests
{
    [Fact]
    public async Task Update_WhenDataIsValid_ReturnsNoContent()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await JobApplicationTestData.SeedDataAsync(db);
        }

        var application = new JobApplicationUpdateDto
        {
            Position = "Game Programmer",
            Status = "Meeting",
            JobUrl = "https://www.job.com"
        };
        var applicationId = 2;

        // Act
        var response = await client.PutAsJsonAsync($"/api/applications/{applicationId}", application);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var applicationInDb = await dbData.Applications.SingleOrDefaultAsync(j => j.Id == applicationId);
        Assert.NotNull(applicationInDb);
        Assert.Equal(application.Position, applicationInDb.Position);
    }

    [Fact]
    public async Task Update_WhenApplicationBelongsToAnotherUser_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await JobApplicationTestData.SeedDataAsync(db);
        }

        var application = new JobApplicationUpdateDto
        {
            Position = "Game Programmer",
            Status = "Meeting",
            JobUrl = "https://www.job.com"
        };
        var applicationId = 3;

        // Act
        var response = await client.PutAsJsonAsync($"/api/applications/{applicationId}", application);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var applicationInDb = await dbData.Applications.SingleOrDefaultAsync(j => j.Id == applicationId);
        Assert.NotNull(applicationInDb);
        Assert.Equal("Game Developer", applicationInDb.Position);
    }
}