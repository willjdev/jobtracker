using JobTracker.Api.Dtos.Common;
using JobTracker.Api.Dtos.JobApplicationDto;

namespace JobTracker.Api.Services.Interfaces;

public interface IJobApplicationService
{
    Task<PagedResponse<JobApplicationResponseDto>> GetAllAsync(JobApplicationSearchDto search);
    Task<JobApplicationResponseDto?> GetByIdAsync(int id);
    Task<JobApplicationResponseDto?> CreateAsync(JobApplicationCreateDto job);
    Task<bool> UpdateAsync(int id, JobApplicationUpdateDto job);
    Task<bool> DeleteAsync(int id);
}