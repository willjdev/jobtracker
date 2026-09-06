using JobTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

public abstract class ServiceTestBase
{
    protected ApiDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ApiDbContext(options);
    }
}