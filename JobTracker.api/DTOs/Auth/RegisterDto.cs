using System.ComponentModel.DataAnnotations;

namespace JobTracker.Api.Dtos.AuthDto;

public class RegisterDto
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(1)]
    public string Name { get; set; }

    [Required(ErrorMessage = "Lastname is required")]
    [MinLength(1)]
    public string Lastname { get; set; }
}