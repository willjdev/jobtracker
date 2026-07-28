using JobTracker.Api.Models;
using JobTracker.Api.Dtos.Common;
using JobTracker.Api.Dtos.JobApplicationDto;
using JobTracker.Api.Services.Interfaces;
using JobTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Services;

public class JobApplicationServices : IJobApplicationService
{
    private readonly ApiDbContext _context;

    public JobApplicationServices (ApiDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<JobApplicationResponseDto>> GetAllAsync(JobApplicationSearchDto search)
    {
        IQueryable<JobApplication> query = _context.Applications.AsNoTracking().AsQueryable();
        
    }
}