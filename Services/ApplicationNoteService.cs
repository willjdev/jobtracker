using JobTracker.Api.Data;
using JobTracker.Api.Dtos.ApplicationNoteDto;
using JobTracker.Api.Models;
using JobTracker.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Services;

public class ApplicationNoteService : IApplicationNoteService
{
    private readonly ApiDbContext _context;

    public ApplicationNoteService(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<List<ApplicationNoteResponseDto>> GetAllAsync()
    {
        List<ApplicationNoteResponseDto> response = [];
        List<ApplicationNote> notes = await _context.Notes.ToListAsync();

        foreach (ApplicationNote note in notes)
        {
            response.Add(new ApplicationNoteResponseDto
            {
                Id = note.Id,
                Content = note.Content,
                CreatedAt = note.CreatedAt
            });
        }

        return response;
    }

    public async Task<ApplicationNoteResponseDto?> GetByIdAsync(int id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note is null)
            return null;
        
        return new ApplicationNoteResponseDto{ Id = note.Id, Content = note.Content, CreatedAt = note.CreatedAt };
    }

    public async Task<ApplicationNoteResponseDto?> CreateAsync(ApplicationNoteCreateDto note)
    {
        var job = await _context.Applications.FindAsync(note.JobApplicationId);
        if (job is null)
            return null;
        
        var newNote = new ApplicationNote
        {
            Content = note.Content,
            JobApplicationId = note.JobApplicationId,
            JobApplication = job
        };
        await _context.Notes.AddAsync(newNote);
        await _context.SaveChangesAsync();

        return new ApplicationNoteResponseDto
        {
            Id = newNote.Id,
            Content = newNote.Content,
            CreatedAt = newNote.CreatedAt
        };
    }

    public async Task<bool> UpdateAsync(int id, ApplicationNoteUpdateDto note)
    {
        var noteDb = await _context.Notes.FindAsync(id);
        if (noteDb is null)
            return false;
        
        noteDb.Content = note.Content;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note is null)
            return false;
        
        _context.Notes.Remove(note);
        await _context.SaveChangesAsync();

        return true;
    }
}