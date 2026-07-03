using System.ComponentModel.DataAnnotations;

namespace JobTracker.api.Models;

public class Company
{
    public int Id { get; set; }
    [Required(ErrorMessage = "The company name is required")]
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<JobApplication> JobApplications { get; set; } = new();

}