using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JobTracker.Api.Data;
using JobTracker.Api.Dtos.AuthDto;
using JobTracker.Api.Models;
using JobTracker.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace JobTracker.Api.Services;

public class AuthService : IAuthService
{

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    private Task<AuthResponseDto> CreateAuthResponseAsync(ApplicationUser user, string message)
    {
        var expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationMinutes"]));
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        var tokenText = new JwtSecurityTokenHandler().WriteToken(token);

        return Task.FromResult(new AuthResponseDto
        {
            Success = true,
            Message = message,
            Token = tokenText,
            Expiration = expiration,
            UserId = user.Id,
            Email = user.Email
        });
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);

        if (existingUser is  not null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Email is already registered."
            };
        }

        var user = new ApplicationUser
        {
            Name = registerDto.Name,
            Lastname = registerDto.Lastname,
            Email = registerDto.Email,
            UserName = registerDto.Email
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = string.Join(" ", result.Errors.Select(e => e.Description))
            };
        }

        return await CreateAuthResponseAsync(user, "User registered succesfully");

    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (user is null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid Email or Password."
            };
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!isPasswordValid)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid Email or Password."
            };
        }

        return await CreateAuthResponseAsync(user, "Login succesful");
    }

}