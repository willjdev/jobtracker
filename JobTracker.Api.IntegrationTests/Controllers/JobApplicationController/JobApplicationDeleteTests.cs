using System.Net;
using JobTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JobTracker.Api.IntegrationTests.TestData;

namespace JobTracker.Api.IntegrationTests.Controllers.JobApplications;

public class JobApplicationControllerDeleteTests
{
    [Fact]
    public async Task Delete_WhenDataIsValid_ReturnNoContent()
    {
        // Arrange 
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await JobApplicationTestData.SeedDataAsync(db);
        }

        var applicationId = 1;

        // Act
        var response = await client.DeleteAsync($"/api/applications/{applicationId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var applicationInDb = await dbData.Applications.SingleOrDefaultAsync(j => j.Id == applicationId);
        Assert.Null(applicationInDb);
    }

    [Fact]
    public async Task Delete_WhenApplicationBelongsToAnotherUser_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await JobApplicationTestData.SeedDataAsync(db);
        }

        var applicationId = 3;

        // Act
        var response = await client.DeleteAsync($"/api/applications/{applicationId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var applicationInDb = await dbData.Applications.SingleOrDefaultAsync(j => j.Id == applicationId);
        Assert.NotNull(applicationInDb);
        Assert.Equal("Game Developer", applicationInDb.Position);
    }
}