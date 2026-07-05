using System.ComponentModel.DataAnnotations;

namespace JobTracker.api.Dtos.CompanyDto;

public class CompanyUpdateDto
{
    [Required(ErrorMessage = "Company name is required")]
    [StringLength(100, ErrorMessage = "The name most be less than 100 characters")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "The description is required")]
    public required string Description { get; set; }

    [Url(ErrorMessage = "Website format is invalid")]
    public string? Website { get; set; }

    public string? Location { get; set; }
}

