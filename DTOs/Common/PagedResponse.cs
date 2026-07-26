namespace JobTracker.Api.Dtos.Common;

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int Records { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}