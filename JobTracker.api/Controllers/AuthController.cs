using JobTracker.Api.Dtos.AuthDto;
using JobTracker.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthService _services;

    public AuthController(ILogger<AuthController> logger, IAuthService service)
    {
        _logger = logger;
        _services = service;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto userData)
    {
        try
        {
            var response = await _services.RegisterAsync(userData);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return StatusCode(500, new { message = "An error ocurred while registeirng user." });
        }
    }
}