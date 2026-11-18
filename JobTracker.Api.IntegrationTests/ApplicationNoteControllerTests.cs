
using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos.ApplicationNoteDto;
using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.Api.IntegrationTests.Controllers;

public class ApplicationNoteControllerTests
{
    private async Task SeedDataAsync(ApiDbContext context)
    {
        var userId = "test-user-id";
        var userId2 = "test-user-id2";

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
            },
            new()
            {
                Id = 3,
                Name = "Naughty Dog",
                Description = "Game Company",
                Website = "www.naughtydog.com",
                Location = "Remote",
                UserId = userId2,
                CreatedAt = new DateTime(2026, 8, 28, 8, 0, 0),
            }
        };

        var jobApplications = new List<JobApplication>
        {
            new()
            {
                Id = 1,
                Position = "Fullstack Developer",
                JobUrl = "https://www.job.com",
                AppliedAt = new DateTime(2026, 8, 17, 6, 20, 0),
                UserId = userId,
                CompanyId = 1,
            },
            new()
            {
                Id = 2,
                Position = "Game Developer",
                AppliedAt = new DateTime(2026, 8, 18, 0, 0, 0),
                JobUrl = "https://www.job.com",
                UserId = userId,
                CompanyId = 2
            },
            new()
            {
                Id = 3,
                Position = "Senior Game Developer",
                Status = "Meeting",
                AppliedAt = new DateTime(2026, 8, 20, 6, 40, 0),
                JobUrl = "https://www.job.com",
                UserId = userId2,
                CompanyId = 3
            }
        };

        var notes = new List<ApplicationNote>
        {
            new()
            {
                Id = 1,
                Content = "I like this company",
                CreatedAt = new DateTime(2026, 7, 10, 6, 20, 0),
                UserId = userId,
                JobApplicationId = 1
            },
            new()
            {
                Id = 2,
                Content = "Sent the email, let's wait for response",
                CreatedAt = new DateTime(2026, 8, 2, 10, 40, 0),
                UserId = userId,
                JobApplicationId = 1
            },
            new()
            {
                Id = 3,
                Content = "Game company, sent resume",
                CreatedAt = new DateTime(2026, 8, 14, 7, 40, 0),
                UserId = userId,
                JobApplicationId = 2
            },
            new()
            {
                Id = 4,
                Content = "Sent the email to the recruiter",
                CreatedAt = new DateTime(2026, 8, 18, 9, 20 ,0),
                UserId = userId,
                JobApplicationId = 2
            },
            new()
            {
                Id = 5,
                Content = "Developer interview",
                CreatedAt = new DateTime(2026, 9, 22, 9, 20 ,0),
                UserId = userId2,
                JobApplicationId = 3
            }
        };

        await context.Companies.AddRangeAsync(companies);
        await context.Applications.AddRangeAsync(jobApplications);
        await context.Notes.AddRangeAsync(notes);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_WhenNotesExist_ReturnList()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
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
            await SeedDataAsync(db);
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
    public async Task Get_WhenApplicationNoteBelongsToAnoterUser_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
        }

        var noteId = 5;

        // Act
        var respone = await client.GetAsync($"/api/application-notes/{noteId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, respone.StatusCode);
    }

    [Fact]
    public async Task Create_WhenDataIsValid_ReturnsCreated()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
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
            await SeedDataAsync(db);
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
            await SeedDataAsync(db);
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

    [Fact]
    public async Task Update_WhenDataIsValid_ReturnsNoContent()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
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
            await SeedDataAsync(db);
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
    public async Task Update_WhenApplicationNoteBelongsToAnotherUser_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
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

    [Fact]
    public async Task Delete_WhenDataIsValid_ReturnsNoContent()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
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
            await SeedDataAsync(db);
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
            await SeedDataAsync(db);
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