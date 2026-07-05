using System.ComponentModel.DataAnnotations;

namespace JobTracker.api.Dtos.CompanyDto;

public class CompanyResponseDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
}

