using JobTracker.Api.Dtos.ApplicationNoteDto;
using JobTracker.Api.Models;

namespace JobTracker.Api.Services.Interfaces;

public interface IApplicationNoteService
{
    Task<List<ApplicationNoteResponseDto>> GetAllAsync();
    Task<ApplicationNoteResponseDto?> GetByIdAsync(int id);
    Task<ApplicationNoteResponseDto?> CreateAsync(ApplicationNoteCreateDto note);
    Task<bool> UpdateAsync(int id, ApplicationNoteUpdateDto note);
    Task<bool> DeleteAsync(int id);
    
}