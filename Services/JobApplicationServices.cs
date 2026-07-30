using JobTracker.Api.Models;
using JobTracker.Api.Dtos.Common;
using JobTracker.Api.Dtos.JobApplicationDto;
using JobTracker.Api.Services.Interfaces;
using JobTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Dtos.ApplicationNoteDto;

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

        if (search.CompanyId != null)
            query = query.Where(j => j.CompanyId == search.CompanyId);

        if (!string.IsNullOrWhiteSpace(search.Position))
            query = query.Where(j => j.Position.Contains(search.Position));
        
        if (search.AppliedAt != null)
        {
            var startDate = search.AppliedAt.Value.Date;
            var endDate = startDate.AddDays(1);

            query = query.Where(j => j.AppliedAt >= startDate && j.AppliedAt < endDate);
        }

        if (!string.IsNullOrWhiteSpace(search.Status))
            query = query.Where(j => j.Status == search.Status);
        
        query = search.FieldName?.ToLower() switch
        {
            "position" => search.SortByType?.ToLower() == "desc" ? query.OrderByDescending(c => c.Position) : query.OrderBy(c => c.Position),
            "status" => search.SortByType?.ToLower() == "desc" ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
            "appliedat" => search.SortByType?.ToLower() == "desc" ? query.OrderByDescending(c => c.AppliedAt) : query.OrderBy(c => c.AppliedAt),
            "companyid" => search.SortByType?.ToLower() == "desc" ? query.OrderByDescending(c => c.CompanyId) : query.OrderBy(c => c.CompanyId),
            _ => query.OrderBy(c => c.Id)
        };

        var totalRecords = await query.CountAsync();

        if (search.Page < 1)
            search.Page = 1;
        if (search.Records < 1)
            search.Records = 4;
        if (search.Records > 50)
            search.Records = 50;
        
        var jobList = await query
            .Skip((search.Page - 1 ) * search.Records)
            .Take(search.Records)
            .Select(j => new JobApplicationResponseDto
            {
                Id = j.Id,
                Position = j.Position,
                Status = j.Status,
                AppliedAt = j.AppliedAt,
                JobUrl = j.JobUrl,
                Company = j.Company!.Name,
                CompanyId = j.CompanyId
            })
            .ToListAsync();
        
        var response = new PagedResponse<JobApplicationResponseDto>
        {
            Items = jobList,
            Page = search.Page,
            Records = search.Records,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)search.Records)
        };

        return response;
    }

    public async Task<JobApplicationResponseDto?> GetByIdAsync(int id)
    {
        var job = await _context.Applications
            .Include(j => j.Company)
            .Include(ja => ja.ApplicationNotes)
            .FirstOrDefaultAsync(jb => jb.Id == id);
        if (job is null)
            return null;
        
        return new JobApplicationResponseDto
        {
            Id = job.Id,
            Position = job.Position,
            Status = job.Status,
            AppliedAt = job.AppliedAt,
            JobUrl = job.JobUrl,
            Company = job.Company?.Name ?? "Sin empresa",
            CompanyId = job.CompanyId,
            Notes = [.. (job.ApplicationNotes ?? [])
                .OrderByDescending(n => n.CreatedAt)
                .Select(j => new ApplicationNoteResponseDto
                {
                    Id = j.Id,
                    Content = j.Content,
                    CreatedAt = j.CreatedAt
                })]
        };
    }

    public async Task<JobApplicationResponseDto?> CreateAsync(JobApplicationCreateDto job)
    {
        var company = await _context.Companies.FindAsync(job.CompanyId);
        if (company is null)
            return default;
        
        var newJob = new JobApplication
        {
            Position = job.Position,
            JobUrl = job.JobUrl,
            CompanyId = job.CompanyId,
            Company = company
        };
        await _context.Applications.AddAsync(newJob);
        await _context.SaveChangesAsync();
        var jobResponse = new JobApplicationResponseDto
        {
            Id = newJob.Id,
            Position = newJob.Position,
            Status = newJob.Status,
            AppliedAt = newJob.AppliedAt,
            JobUrl = newJob.JobUrl,
            Company = newJob.Company.Name,
            CompanyId = newJob.CompanyId
        };

        return jobResponse;
    }

    public async Task<bool> UpdateAsync(int id, JobApplicationUpdateDto job)
    {
        var jobDb = await _context.Applications.FindAsync(id);
        if (jobDb is null)
            return false;

        jobDb.Position = job.Position;

        if (job.Status != null)
            jobDb.Status = job.Status;

        if (job.JobUrl != null)
            jobDb.JobUrl = job.JobUrl;

        await _context.SaveChangesAsync();

        return true; 
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var job = await _context.Applications.FindAsync(id);

        if (job is null)
            return false;
        
        _context.Applications.Remove(job);
        await _context.SaveChangesAsync();

        return true;
    }
}