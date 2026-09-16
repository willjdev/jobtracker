using System.ComponentModel.DataAnnotations;

namespace JobTracker.Api.Dtos.AuthDto;

public class RegisterDto
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lastname is required")]
    [MinLength(1)]
    public string Lastname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
}