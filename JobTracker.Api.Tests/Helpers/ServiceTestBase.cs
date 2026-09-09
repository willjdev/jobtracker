using JobTracker.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Tests.Helpers;

public abstract class ServiceTestBase
{
    protected async Task<ApiDbContext> CreateContextAsync(SqliteConnection connection)
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
        var connection = new SqliteConnection("Datasource=:memory:");
        await connection.OpenAsync();

        return connection;
    }

    protected async Task<SqliteTestContext> CreateSqliteTestContextAsync()
    {
        var connection = await CreateOpenConnectionAsync();
        var context = await CreateContextAsync(connection);

        return new SqliteTestContext(connection, context);
    }
}