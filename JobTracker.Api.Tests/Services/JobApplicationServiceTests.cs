using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Services;
using JobTracker.Api.Data;
using JobTracker.Api.Models;
using JobTracker.Api.Dtos.JobApplicationDto;

namespace JobTracker.Api.Tests.Services;

public class JobApplicationServiceTests
{
    private async Task SeedDatabaseAsync(ApiDbContext context)
    {
        var jobApplications = new List<JobApplication>
        {
            new()
            {
                Id = 1,
                Position = "Fullstack Developer",
                JobUrl = "https://www.job.com",
                AppliedAt = new DateTime(2026, 8, 17, 6, 20, 0),
                CompanyId = 1,
                Company = new Company
                {
                    Id = 1,
                    Name = "Microsoft",
                    Description = "Big Company",
                    Website = "www.microsoft.com",
                    Location = "Holand",
                    CreatedAt = new DateTime(2026, 7, 10, 6, 10, 0),
                },
                ApplicationNotes = new List<ApplicationNote>
                {
                    new()
                    {
                        Id = 1,
                        Content = "Waiting response",
                        CreatedAt = new DateTime(2026, 8, 17, 6, 30, 0),
                        JobApplicationId = 1
                    },
                    new()
                    {
                        Id = 2,
                        Content = "Resume sent",
                        CreatedAt = new DateTime(2026, 8, 18, 7, 30, 0),
                        JobApplicationId = 1
                    }
                }
            },
            new()
            {
               Id = 2,
               Position = "Game Developer",
               AppliedAt = new DateTime(2026, 8, 18, 0, 0, 0),
               JobUrl = "https//www.job.com",
               CompanyId = 2,
               Company = new Company
               {
                    Id = 2,
                    Name = "Santa Monica",
                    Description = "Game Company",
                    Website = "www.santamonica.com",
                    Location = "Remote",
                    CreatedAt = new DateTime(2026, 7, 21, 0, 0, 0),
               },
               ApplicationNotes = new List<ApplicationNote>
               {
                   new()
                   {
                        Id = 3,
                        Content = "Sent application, waiting for response",
                        CreatedAt = new DateTime(2026, 8, 18, 8, 20, 0),
                        JobApplicationId = 2
                   },
                   new()
                   {
                       Id = 4,
                       Content = "Received email!!",
                       CreatedAt = new DateTime(2026, 8, 19, 8, 24, 0),
                       JobApplicationId = 2
                   }
               }
            },
            new()
            {
                Id = 3,
                Position = ".NET Developer",
                AppliedAt = new DateTime(2026, 8, 20, 6, 40, 0),
                JobUrl = "https//www.job.com",
                CompanyId = 3,
                Company = new Company
                {
                    Id = 3,
                    Name = "Microsoft Colombia",
                    Description = "Big Company",
                    Website = "www.microsoft.com",
                    Location = "Colombia",
                    CreatedAt = new DateTime(2026, 8, 2, 8, 20, 0)
                },
                ApplicationNotes = new List<ApplicationNote>
                {
                    new()
                    {
                        Id = 5,
                        Content = "Sent an email to recruiter",
                        CreatedAt = new DateTime(2026, 8, 20, 9, 0, 0),
                        JobApplicationId = 3
                    },
                    new()
                    {
                        Id = 6,
                        Content = "Recruiter wants to interview me!",
                        CreatedAt = new DateTime(2026, 8, 22, 10, 0, 0),
                        JobApplicationId = 3
                    }
                }
            }
        };

        await context.Applications.AddRangeAsync(jobApplications);
        await context.SaveChangesAsync();
    }
    private ApiDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ApiDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_WhenJobApplicationExist_ReturnsPagedResponse()
    {
        // Arrange 
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var searchDto = new JobApplicationSearchDto{};

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(3, result.TotalRecords);
        Assert.All(result.Items, Assert.NotNull);
    }    

    [Fact]
    public async Task GetAllAsync_WhenDbIsEmpty_ReturnsEmptyPagedResponse()
    {
        // Arrange
        using var context = CreateContext();

        var service = new JobApplicationService(context);
        var searchDto = new JobApplicationSearchDto{};

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalPages);
        Assert.Equal(0, result.TotalRecords);
    }

    [Theory]
    [InlineData(99, 0, null, null)]
    [InlineData(1, 1, "Fullstack Developer", "Microsoft")]
    public async Task GetAllAsync_WhenFilteringByCompanyId_ReturnsMatchingJobApplications(
        int companyId, 
        int expectedOutput, 
        string? expectedPosition,
        string? company)
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var searchDto = new JobApplicationSearchDto{ CompanyId = companyId };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.Equal(expectedOutput, result.Items.Count);
        Assert.Equal(expectedPosition, result.Items.FirstOrDefault()?.Position);
        Assert.Equal(company, result.Items.FirstOrDefault()?.Company);
        Assert.Equal(expectedOutput, result.TotalRecords);
    }

    [Theory]
    [InlineData(".NET Developer", ".NET Developer", 1, 1, 3)]
    [InlineData("Ruby Developer", null, 0, 0, null)]
    public async Task GetAllAsync_WhenFilteringByPosition_ReturnsMatchingJobApplications(
        string position,
        string? expectedPosition,
        int expectedCount,
        int expectedTotalPages,
        int? expectedCompanyId)
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var searchDto = new JobApplicationSearchDto{ Position = position };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.Equal(expectedCount, result.Items.Count);
        Assert.Equal(expectedPosition, result.Items.FirstOrDefault()?.Position);
        Assert.Equal(expectedTotalPages, result.TotalPages);
        Assert.Equal(expectedCompanyId, result.Items.FirstOrDefault()?.CompanyId);
    }

    [Theory]
    [InlineData("Applied", 3, 3, 1)]
    [InlineData("Accepted", 0, 0, null)]
    public async Task GetAllAsync_WhenFilteringByStatus_ReturnsMatchingJobApplications(
        string status,
        int expectedCount,
        int expectedTotalRecords,
        int? expectedFirstItemCompanyId)
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service =  new JobApplicationService(context);
        var searchDto = new JobApplicationSearchDto{ Status = status };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.Equal(expectedCount, result.Items.Count);
        Assert.Equal(expectedTotalRecords, result.TotalRecords);
        Assert.Equal(expectedFirstItemCompanyId, result.Items.FirstOrDefault()?.CompanyId);
    }

    [Theory]
    [InlineData("2026-08-18T00:00:00", 1, "Game Developer", 2, 1)]
    [InlineData("2026-11-18T00:00:00", 0, null, null, 0)]
    public async Task GetAllAsync_WhenFilteringByAppliedAt_ReturnsMatchingJobApplications(
        DateTime date,
        int expectedItemsCount,
        string? expectedPosition,
        int? expectedCompanyId,
        int expectedTotalRecords)
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var searchDto = new JobApplicationSearchDto{ AppliedAt = date };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.Equal(expectedItemsCount, result.Items.Count);
        Assert.Equal(expectedTotalRecords, result.TotalRecords);

        if (expectedItemsCount > 0)
        {
            var item = Assert.Single(result.Items);

            Assert.Equal(expectedPosition, item.Position);
            Assert.Equal(expectedCompanyId, item.CompanyId);
        }
        else
        {
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalPages);
        }
    }
}