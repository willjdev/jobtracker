using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace JobTracker.Api.Models;
public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(1)]
    public string Name { get; set; }

    [Required(ErrorMessage = "Lastname is required")]
    [MinLength(1)]
    public string Lastname { get; set; }

    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
}