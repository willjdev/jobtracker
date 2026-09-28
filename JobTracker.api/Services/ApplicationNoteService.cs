using JobTracker.Api.Data;
using JobTracker.Api.Dtos.ApplicationNoteDto;
using JobTracker.Api.Models;
using JobTracker.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Services;

public class ApplicationNoteService : IApplicationNoteService
{
    private readonly ApiDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ApplicationNoteService(ApiDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ApplicationNoteResponseDto>> GetAllAsync()
    {
        return await _context.Notes
            .AsNoTracking()
            .Where(n => n.UserId == _currentUser.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new ApplicationNoteResponseDto
            {
                Id = n.Id,
                Content = n.Content,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ApplicationNoteResponseDto?> GetByIdAsync(int id)
    {
        var note = await _context.Notes.FirstOrDefaultAsync(n =>
        n.Id == id && n.UserId == _currentUser.UserId);
        if (note is null)
            return null;
        
        return new ApplicationNoteResponseDto{ Id = note.Id, Content = note.Content, CreatedAt = note.CreatedAt };
    }

    public async Task<ApplicationNoteResponseDto?> CreateAsync(ApplicationNoteCreateDto note)
    {
        var job = await _context.Applications.FirstOrDefaultAsync(j =>
        j.Id == note.JobApplicationId && j.UserId == _currentUser.UserId);
        if (job is null)
            return null;
        
        var newNote = new ApplicationNote
        {
            Content = note.Content,
            JobApplicationId = note.JobApplicationId,
            JobApplication = job,
            UserId = _currentUser.UserId!
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
        var noteDb = await _context.Notes.FirstOrDefaultAsync(n =>
        n.Id == id && n.UserId == _currentUser.UserId);
        if (noteDb is null)
            return false;
        
        noteDb.Content = note.Content;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var note = await _context.Notes.FirstOrDefaultAsync(n =>
        n.Id == id && n.UserId == _currentUser.UserId);
        if (note is null)
            return false;
        
        _context.Notes.Remove(note);
        await _context.SaveChangesAsync();

        return true;
    }
}