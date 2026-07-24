using JobTracker.Api.Dtos.ApplicationNoteDto;
using JobTracker.Api.Models;

namespace JobTracker.Api.Dtos.JobApplicationDto;

public class JobApplicationResponseDto
{
    public int Id { get; set; }
    public required string Position { get; set; }
    public required string Status { get; set; }
    public DateTime AppliedAt { get; set; }
    public string? JobUrl { get; set; }
    public required string Company { get; set; }
    public int CompanyId { get; set; }
    public List<ApplicationNoteResponseDto>? Notes { get; set; }
}