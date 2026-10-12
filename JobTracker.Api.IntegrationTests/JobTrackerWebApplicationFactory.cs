using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Hosting;
using JobTracker.Api.IntegrationTests.Authentication;
using Microsoft.AspNetCore.Authentication;


namespace JobTracker.Api.IntegrationTests;

public class JobTrackerWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApiDbContext>));
            services.RemoveAll<IDbContextOptionsConfiguration<ApiDbContext>>();

            services.AddSingleton<DbConnection>(_ =>
            {
                var connection = new SqliteConnection(
                    "Datasource=:memory:");
                connection.Open();
                return connection;
            });

            services.AddDbContext<ApiDbContext>((sp, options) =>
            {
                var connection = sp.GetRequiredService<DbConnection>();

                options.UseSqlite(connection);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = 
                    TestAuthHandler.Scheme;
                
                options.DefaultChallengeScheme =
                    TestAuthHandler.Scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.Scheme,
                options => {});
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApiDbContext>();   

        db.Database.EnsureCreated();

        return host;
    }
}
