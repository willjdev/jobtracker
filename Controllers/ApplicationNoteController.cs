using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Models;
using JobTracker.Api.Dtos.ApplicationNoteDto;
using JobTracker.Api.Data;
using JobTracker.Api.Services.Interfaces;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/application-notes")]
public class ApplicationNotesController : ControllerBase
{
    //private readonly ApiDbContext _context;
    private readonly ILogger<ApplicationNotesController> _logger;
    private readonly IApplicationNoteService _services;

    public ApplicationNotesController(ILogger<ApplicationNotesController> logger, IApplicationNoteService services)
    {
        //_context = context;
        _logger = logger;
        _services = services;
    }

    [HttpGet]
    public async Task<ActionResult<List<ApplicationNoteResponseDto>>> GetAll()
    {
        try
        {
            /* List<ApplicationNoteResponseDto> notesResponse = [];
            List<ApplicationNote> notes = await _context.Notes.ToListAsync();

            foreach (ApplicationNote note in notes)
            {
                notesResponse.Add(new ApplicationNoteResponseDto{ Id = note.Id, Content = note.Content, CreatedAt = note.CreatedAt });

            }
            return notesResponse; */
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
            /* var note = await _context.Notes.FindAsync(id);
            if (note is null)
                return NotFound();
            return new ApplicationNoteResponseDto { Id = note.Id, Content = note.Content, CreatedAt = note.CreatedAt }; */
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
            /* var job = await _context.Applications.FindAsync(note.JobApplicationId);
            if (job is null)
                return BadRequest();

            var newNote = new ApplicationNote{ Content = note.Content, JobApplicationId = note.JobApplicationId, JobApplication = job };
            await _context.Notes.AddAsync(newNote);
            await _context.SaveChangesAsync();
            var response = new ApplicationNoteResponseDto
            {
                Id = newNote.Id,
                Content = newNote.Content,
                CreatedAt = newNote.CreatedAt
            }; */
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
            /* var noteDb = await _context.Notes.FindAsync(id);
            if (noteDb is null || noteDb.Id != id)
                return NotFound();
            
            noteDb.Content = note.Content;

            await _context.SaveChangesAsync();

            return NoContent(); */
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
            /* var note = await _context.Notes.FindAsync(id);
            if (note is null)
                return NotFound();
            
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent(); */
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