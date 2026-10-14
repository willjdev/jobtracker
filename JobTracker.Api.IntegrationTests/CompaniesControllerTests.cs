


using System.Net;

namespace JobTracker.Api.IntegrationTests.Controllers;

public class CompaniesControllerTests : IClassFixture<JobTrackerWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CompaniesControllerTests(JobTrackerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_WhenUserIsAuthenticated_ReturnsOk()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/companies");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}