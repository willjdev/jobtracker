using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos.ApplicationNoteDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JobTracker.Api.IntegrationTests.TestData;

namespace JobTracker.Api.IntegrationTests.Controllers.ApplicationNotes;

public class ApplicationNoteControllerPostTests
{
    [Fact]
    public async Task Create_WhenDataIsValid_ReturnsCreated()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await ApplicationNoteTestData.SeedDataAsync(db);
        }

        var note = new ApplicationNoteCreateDto
        {
            Content = "This is a great company!!!!",
            JobApplicationId = 2
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/application-notes", note);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var noteInDb = await dbData.Notes.SingleOrDefaultAsync(n => n.Content == "This is a great company!!!!");
        Assert.NotNull(noteInDb);
        Assert.Equal("test-user-id", noteInDb.UserId);
        Assert.Equal(2, noteInDb.JobApplicationId);
        Assert.Equal(note.Content, noteInDb.Content);
    }

    [Fact]
    public async Task Create_WhenDataIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await ApplicationNoteTestData.SeedDataAsync(db);
        }

        var note = new ApplicationNoteCreateDto
        {
            Content = "This is a great company!!!!",
            JobApplicationId = 8
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/application-notes/", note);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var noteInDb = await dbData.Notes.SingleOrDefaultAsync(n => n.Content ==  note.Content);
        Assert.Null(noteInDb);
    }

    [Fact]
    public async Task Create_WhenJobApplicationBelongsToAnotherUser_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await ApplicationNoteTestData.SeedDataAsync(db);
        }

        var note = new ApplicationNoteCreateDto
        {
            Content = "This is a great company!!!!",
            JobApplicationId = 3
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/application-notes", note);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var noteInDb = await dbData.Notes.SingleOrDefaultAsync(n => n.Content == note.Content);
        Assert.Null(noteInDb);
    }

    [Fact]
    public async Task Create_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add("X-Test-Unauthenticated", "false");

        var note = new ApplicationNoteCreateDto
        {
            Content = "This is a great company!!!!",
            JobApplicationId = 2
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/application-notes", note);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var scopeDb = factory.Services.CreateScope();
        var dbData = scopeDb.ServiceProvider.GetRequiredService<ApiDbContext>();
        var noteInDb = await dbData.Notes.SingleOrDefaultAsync(n => n.Content == note.Content);
        Assert.Null(noteInDb);
    }
}