namespace JobTracker.Api.Dtos.JobApplicationDto;


public class JobApplicationSearchDto
{
    public string? Position { get; set; }
    public string? Status { get; set; }
    public DateTime? AppliedAt { get; set; }
    public int? CompanyId { get; set; }
    public string? FieldName { get; set; }
    public string? SortByType { get; set; }
    public int Page { get; set;} = 1;
    public int Records { get; set; } = 4;
}