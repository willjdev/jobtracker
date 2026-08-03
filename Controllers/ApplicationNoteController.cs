using Microsoft.AspNetCore.Mvc;
using JobTracker.Api.Dtos.ApplicationNoteDto;
using JobTracker.Api.Services.Interfaces;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/application-notes")]
public class ApplicationNotesController : ControllerBase
{
    private readonly ILogger<ApplicationNotesController> _logger;
    private readonly IApplicationNoteService _services;

    public ApplicationNotesController(ILogger<ApplicationNotesController> logger, IApplicationNoteService services)
    {
        _logger = logger;
        _services = services;
    }

    [HttpGet]
    public async Task<ActionResult<List<ApplicationNoteResponseDto>>> GetAll()
    {
        try
        {
            return await _services.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting application notes");
            return StatusCode(500, new { message = "An error occurred while getting notes"});
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationNoteResponseDto>> Get(int id)
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
            _logger.LogError(ex, "Error getting application note");
            return StatusCode(500, new { message = "An error occurred while getting note"});
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(ApplicationNoteCreateDto note)
    {
        try
        {
            var response = await _services.CreateAsync(note);
            if (response is null)
                return BadRequest();
            else
                return CreatedAtAction(nameof(Get), new { id = response.Id}, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating application note");
            return StatusCode(500, new { message = "An error occurred while creating applicatio note"});
        }

    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ApplicationNoteUpdateDto note)
    {
        try
        {
            var response =  await _services.UpdateAsync(id, note);
            if (!response)
                return NotFound();
            else    
                return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application note");
            return StatusCode(500, new { message = "An error occurred while updating applicatio note"});
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var response = await _services.DeleteAsync(id);
            if (!response)
                return NotFound();
            else
                return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting applicatio note");
            return StatusCode(500, new { message = "An error occurred while deleting applicatio note"});
        }
    }
}