
using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos.ApplicationNoteDto;
using JobTracker.Api.Models;
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


}