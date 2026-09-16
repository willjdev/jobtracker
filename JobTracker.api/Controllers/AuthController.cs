using JobTracker.Api.Dtos.AuthDto;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    /* [HttpPost]
    public async Task<IActionResult> Register(RegisterDto userData)
    {
    
    } */
}