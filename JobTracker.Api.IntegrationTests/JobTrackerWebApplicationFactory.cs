using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Data;
using Microsoft.Data.Sqlite;

namespace JobTracker.Api.IntegrationTests;

public class JobTrackerWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
            services.RemoveAll(typeof(DbContextOptions<ApiDbContext>)));
    }
    
    
    
    protected async Task<ApiDbContext> CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseSqlite(connection)
            .Options;
        
        var context = new ApiDbContext(options);

        await context.Database.EnsureCreatedAsync();

        return context;
    }

    protected async Task<SqliteConnection> CreateOpenConnectionAsync()
    {
        var connection = new SqliteConnection("Datasourc=:memory:");
        await connection.OpenAsync();

        return connection;
    }
}
