using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos.Common;
using JobTracker.Api.Dtos.JobApplicationDto;
using JobTracker.Api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.Api.IntegrationTests.Controllers;

public class JobApplicationControllerTests
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
                Position = ".NET Developer",
                JobUrl = "https://www.job.com",
                AppliedAt = new DateTime(2026, 8, 17, 6, 20, 0),
                CompanyId = 1,
                UserId = userId,
            },
            new()
            {
               Id = 2,
               Position = "Game Tester",
               AppliedAt = new DateTime(2026, 8, 18, 0, 0, 0),
               JobUrl = "https//www.job.com",
               CompanyId = 2,
               UserId = userId,
            },
            new()
            {
                Id = 3,
                Position = "Game Developer",
                Status = "Meeting",
                AppliedAt = new DateTime(2026, 8, 20, 6, 40, 0),
                JobUrl = "https//www.job.com",
                CompanyId = 3,
                UserId = userId2,
            },
        };

        await context.Companies.AddRangeAsync(companies);
        await context.Applications.AddRangeAsync(jobApplications);
        await context.SaveChangesAsync();

    }

    [Fact]
    public async Task GetJobApplication_WhenUserIsAuthenticated_ReturnsOk()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/applications");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetJobApplication_WhenJobApplicationsExist_ReturnsJobApplications()
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
        var response = await client.GetAsync("/api/applications");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<JobApplicationResponseDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, j => j.Position == ".NET Developer");
        Assert.Contains(result.Items, j => j.Position == "Game Tester");
        Assert.DoesNotContain(result.Items, j => j.Position == "Game Developer");
    }

    [Fact]
    public async Task GetJobApplication_WhenNoJobApplicationExist_ReturnsEmptyList()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/applications");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<JobApplicationResponseDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetJobApplication_WhenUserDoesNotHasAuthorization_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Unauthenticated", "false");

        // Act
        var response = await client.GetAsync("/api/applications");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetJobApplication_WhenPageIsSpecified_ReturnsLimitedItems()
    {
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);
            var applications = new List<JobApplication>
            {
                new()
                {
                    Id = 4,
                    Position = "Senior Game Developer",
                    JobUrl = "https://www.job.com",
                    AppliedAt = new DateTime(2026, 8, 17, 6, 20, 0),
                    CompanyId = 3,
                    UserId = "test-user-id2",
                },
                new()
                {
                    Id = 5,
                    Position = "Senior .NET Developer",
                    JobUrl = "https://www.job.com",
                    AppliedAt = new DateTime(2026, 8, 17, 6, 20, 0),
                    CompanyId = 1,
                    UserId = "test-user-id",
                },
                new()
                {
                    Id = 6,
                    Position = "Senior Game Tester",
                    JobUrl = "https://www.job.com",
                    AppliedAt = new DateTime(2026, 8, 17, 6, 20, 0),
                    CompanyId = 2,
                    UserId = "test-user-id",
                },
                new()
                {
                    Id = 7,
                    Position = "Junior .NET Developer",
                    JobUrl = "https://www.job.com",
                    AppliedAt = new DateTime(2026, 8, 17, 6, 20, 0),
                    CompanyId = 1,
                    UserId = "test-user-id",
                }
            };

            await db.Applications.AddRangeAsync(applications);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync("/api/applications?page=1&records=2");
        var responseSecondPage = await client.GetAsync("/api/applications?page=2&records=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<JobApplicationResponseDto>>();
        var resultSecondPage = await responseSecondPage.Content.ReadFromJsonAsync<PagedResponse<JobApplicationResponseDto>>();
        Assert.NotNull(result);
        Assert.NotNull(resultSecondPage);
        Assert.Equal(5, result.TotalRecords);
        Assert.Equal(5, resultSecondPage.TotalRecords);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(3, resultSecondPage.TotalPages);
        var applicationsFirstResponse = result.Items.Select(c => c.Position).ToList();
        var applicationsSecondResponse = resultSecondPage.Items.Select(c => c.Position).ToList();
        Assert.Equal(new[] {".NET Developer", "Game Tester"}, applicationsFirstResponse);
        Assert.Equal(new[] {"Senior .NET Developer", "Senior Game Tester"}, applicationsSecondResponse);   
    }

    [Fact]
    public async Task GetJobApplication_WhenFilteringByPositionWithDescendingOrder_ReturnsExpectedItems()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await SeedDataAsync(db);

            var job = new JobApplication
            {
                Id = 4,
                Position = "Game Programmer",
                Status = "Meeting",
                AppliedAt = new DateTime(2026, 8, 20, 6, 40, 0),
                JobUrl = "https//www.job.com",
                CompanyId = 2,
                UserId = "test-user-id"
            };

            await db.Applications.AddAsync(job);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync("/api/applications?position=Game");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<JobApplicationResponseDto>>();
        Assert.NotNull(result);
        var applications = result.Items.Select(j => j.Position).ToList();
        Assert.Equal(new[] {"Game Tester", "Game Programmer"}, applications);
        Assert.Equal(2, result.Items.Count);
    }

}