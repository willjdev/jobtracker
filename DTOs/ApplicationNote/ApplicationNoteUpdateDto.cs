using System.ComponentModel.DataAnnotations;

namespace JobTracker.api.Dtos.ApplicationNote;

public class ApplicationNoteUpdateDto
{
    [Required(ErrorMessage = "Note content is required")]
    public string? Content { get; set; }
}