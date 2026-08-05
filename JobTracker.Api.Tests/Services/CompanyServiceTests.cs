using Xunit;
using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Services;
using JobTracker.Api.Data;
using JobTracker.Api.Models;

namespace JobTracker.Api.Tests.Services;

public class CompanyServiceTests
{
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