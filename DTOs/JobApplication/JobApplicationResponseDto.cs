using JobTracker.api.Models;

namespace JobTracker.api.Dtos.JobApplication;

public class JobApplicationResponseDto
{
    public required string Position { get; set; }
    public required string Status { get; set; }
    public DateTime AppliedAt { get; set; }
    public string? JobUrl { get; set; }
    public required string Company { get; set; }
}