namespace JobTracker.Api.Models;

public class ApplicationNote
{
    public int Id { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int JobApplicationId { get; set; }
    public JobApplication? JobApplication { get; set; }
}