using JobTracker.Api.Data;
using Microsoft.Data.Sqlite;

namespace JobTracker.Api.Tests.Helpers;

public sealed class SqliteTestContext : IAsyncDisposable
{
    public SqliteConnection Connection { get; }
    public ApiDbContext Context { get; }

    public SqliteTestContext(SqliteConnection connection, ApiDbContext context)
    {
        Connection = connection;
        Context = context;
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await Connection.DisposeAsync();
    }
}