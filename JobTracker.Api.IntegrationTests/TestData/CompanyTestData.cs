using JobTracker.Api.Data;
using JobTracker.Api.Models;

namespace JobTracker.Api.IntegrationTests.TestData;

internal static class CompanyTestData
{
public static async Task SeedDataAsync(ApiDbContext context)
    {
        var userId = "test-user-id";
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
                UserId = "test-user-id2",
                CreatedAt = new DateTime(2026, 8, 28, 8, 0, 0),
            }
        };
        
        await context.Companies.AddRangeAsync(companies);
        await context.SaveChangesAsync();
    }
}