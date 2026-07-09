using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobTracker.api.Models;
using JobTracker.api.Dtos.ApplicationNote;

namespace JobTracker.api.Controllers;

[ApiController]
[Route("[controller]")]
public class ApplicationNoteController : ControllerBase
{
    private readonly ApiDbContext _context;

    public ApplicationNoteController(ApiDbContext context)
    {
        _context = context;
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
                notesResponse.Add(new ApplicationNoteResponseDto{ Content = note.Content, CreatedAt = note.CreatedAt });

            }
            return notesResponse;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, new { message = "An error ocurred while getting notes"});
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
            return new ApplicationNoteResponseDto { Content = note.Content, CreatedAt = note.CreatedAt };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(ApplicatioNoteCreateDto note)
    {
        if (ModelState.IsValid)
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
                Console.WriteLine(ex.Message);
            }
        }

        return BadRequest();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ApplicationNoteUpdateDto note)
    {
        if (ModelState.IsValid)
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
            var note = await _context.Notes.FindAsync(id);
            if (note is null)
                return NotFound();
            
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest();
        }
    }
}