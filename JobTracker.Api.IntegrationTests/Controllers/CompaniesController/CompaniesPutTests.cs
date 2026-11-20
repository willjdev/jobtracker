using System.Net;
using System.Net.Http.Json;
using JobTracker.Api.Data;
using JobTracker.Api.IntegrationTests.TestData;
using Microsoft.Extensions.DependencyInjection;
using JobTracker.Api.Dtos.CompanyDto;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.IntegrationTests.Controllers.Companies;

public class CompaniesControllerPutTests
{
    [Fact]
    public async Task Update_WhenDataIsValid_ReturnsNoContent()
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
        var companyData = new CompanyUpdateDto
        {
            Name = "Microsoft Colombia",
            Description = "Good company in Colombia"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/companies/{companyId}", companyData);
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scopeResponse = factory.Services.CreateScope();
        var dbResponse = scopeResponse.ServiceProvider.GetRequiredService<ApiDbContext>();
        var companyInDb = await dbResponse.Companies.SingleOrDefaultAsync(c => c.Id == companyId);
        Assert.NotNull(companyInDb);
        Assert.Equal(companyData.Name, companyInDb.Name);
    }

    [Fact]
    public async Task Update_WhenCompanyDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();

        var companyId = 1;
        var companyData = new CompanyUpdateDto
        {
            Name = "Microsoft Colombia",
            Description = "Good company in Colombia"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/companies/{companyId}", companyData);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WhenCompanyBelongsToAnotherUser_ReturnsNotFound()
    {
        // Arrange
        using var factory = new JobTrackerWebApplicationFactory();
        using var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            await CompanyTestData.SeedDataAsync(db);
        }

        client.DefaultRequestHeaders.Add("X-Test-UserId", "test-user-id2");

        var companyId = 1;
        var companyData = new CompanyUpdateDto
        {
            Name = "Microsoft Colombia",
            Description = "Good company in Colombia"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/companies/{companyId}", companyData);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        using (var verifyScope = factory.Services.CreateScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var company = await db.Companies.AsNoTracking().SingleAsync(c => c.Id == companyId);
            Assert.Equal("Microsoft", company.Name);
            Assert.Equal("Big Company", company.Description);
            Assert.Equal("test-user-id", company.UserId);
        }
    }
}