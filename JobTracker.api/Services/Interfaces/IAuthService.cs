

using JobTracker.Api.Dtos.AuthDto;

namespace JobTracker.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync (RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
}