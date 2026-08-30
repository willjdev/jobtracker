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
        var companies = new List<Company>
        {
            new()
            {
                Id = 1,
                Name = "Microsoft",
                Description = "Big Company",
                Website = "www.microsoft.com",
                Location = "Holand",
                CreatedAt = new DateTime(2026, 7, 10, 6, 10, 0),
            },
            new()
            {
                Id = 2,
                Name = "Santa Monica",
                Description = "Game Company",
                Website = "www.santamonica.com",
                Location = "Remote",
                CreatedAt = new DateTime(2026, 7, 21, 0, 0, 0),
            },
            new()
            {
                Id = 4,
                Name = "Microsoft Netherlands",
                Description = "Big Company",
                Website = "www.microsoft.com",
                Location = "Netherlands",
                CreatedAt = new DateTime(2026, 8, 4, 8, 20, 0)
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
                CompanyId = 1,
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
                Status = "Meeting",
                AppliedAt = new DateTime(2026, 8, 20, 6, 40, 0),
                JobUrl = "https//www.job.com",
                CompanyId = 1,
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
            },
            new()
            {
                Id = 4,
                Position = ".NET Developer",
                Status = "Interview",
                AppliedAt = new DateTime(2026, 8, 10, 6, 40, 0),
                JobUrl = "https//www.job.com",
                CompanyId = 4,
                ApplicationNotes = new List<ApplicationNote>
                {
                    new()
                    {
                        Id = 7,
                        Content = "Sent an email to recruiter",
                        CreatedAt = new DateTime(2026, 8, 10, 9, 0, 0),
                        JobApplicationId = 4
                    },
                    new()
                    {
                        Id = 8,
                        Content = "Meeting tomorrow!!",
                        CreatedAt = new DateTime(2026, 8, 14, 10, 0, 0),
                        JobApplicationId = 4
                    }
                }
            }
        };

        await context.Companies.AddRangeAsync(companies);
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
        Assert.Equal(4, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(4, result.TotalRecords);
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
    [InlineData(99, 0, 0, null, null)]
    [InlineData(1, 2, 2, "Fullstack Developer", "Microsoft")]
    public async Task GetAllAsync_WhenFilteringByCompanyId_ReturnsMatchingJobApplications(
        int companyId, 
        int expectedCount, 
        int expectedTotalRecords,
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
        Assert.Equal(expectedCount, result.Items.Count);
        Assert.Equal(expectedTotalRecords, result.TotalRecords);

        if (expectedCount > 0)
        {
            Assert.Equal(expectedPosition, result.Items[0].Position);
            Assert.All(result.Items, item =>
            {
                Assert.Equal(companyId, item.CompanyId);
                Assert.Equal(company, item.Company);
            });
        }
        else
        {
            Assert.Empty(result.Items);
        }
    }

    [Theory]
    [InlineData(".NET Developer", ".NET Developer", 2, 1, 1)]
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
        Assert.Equal(expectedTotalPages, result.TotalPages);

        if (expectedCount > 0)
        {
            Assert.Equal(expectedCompanyId, result.Items[0].CompanyId);
            Assert.All(result.Items, item =>
            {
                Assert.Equal(expectedPosition, item.Position);
            });
        }
        else
        {
            Assert.Empty(result.Items);
        }
    }

    [Theory]
    [InlineData("Applied", 2, 2, 1)]
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
        

        if (expectedCount > 0)
        {
            var item = result.Items[0];

            Assert.Equal(expectedFirstItemCompanyId, item.CompanyId);
            Assert.All(result.Items, item =>
            {
                Assert.Equal(status, item.Status);
            });
        }
        else
        {
            Assert.Empty(result.Items);
        }
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

    [Theory]
    [InlineData("position", "asc", new[] {3, 4, 1, 2})]
    [InlineData("position", "desc", new[] {2, 1, 3, 4})]
    [InlineData(null, null, new[] {1, 2, 3, 4})]
    public async Task GetAllAsync_WhenSortingByFieldName_ReturnsSortedItems(
        string? fieldName,
        string? sortByType,
        int[] expectedOrder
        )
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var searchDto = new JobApplicationSearchDto{ FieldName = fieldName, SortByType = sortByType };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        var resultOrder = result.Items
            .Select(item => item.Id)
            .ToArray();

        Assert.Equal(expectedOrder, resultOrder);
    }  

    [Theory]
    [InlineData("status", "asc", new[] {1, 2, 4, 3})] 
    [InlineData("status", "desc", new[] {3, 4, 1, 2})]
    public async Task GetAllAsync_WhenSortingByStatus_ReturnsSortedItems(
        string fieldName,
        string sortByType,
        int[] expectedOrder
        )
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var searchDto = new JobApplicationSearchDto{ FieldName = fieldName, SortByType = sortByType };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        var resultOrder = result.Items
            .Select(item => item.Id)
            .ToArray();
        
        Assert.Equal(expectedOrder, resultOrder);
    }

    [Theory]
    [InlineData("appliedat", "asc", new[] {4, 1, 2, 3})]
    [InlineData("appliedat", "desc", new[] {3, 2, 1, 4})]
    public async Task GetAllAsync_WhenSortingByAppliedAt_ReturnsSortedItems(
        string fieldName,
        string sortByType,
        int[] expectedOrder
        )
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var searchDto = new JobApplicationSearchDto{ FieldName = fieldName, SortByType = sortByType };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        var resultOrder = result.Items
            .Select(item => item.Id)
            .ToArray();
        
        Assert.Equal(expectedOrder, resultOrder);
    }

    [Theory]
    [InlineData("companyid", "asc", new[] {1, 3, 2, 4})]
    [InlineData("companyid", "desc", new[] {4, 2, 1, 3})]
    public async Task GetAllAsync_WhenSortingByCompanyId_ReturnSortedItems(
        string fieldName,
        string sortByType,
        int[] expectedOrder
        )
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var searchDto = new JobApplicationSearchDto{ FieldName = fieldName, SortByType = sortByType };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        var resultOrder = result.Items
            .Select(item => item.Id)
            .ToArray();

        Assert.Equal(expectedOrder, resultOrder); 
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(0, 4)]
    [InlineData(70, 50)]
    public async Task GetAllAsync_WhenRecordsIsOutsideAllowedRange_ClampsRecordsToValidLimits(
        int inputRecords,
        int expectedRecords
        )
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var searchDto = new JobApplicationSearchDto{ Records = inputRecords };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.Equal(expectedRecords, result.Records);
    }

    [Fact]
    public async Task GetAllAsync_WhenPageIsLessThanOne_ClampsToOne()
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var searchDto = new JobApplicationSearchDto{ Page = 0 };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task GetAllAsync_WhenPageIsTwo_ReturnPageTwoItems()
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var searchDto = new JobApplicationSearchDto{ Page = 2, Records = 2 };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(".NET Developer", result.Items[0].Position);
        Assert.Equal(4, result.Items[1].CompanyId);
    }

    [Theory]
    [InlineData(2, "Game Developer", 2, 1)]
    [InlineData(99, null, null, 0)]
    public async Task GetByIdAsync_GivenId_ReturnsExpectedItem(
        int inputId,
        string? expectedPosition,
        int? expectedCompanyId,
        int expectedItemsCount
        )
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        
        // Act
        var result = await service.GetByIdAsync(inputId);

        // Assert
        if (expectedItemsCount > 0)
        {
            Assert.Equal(expectedPosition, result?.Position);
            Assert.Equal(expectedCompanyId, result?.CompanyId);
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Fact]
    public async Task GetByIdAsync_WhenJobApplicationHasNotes_ReturnsItemWithNotes()
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var jobId = 1;

        // Act
        var result = await service.GetByIdAsync(jobId);

        // Assert
        Assert.NotNull(result);
        var notes = result.Notes;
        Assert.NotNull(notes);
        Assert.Equal(2, notes.Count);
        Assert.All(notes, item =>
        {
            Assert.NotNull(item);
        });
    }

    [Fact]
    public async Task CreateAsync_WhenCompanyExists_ReturnsCreatedJobApplicationResponseDto()
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var jobDto = new JobApplicationCreateDto
        {
            Position = "Senior Game Developer",
            JobUrl = "https://www.job.com",
            CompanyId = 1
        };

        // Act
        var result = await service.CreateAsync(jobDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(jobDto.Position, result?.Position);
        Assert.Equal(jobDto.JobUrl, result?.JobUrl);
        Assert.Equal(jobDto.CompanyId, result?.CompanyId);
        
        var createdJob = await context.Applications.FirstOrDefaultAsync(j => j.Position == jobDto.Position);
        Assert.NotNull(createdJob);
        Assert.Equal(jobDto.CompanyId, createdJob.CompanyId);
    }

    [Fact]
    public async Task CreateAsync_WhenCompanyDoesNotExists_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var jobDto = new JobApplicationCreateDto
        {
            Position = "Senior Game Developer",
            JobUrl = "https://www.job.com",
            CompanyId = 99
        };

        // Act
        var result = await service.CreateAsync(jobDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenJobApplicationExists_UpdatesAndReturnsTrue()
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        int jobId = 1;
        var jobUpdate = new JobApplicationUpdateDto{ Position = "Senior Fullstack Developer" };

        // Act
        var result = await service.UpdateAsync(jobId, jobUpdate);

        // Assert
        var updatedJobApplication = await context.Applications.FindAsync(jobId);
        Assert.True(result);
        Assert.NotNull(updatedJobApplication);
        Assert.Equal(jobUpdate.Position, updatedJobApplication.Position);
    }

    [Fact]
    public async Task UpdateAsync_WhenJobApplicationDoesNotExists_ReturnsFalse()
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var jobId = 99;
        var jobUpdate = new JobApplicationUpdateDto{ Position = "Senior Game Developer" };

        // Act
        var result = await service.UpdateAsync(jobId, jobUpdate);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenJobApplicationExists_DeletesAndReturnsTrue()
    {
        // Arrange 
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var jobId = 1;

        // Act
        var result = await service.DeleteAsync(jobId);

        // Assert
        var deletedJobApplication = await context.Applications.FindAsync(jobId);
        Assert.True(result);
        Assert.Null(deletedJobApplication);
    }

    [Fact]
    public async Task DeleteAsync_WhenJobApplicationDoesNotExists_ReturnsFalse()
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);

        var service = new JobApplicationService(context);
        var jobId = 99;

        // Act
        var result = await service.DeleteAsync(jobId);

        // Assert
        Assert.False(result);
    }
}