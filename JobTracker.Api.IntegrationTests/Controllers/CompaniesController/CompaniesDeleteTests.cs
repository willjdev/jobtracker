using System.Net;
using JobTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JobTracker.Api.IntegrationTests.TestData;

namespace JobTracker.Api.IntegrationTests.Controllers;

public class CompaniesControllerDeleteTests
{
    [Fact]
    public async Task Delete_WhenDataIsValid_ReturnsNoContent()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await CompanyTestData.SeedDataAsync(db);
        }

        var companyId = 1;

        // Act
        var response = await client.DeleteAsync($"/api/companies/{companyId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var companies = await db.Companies.AsNoTracking().ToListAsync();
            Assert.Equal(2, companies.Count);
            Assert.DoesNotContain(companies, c => c.Id == 1);
            Assert.Contains(companies, c => c.Id == 2);
            Assert.Contains(companies, c => c.Id == 3);
        }
    }

    [Fact]
    public async Task Delete_WhenCompanyDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();

        var companyId = 1;

        // Act
        var response = await client.DeleteAsync($"/api/companies/{companyId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenCompanyBelongsToAnotherUser_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        await CompanyTestData.SeedDataAsync(db);

        client.DefaultRequestHeaders.Add("X-Test-UserId", "test-user-id2");

        var companyId = 1;

        // Act
        var response = await client.DeleteAsync($"/api/companies/{companyId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using (var verifyScope = factory.Services.CreateScope())
        {
            var dbVerify = verifyScope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var companyCount = await dbVerify.Companies.CountAsync();
            var companyInDb = await dbVerify.Companies.SingleOrDefaultAsync(c => c.Id == companyId);
            Assert.NotNull(companyInDb);
            Assert.Equal(3, companyCount);
        }
    }
}