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
            var jobs = await _context.Applications.ToListAsync();

            foreach (JobApplication job in jobs)
            {
                jobsList.Add(new JobApplicationResponseDto
                {
                    Position = job.Position,
                    Status = job.Status,
                    AppliedAt = job.AppliedAt,
                    JobUrl = job.JobUrl,
                    Company = job.Company.Name
                });
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
                Company = job.Company.Name
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return NotFound();
        }
    }


    
}

