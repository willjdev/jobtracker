using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos.ApplicationNoteDto;
using Microsoft.Extensions.DependencyInjection;
using JobTracker.Api.IntegrationTests.TestData;

namespace JobTracker.Api.IntegrationTests.Controllers.ApplicationNotes;

public class ApplicationNoteControllerGetTests
{
    [Fact]
    public async Task GetAll_WhenNotesExist_ReturnList()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await ApplicationNoteTestData.SeedDataAsync(db);
        }

        // Act
        var response = await client.GetAsync("/api/application-notes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<ApplicationNoteResponseDto>>();
        Assert.NotNull(result);
        var notes = result.Select(n => n.Content).ToList();
        Assert.Equal(new[] 
        {
            "Sent the email to the recruiter",
            "Game company, sent resume",
            "Sent the email, let's wait for response",
            "I like this company",
        }, notes);
        Assert.DoesNotContain(result, n => n.Content == "Developer interview");
    }

    [Fact]
    public async Task GetAll_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add("X-Test-Unauthenticated", "false");

        // Act
        var response = await client.GetAsync("/api/application-notes");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenApplicationNoteExist_ReturnsApplicationNote()
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
        var response = await client.GetAsync($"/api/application-notes/{noteId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApplicationNoteResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("I like this company", result.Content);
    }

    [Fact]
    public async Task Get_WhenApplicationNoteDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        
        var noteId = 1;

        // Act
        var response = await client.GetAsync($"/api/application-notes/{noteId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenApplicationNoteBelongsToAnotherUser_ReturnsNotFound()
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
        var respone = await client.GetAsync($"/api/application-notes/{noteId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respone.StatusCode);
    }
}