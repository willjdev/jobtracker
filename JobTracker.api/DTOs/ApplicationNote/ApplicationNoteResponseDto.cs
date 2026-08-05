namespace JobTracker.Api.Dtos.ApplicationNoteDto;

public class ApplicationNoteResponseDto
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
}