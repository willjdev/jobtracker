using Xunit;
using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Services;
using JobTracker.Api.Data;
using JobTracker.Api.Models;
using JobTracker.Api.Dtos.CompanyDto;

namespace JobTracker.Api.Tests.Services;

public class CompanyServiceTests
{
    // Helper
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
                JobApplications = new List<JobApplication>
                {
                    new()
                    {
                        Id = 1,
                        Position = ".NET Developer",
                        JobUrl = "www.job.com",
                        CompanyId = 1
                    },
                    new()
                    {
                        Id = 2,
                        Position = "Senior .NET Developer",
                        JobUrl = "www.job.com",
                        CompanyId = 1
                    }

                }
            },
            new()
            {
                Id = 2,
                Name = "Santa Monica",
                Description = "Game Company",
                Website = "www.santamonica.com",
                Location = "Remote",
                CreatedAt = new DateTime(2026, 7, 20, 7, 0, 0),
                JobApplications = new List<JobApplication>
                {
                    new()
                    {
                        Id = 3,
                        Position = "Game Developer",
                        JobUrl = "www.job.com",
                        CompanyId = 2
                    },
                    new()
                    {
                        Id = 4,
                        Position = "Senior Game Developer",
                        JobUrl = "www.job.com",
                        CompanyId = 2
                    }

                }
            },
            new()
            {
                Id = 3,
                Name = "Microsoft Colombia",
                Description = "Big Company",
                Website = "www.microsoft.com",
                Location = "Colombia",
                CreatedAt = new DateTime(2026, 8, 2, 8, 20, 0),
                JobApplications = new List<JobApplication>
                {
                    new()
                    {
                        Id = 5,
                        Position = "Junior .NET Developer",
                        JobUrl = "www.job.com",
                        CompanyId = 3
                    },
                    new()
                    {
                        Id = 6,
                        Position = "Senior Azure + .NET Developer",
                        JobUrl = "www.job.com",
                        CompanyId = 3
                    }

                }
            }
        };

        await context.Companies.AddRangeAsync(companies);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_WhenCompaniesExist_ReturnPagedResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ApiDbContext(options);

        await SeedDatabaseAsync(context);
        
        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto();

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Result
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalRecords);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(3, result.Items.Count);
        Assert.All(result.Items, item => Assert.NotNull(item));
        Assert.Equal("Santa Monica", result.Items[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_WhenRecordsSetToOne_ReturnsTwoPages()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ApiDbContext(options);

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{ Name = "Micro", Records = 1 };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.Records);
        Assert.Equal(2, result.TotalRecords);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetAllAsync_WhenPagesetToTwo_ReturnsPageTwoResults()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ApiDbContext(options);

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{ Records = 1, Page = 2 };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(2, result.Page);
    }

    [Fact]
    public async Task GetAllAsync_WhenNameDoesNotExist_ReturnsEmptyPagedResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ApiDbContext(options);

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{ Name = "Naughty Dog" };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalPages);
        Assert.Equal(0, result.TotalRecords);
    }

    [Fact]
    public async Task GetAllAsync_WhenRecordsIsEighty_ReturnsRecordsIsFifty()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ApiDbContext(options);

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{ Records = 80 };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.Records);
    }

    [Fact]
    public async Task GetAllAsync_WhenFilterByName_ReturnsMatchingCompanies()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ApiDbContext(options);

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto { Name = "Santa Monica" };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Santa Monica", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAllAsync_WhenSortedByNameDesc_ReturnsCompaniesInDescendingOrder()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ApiDbContext(options);

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var searchDto =  new CompanySearchDto{ Name = "Micro", FieldName = "Name", SortByType = "desc" };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Microsoft Colombia", result.Items[0].Name);
        Assert.Equal("Microsoft", result.Items[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_WhenFilterByLocation_ReturnsMatchingCompanies()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ApiDbContext(options);

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{ Location = "Remote" };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Santa Monica", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAllAsync_WhenFilterByCreatedAt_ReturnsMatchingCompanies()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApiDbContext(options);

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{ CreatedAt = new DateTime(2026, 8, 2, 8, 20, 0) };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Microsoft Colombia", result.Items[0].Name);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenCompanyExists_ReturnCompany()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ApiDbContext(options);

        context.Add(new Company
        {
            Id = 1,
            Name = "Microsoft",
            Description = "Big Company",
            Website = "www.microsoft.com",
            Location = "Holand",
        });
        await context.SaveChangesAsync();

        var service =  new CompanyService(context);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Microsoft", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCompanyDoesNotExist_ReturnsNull()
    {
        // Arrange

        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ApiDbContext(options);

        var service = new CompanyService(context);

        // Act
        var result = await service.GetByIdAsync(99);

        // Assert
        Assert.Null(result);
    }
}