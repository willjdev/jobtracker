using Xunit;
using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Services;
using JobTracker.Api.Data;
using JobTracker.Api.Models;
using JobTracker.Api.Dtos.CompanyDto;

namespace JobTracker.Api.Tests.Services;

public class CompanyServiceTests
{
    [Fact]
    public async Task GetAllAsync_WhenCompaniesExist_ReturnPagedResponse()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        using var context = new ApiDbContext(options);

        var companies = new List<Company>
        {
            new() {
                Id = 1,
                Name = "Microsoft",
                Description = "Big Company",
                Website = "www.microsoft.com",
                Location = "Holand",
            },
            new()
            {
                Id = 2,
                Name = "Santa Monica",
                Description = "Game Company",
                Website = "www.santamonica.com",
                Location = "Remote",
            }
        };
        await context.AddRangeAsync(companies);
        await context.SaveChangesAsync();
        
        var service = new CompanyService(context);
        var searchDto = new CompanySearchDto();

        // Act
        var result = await service.GetAllAsync(searchDto);

        // Result
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalRecords);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.NotNull(item));
        Assert.Equal("Santa Monica", result.Items[1].Name);
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
    public async Task GetTaskAsync_WhenCompanyDoesNotExist_ReturnsNull()
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