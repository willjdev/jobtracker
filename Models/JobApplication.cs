namespace JobTracker.api.Models;

public class JobApplication
{
    public int Id { get; set; }
    public required string Position { get; set; }
    public string Status { get; set; } = "Applied";
    public DateTime AppliedAt { get; set; } = DateTime.Now;
    public string? JobUrl { get; set; }
    public int CompanyId { get; set; }
    public Company? Company { get; set; }

}