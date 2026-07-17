namespace JobTracker.Api.Dtos.JobApplicationDto;


public class JobApplicationSearchDto
{
    public string? Position { get; set; }
    public string? Status { get; set; }
    public DateTime? AppliedAt { get; set; }
    public int? CompanyId { get; set; }
}