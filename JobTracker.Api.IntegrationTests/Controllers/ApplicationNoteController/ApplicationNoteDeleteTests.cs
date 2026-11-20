using System.Net;
using JobTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JobTracker.Api.IntegrationTests.TestData;

namespace JobTracker.Api.IntegrationTests.Controllers.ApplicationNotes;

public class ApplicationNoteControllerDeleteTests
{
    [Fact]
    public async Task Delete_WhenDataIsValid_ReturnsNoContent()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await ApplicationNoteTestData.SeedDataAsync(db);
        }

        var noteId = 1;

        // Act
        var response = await client.DeleteAsync($"/api/application-notes/{noteId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var noteInDb = await dbData.Notes.SingleOrDefaultAsync(n => n.Id == noteId);
        var notesCount = await dbData.Notes.CountAsync();
        Assert.Null(noteInDb);
        Assert.Equal(4, notesCount);
    }

    [Fact]
    public async Task Delete_WhenDataIsInvalid_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await ApplicationNoteTestData.SeedDataAsync(db);
        }

        var noteId = 9;

        // Act
        var response = await client.DeleteAsync($"/api/application-notes/{noteId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var notesCount = await dbData.Notes.CountAsync();
        Assert.Equal(5, notesCount);
    }

    [Fact]
    public async Task Delete_WhenApplicationNoteBelongsToAnotherUser()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await ApplicationNoteTestData.SeedDataAsync(db);
        }

        var noteId = 5;

        // Act
        var response = await client.DeleteAsync($"/api/application-notes/{noteId}");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var noteInDb = await dbData.Notes.SingleOrDefaultAsync(n => n.Id == noteId);
        var notesCount = await dbData.Notes.CountAsync();
        Assert.NotNull(noteInDb);
        Assert.Equal("Developer interview", noteInDb.Content);
        Assert.Equal(5, notesCount);
    }
}