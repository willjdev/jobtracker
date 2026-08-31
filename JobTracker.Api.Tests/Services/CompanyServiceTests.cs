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
    private ApiDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        return new ApiDbContext(options);
    }

    private async Task<ApiDbContext> CreateSeededContextAsync()
    {
        var context = CreateContext();
        await SeedDatabaseAsync(context);
        return context;
    }
    [Fact]
    public async Task GetAllAsync_WhenCompaniesExist_ReturnPagedResponse()
    {
        // Arrange
        using var context = await CreateSeededContextAsync();
        
        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{};

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Result
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalRecords);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(3, result.Items.Count);
        Assert.All(result.Items, Assert.NotNull);
        Assert.Equal("Santa Monica", result.Items[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_WhenDbIsEmpty_ReturnsEmptyPagedResponse()
    {
        // Arrange
        using var context = CreateContext();

        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{};

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalPages);
        Assert.Equal(0, result.TotalRecords);
    }

    [Theory]
    [InlineData("Santa Monica", "Santa Monica")]
    [InlineData("Naughty Dog", null)]
    public async Task GetAllAsync_WhenFilteringByName_ReturnsExpectedResults(
        string name,
        string? expectedName
        )
    {
        // Arrange
        using var context = await CreateSeededContextAsync();

        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{ Name= name };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.NotNull(result);

        if (expectedName is not null)
        {
            var company = Assert.Single(result.Items);

            Assert.Equal(expectedName, company.Name);
            Assert.Equal(1, result.TotalRecords);
        }
        else
        {
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalRecords);
            Assert.Equal(0, result.TotalPages);
        }
    }

    [Theory]
    [InlineData("Remote", "Santa Monica")]
    [InlineData("Japan", null)]
    public async Task GetAllAsync_WhenFilteringByLocation_ReturnsExpectedResults(
        string location,
        string? expectedCompanyName
        )
    {
        // Arrange
        using var context = await CreateSeededContextAsync();

        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{ Location = location };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.NotNull(result);

        if (expectedCompanyName is not null)
        {
            var item = Assert.Single(result.Items);
            Assert.Equal(expectedCompanyName, item.Name);
            Assert.Equal(1, result.TotalRecords);
        }
        else
        {
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalRecords);
        }
    }

    [Fact]
    public async Task GetAllAsync_WhenFilterByCreatedAt_ReturnsMatchingCompanies()
    {
        // Arrange
        using var context = CreateContext();

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
    public async Task GetAllAsync_WhenFilterByJobApplication_ReturnsMatchingCompanies()
    {
        // Arrange
        using var context = CreateContext();

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{ JobApplicationPosition = ".NET" };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.StartsWith("M", item.Name));
    }

    [Fact]
    public async Task GetAllAsync_WhenFieldNameIsNull_ReturnsSortedById()
    {
        // Arrange
        using var context = CreateContext();

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{ FieldName = null };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Items[0].Id);
        Assert.Equal(2, result.Items[1].Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenRecordsSetToOne_ReturnsTwoPages()
    {
        // Arrange
        using var context = CreateContext();

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
        using var context = CreateContext();

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

    

    [Theory]
    [InlineData(-10, 4)]
    [InlineData(80, 50)]
    public async Task GetAllAsync_WhenRecordsIsOutsideAllowedRange_ClampsRecordsToValidLimits(int inputRecords, int expectedRecords)
    {
        // Arrange
        using var context = CreateContext();
        await SeedDatabaseAsync(context);
        
        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{ Records = inputRecords };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRecords, result.Records);
    }

    [Fact]
    public async Task GetAllAsync_WhenPageIsLessThanOne_ReturnsPageIsOne()
    {
        // Arrange
        using var context = CreateContext();

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto{ Page = -5 };

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
    }

    

    [Fact]
    public async Task GetAllAsync_WhenSortedByNameDesc_ReturnsCompaniesInDescendingOrder()
    {
        // Arrange
        using var context = CreateContext();

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
    public async Task GetByIdAsync_WhenCompanyExists_ReturnCompany()
    {
        // Arrange
        using var context = CreateContext();

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
        using var context = CreateContext();

        var service = new CompanyService(context);

        // Act
        var result = await service.GetByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WhenCompanyCreateDtoIsValid_ReturnsCompanyResponseDto()
    {
        // Arrange
        using var context = CreateContext();
    
        var service = new CompanyService(context);
        var createDto = new CompanyCreateDto
        {
            Name = "Naughty Dog",
            Description = "Creators of The Last Of Us",
            Website = "https://wwww.naughtydog.com",
            Location = "Remote"
        };

        // Act
        var result = await service.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(createDto.Name, result.Name);
        Assert.Equal(createDto.Description, result.Description);

        var companyInDb = await context.Companies.FindAsync(result.Id);
        Assert.NotNull(companyInDb);
    }

    [Fact]
    public async Task UpdateAsync_WhenUpdateSuccess_ReturnsTrue()
    {
        // Arrange
        using var context = CreateContext();

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var testId = 1;
        var updateDto = new CompanyUpdateDto
        {
            Name = "Microsoft Xbox",
            Description = "Big gaming company around the World"
        };

        // Act
        var result = await service.UpdateAsync(testId, updateDto);

        // Assert
        var companyInDb = await context.Companies.FindAsync(testId);
        Assert.True(result);
        Assert.NotNull(companyInDb);
        Assert.Equal(updateDto.Name, companyInDb.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenCompanyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        using var context = CreateContext();

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var testId = 99;
        var updateDto = new CompanyUpdateDto
        {
            Name = "Microsoft Xbox",
            Description = "Big gaming company around the World"
        };

        // Act
        var result = await service.UpdateAsync(testId, updateDto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenCompanyExists_RemovesCompanyAndReturnsTrue()
    {
        // Arrange
        using var context = CreateContext();

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var testId = 1;

        // Act
        var result = await service.DeleteAsync(testId);

        // Assert
        var companyInDb = await context.Companies.FindAsync(testId);
        Assert.True(result);
        Assert.Null(companyInDb);
    }

    [Fact]
    public async Task DeleteAsync_WhenCompanyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        using var context = CreateContext();

        await SeedDatabaseAsync(context);

        var service = new CompanyService(context);
        var testId = 99;

        // Act
        var result = await service.DeleteAsync(testId);

        // Assert
        Assert.False(result);
    }
}