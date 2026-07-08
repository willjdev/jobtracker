using JobTracker.api.Models;

namespace JobTracker.api.Dtos.ApplicationNote;

public class ApplicationNoteResponseDto
{
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
}