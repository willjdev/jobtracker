using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobTracker.api.Models;
using JobTracker.api.Dtos.ApplicationNote;

namespace JobTracker.api.Controllers;

[ApiController]
[Route("api/application-notes")]
public class ApplicationNotesController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ILogger<ApplicationNotesController> _logger;

    public ApplicationNotesController(ApiDbContext context, ILogger<ApplicationNotesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ApplicationNoteResponseDto>>> GetAll()
    {
        try
        {
            List<ApplicationNoteResponseDto> notesResponse = [];
            List<ApplicationNote> notes = await _context.Notes.ToListAsync();

            foreach (ApplicationNote note in notes)
            {
                notesResponse.Add(new ApplicationNoteResponseDto{ Id = note.Id, Content = note.Content, CreatedAt = note.CreatedAt });

            }
            return notesResponse;
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
            var note = await _context.Notes.FindAsync(id);
            if (note is null)
                return NotFound();
            return new ApplicationNoteResponseDto { Id = note.Id, Content = note.Content, CreatedAt = note.CreatedAt };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting application note");
            return StatusCode(500, new { message = "An error occurred while getting note"});
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(ApplicatioNoteCreateDto note)
    {
        try
        {
            var job = await _context.Applications.FindAsync(note.JobApplicationId);
            if (job is null)
                return BadRequest();

            var newNote = new ApplicationNote{ Content = note.Content, JobApplicationId = note.JobApplicationId, JobApplication = job };
            await _context.Notes.AddAsync(newNote);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = newNote.Id}, note);
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
            var noteDb = await _context.Notes.FindAsync(id);
            if (noteDb is null || noteDb.Id != id)
                return BadRequest();
            
            noteDb.Content = note.Content;

            await _context.SaveChangesAsync();

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
            var note = await _context.Notes.FindAsync(id);
            if (note is null)
                return NotFound();
            
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting applicatio note");
            return StatusCode(500, new { message = "An error occurred while deleting applicatio note"});
        }
    }
}