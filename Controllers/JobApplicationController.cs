using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobTracker.api.Models;
using JobTracker.api.Dtos.JobApplication;

namespace JobTracker.api.Controllers;

[ApiController]
[Route("[controller]")]
public class JobApplicationController : ControllerBase
{
    private readonly ApiDbContext _context;

    public JobApplicationController(ApiDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<JobApplicationResponseDto>>> GetAll()
    {
        try
        {
            List<JobApplicationResponseDto> jobsList = [];
            var jobs = await _context.Applications
                .Include(ja => ja.Company)
                .ToListAsync();

            foreach (JobApplication job in jobs)
            {
                var newJobResponse = new JobApplicationResponseDto
                {
                    Position = job.Position,
                    Status = job.Status,
                    AppliedAt = job.AppliedAt,
                    JobUrl = job.JobUrl,
                    Company = job.Company?.Name ?? "Sin empresa"
                };
                jobsList.Add(newJobResponse);
            }

            return jobsList;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, new { message = "Ocurrió un error al obtener las aplicaciones"});
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobApplicationResponseDto>> Get(int id)
    {
        try
        {
            var job = await _context.Applications.FindAsync(id);
            if (job is null)
                return NotFound();
            
            return new JobApplicationResponseDto
            {
                Position = job.Position,
                Status = job.Status,
                AppliedAt = job.AppliedAt,
                JobUrl = job.JobUrl,
                Company = job.Company?.Name ?? "Sin empresa"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(JobApplicationCreateDto job)
    {
        if (ModelState.IsValid)
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
                Console.WriteLine(ex.Message);
            }
        }

        return BadRequest();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, JobApplicationUpdateDto job)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var jobDb = await _context.Applications.FindAsync(id);
                if (jobDb is null || jobDb.Id != id)
                    return NotFound();
                
                jobDb.Position = job.Position;
                
                /* if (job.Status == null)
                {
                    jobDb.Status = jobDb.Status;
                }
                else
                {
                    jobDb.Status = job.Status;
                } */

                if (job.Status != null)
                    jobDb.Status = job.Status;

                if (job.JobUrl != null)
                    jobDb.JobUrl = job.JobUrl;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        return BadRequest();
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
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest();
        }

        return NoContent();
    }

    
}

