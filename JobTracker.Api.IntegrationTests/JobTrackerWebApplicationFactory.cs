using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;


namespace JobTracker.Api.IntegrationTests;

public class JobTrackerWebApplicationFactory : WebApplicationFactory<Program>
{
    private DbConnection? _dbConnection;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApiDbContext>));

            _dbConnection = new SqliteConnection("Datasource=:memory:");
            _dbConnection.Open();

            services.AddDbContext<ApiDbContext>(options =>
            {
                options.UseSqlite(_dbConnection);
            });
            var dbContext = CreateDbContext(services);
            dbContext.Database.EnsureCreated();
            
        });
    }

    private static ApiDbContext CreateDbContext(IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        return db;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _dbConnection?.Dispose();
    }
}
