namespace JobTracker.Api.Dtos.CompanyDto;

public class CompanySearchDto
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? JobApplicationPosition { get; set; }
}