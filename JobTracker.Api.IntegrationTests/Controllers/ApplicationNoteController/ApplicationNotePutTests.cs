using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos.ApplicationNoteDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JobTracker.Api.IntegrationTests.TestData;

namespace JobTracker.Api.IntegrationTests.Controllers.ApplicationNotes;

public class ApplicaionNoteControllerPutTests
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
            await ApplicationNoteTestData.SeedDataAsync(db);
        }

        var note = new ApplicationNoteUpdateDto
        {
            Content = "This is a great company!!!!",
        };
        var noteId = 1;

        // Act
        var response = await client.PutAsJsonAsync($"/api/application-notes/{noteId}", note);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var noteInDb = await dbData.Notes.SingleOrDefaultAsync(n => n.Id == noteId);
        Assert.NotNull(noteInDb);
        Assert.Equal(note.Content, noteInDb.Content);
    }

    [Fact]
    public async Task Update_WhenDataIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await ApplicationNoteTestData.SeedDataAsync(db);
        }

        var note = new ApplicationNoteUpdateDto
        {
    
        };
        var noteId = 1;

        // Act
        var response = await client.PutAsJsonAsync($"/api/application-notes/{noteId}", note);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var noteInDb = await dbData.Notes.SingleOrDefaultAsync(n => n.Id == noteId);
        Assert.NotNull(noteInDb);
        Assert.Equal("I like this company", noteInDb.Content);
    }

    [Fact]
    public async Task Update_WhenApplicationNoteDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await ApplicationNoteTestData.SeedDataAsync(db);
        }

        var note = new ApplicationNoteUpdateDto
        {
            Content = "This is a great company!!!!",
        };
        var noteId = 10;

        // Act
        var response = await client.PutAsJsonAsync($"/api/application-notes/{noteId}", note);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var noteInDb = await dbData.Notes.SingleOrDefaultAsync(n => n.Content == note.Content);
        Assert.Null(noteInDb);
    }

    [Fact]
    public async Task Update_WhenApplicationNoteBelongsToAnotherUser_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await ApplicationNoteTestData.SeedDataAsync(db);
        }

        var note = new ApplicationNoteUpdateDto
        {
            Content = "This is a great company!!!!",
        };
        var noteId = 5;

        // Act
        var response = await client.PutAsJsonAsync($"/api/application-notes/{noteId}", note);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var noteInDb = await dbData.Notes.SingleOrDefaultAsync(n => n.Id ==  noteId);
        Assert.NotNull(noteInDb);
        Assert.Equal("Developer interview", noteInDb.Content);
        Assert.Equal("test-user-id2", noteInDb.UserId);
    }
}