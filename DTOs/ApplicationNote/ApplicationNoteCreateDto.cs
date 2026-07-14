using System.ComponentModel.DataAnnotations;

namespace JobTracker.Api.Dtos.ApplicationNoteDto;

public class ApplicationNoteCreateDto
{
    [Required(ErrorMessage = "Note content is required")]
    public string? Content { get; set; }
    public int JobApplicationId { get; set; }

}