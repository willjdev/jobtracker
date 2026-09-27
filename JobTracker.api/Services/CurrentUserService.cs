using System.Security.Claims;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccesor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccesor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccesor.HttpContext?
            .User
            .FindFirstValue(ClaimTypes.NameIdentifier);
}