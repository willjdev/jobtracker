
using JobTracker.Api.Data;
using JobTracker.Api.Models;

namespace JobTracker.Api.IntegrationTests.Controllers;

public class ApplicationNoteControllerTests
{
    private async Task SeedDataAsync(ApiDbContext context)
    {
        var userId = "test-user-id";
        var userId2 = "test-user-id2";

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
                JobUrl = "https//www.job.com",
                UserId = userId,
                CompanyId = 2
            },
            new()
            {
                Id = 3,
                Position = ".NET Developer",
                Status = "Meeting",
                AppliedAt = new DateTime(2026, 8, 20, 6, 40, 0),
                JobUrl = "https//www.job.com",
                UserId = userId2,
                CompanyId = 1
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
                Content = "Sent the email to the recruiter",
                CreatedAt = new DateTime(2026, 9, 22, 9, 20 ,0),
                UserId = userId2,
                JobApplicationId = 3
            }
        };

        await context.Applications.AddRangeAsync(jobApplications);
        await context.Notes.AddRangeAsync(notes);
        await context.SaveChangesAsync();
    }
}