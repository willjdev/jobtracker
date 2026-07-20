using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Models;
using JobTracker.Api.Dtos.JobApplicationDto;
using JobTracker.Api.Data;
using Microsoft.IdentityModel.Tokens;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/applications")]
public class JobApplicationsController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ILogger<JobApplicationsController> _logger;
    public JobApplicationsController(ApiDbContext context, ILogger<JobApplicationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<JobApplicationResponseDto>>> GetJobApplications([FromQuery] JobApplicationSearchDto search)
    {
        try
        {
            IQueryable<JobApplication> query = _context.Applications.AsQueryable();

            if (search.CompanyId != null)
                query = query.Where(j => j.CompanyId == search.CompanyId);
            if (!string.IsNullOrWhiteSpace(search.Position))
                query = query.Where(j => j.Position.Contains(search.Position));
            if (search.AppliedAt != null)
                query = query.Where(j => j.AppliedAt.Date == search.AppliedAt.Value.Date);
            if (!string.IsNullOrWhiteSpace(search.Status))
                query = query.Where(j => j.Status == search.Status);
            
            var jobList = await query.Select(j => new JobApplicationResponseDto
            {
                Id = j.Id,
                Position = j.Position,
                Status = j.Status,
                AppliedAt = j.AppliedAt,
                JobUrl = j.JobUrl,
                Company = j.Company!.Name 
            })
            .ToListAsync();

            return jobList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting search");
            return StatusCode(500, new { message = "An error occurred while getting job searched"});            
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobApplicationResponseDto>> Get(int id)
    {
        try
        {
            var job = await _context.Applications
                .Include(j => j.Company)
                .FirstOrDefaultAsync(ja => ja.Id == id);
            if (job is null)
                return NotFound();
            
            return new JobApplicationResponseDto
            {
                Id = job.Id,
                Position = job.Position,
                Status = job.Status,
                AppliedAt = job.AppliedAt,
                JobUrl = job.JobUrl,
                Company = job.Company?.Name ?? "Sin empresa"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job application");
            return StatusCode(500, new { message = "An error occurred while getting job application"});
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(JobApplicationCreateDto job)
    {
        try
        {
            var company = await _context.Companies.FindAsync(job.CompanyId);
            if (company is null)
                return BadRequest();

            var newJob = new JobApplication{ Position = job.Position, JobUrl = job.JobUrl, CompanyId = job.CompanyId , Company = company };
            await _context.Applications.AddAsync(newJob);
            await _context.SaveChangesAsync();
            var jobResponse = new JobApplicationResponseDto{ Position = newJob.Position, Status = newJob.Status, AppliedAt = newJob.AppliedAt, JobUrl = newJob.JobUrl, Company = newJob.Company.Name };
            return CreatedAtAction(nameof(Get), new { id = newJob.Id }, jobResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job application");
            return StatusCode(500, new { message = "An error occurred while creating job application"});
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, JobApplicationUpdateDto job)
    {
        try
        {
            var jobDb = await _context.Applications.FindAsync(id);
            if (jobDb is null || jobDb.Id != id)
                return NotFound();
            
            jobDb.Position = job.Position;

            if (job.Status != null)
                jobDb.Status = job.Status;

            if (job.JobUrl != null)
                jobDb.JobUrl = job.JobUrl;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job application");
            return StatusCode(500, new { message = "An error occurred while updating job application"});
        }

    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var job = await _context.Applications.FindAsync(id);

            if (job is null)
                return NotFound();
            
            _context.Applications.Remove(job);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job application");
            return StatusCode(500, new { message = "An error occurred while deleting job application"});
        }
    }

// *******************************************************************************
    
}

