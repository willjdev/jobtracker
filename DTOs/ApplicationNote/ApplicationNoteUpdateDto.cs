using System.ComponentModel.DataAnnotations;

namespace JobTracker.Api.Dtos.ApplicationNoteDto;

public class ApplicationNoteUpdateDto
{
    [Required(ErrorMessage = "Note content is required")]
    public string? Content { get; set; }
}