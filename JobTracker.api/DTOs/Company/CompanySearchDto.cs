namespace JobTracker.Api.Dtos.CompanyDto;

public class CompanySearchDto
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? JobApplicationPosition { get; set; }
    public string? FieldName { get; set; }
    public string? SortByType { get; set; }
    public int Page { get; set;} = 1;
    public int Records { get; set; } = 4;
}