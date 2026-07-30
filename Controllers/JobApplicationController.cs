using Microsoft.AspNetCore.Mvc;
using JobTracker.Api.Dtos.JobApplicationDto;
using JobTracker.Api.Dtos.Common;
using JobTracker.Api.Services.Interfaces;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/applications")]
public class JobApplicationsController : ControllerBase
{
    private readonly ILogger<JobApplicationsController> _logger;
    private readonly IJobApplicationService _services;
    public JobApplicationsController(ILogger<JobApplicationsController> logger, IJobApplicationService services)
    {
        _logger = logger;
        _services = services;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<JobApplicationResponseDto>>> GetJobApplications([FromQuery] JobApplicationSearchDto search)
    {
        try
        {
            PagedResponse<JobApplicationResponseDto> response = await _services.GetAllAsync(search);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting search");
            return StatusCode(500, new { message = "An error occurred while getting job applications"});            
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobApplicationResponseDto>> Get(int id)
    {
        try
        {
            var response = await _services.GetByIdAsync(id);
            if (response is null)
                return NotFound();
            else
                return response;
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
            var response = await _services.CreateAsync(job);
            if (response is null)
                return BadRequest();
            else
                return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
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
            return await _services.UpdateAsync(id, job) ? NoContent() : NotFound();
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
            return await _services.DeleteAsync(id) ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job application");
            return StatusCode(500, new { message = "An error occurred while deleting job application"});
        }
    }    
}

