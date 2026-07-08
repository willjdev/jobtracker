using System.ComponentModel.DataAnnotations;

namespace JobTracker.api.Dtos.ApplicationNote;

public class ApplicatioNoteUpdateDto
{
    [Required(ErrorMessage = "Note content is required")]
    public string? Content { get; set; }
    public int JobApplicationId { get; set; }

}