using System.ComponentModel.DataAnnotations;

namespace JobTracker.api.Dtos.JobApplication;

public class JobApplicationCreateDto
{
    [Required(ErrorMessage ="Job position is required")]
    [StringLength(80, ErrorMessage = "Job position length must be less than 80 characters")]
    public required string Position { get; set; }

    [Url(ErrorMessage = "Url format is invalid")]
    public string? JobUrl { get; set; }
    
    //public int CompanyId { get; set; } De pronto puede obtenerse de una lista
    
}