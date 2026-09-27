namespace JobTracker.Api.Tests.Helpers;


public class FakeCurrentUserService : ICurrentUserService
{
    public string? UserId { get; set; }
}