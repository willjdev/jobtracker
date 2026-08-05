using JobTracker.Api.Dtos.Common;
using JobTracker.Api.Dtos.CompanyDto;

namespace JobTracker.Api.Services.Interfaces;

public interface ICompanyService
{
    Task<PagedResponse<CompanyResponseDto>> GetAllAsync(CompanySearchDto search);
    Task<CompanyResponseDto?> GetByIdAsync(int id);
    Task<CompanyResponseDto?> CreateAsync(CompanyCreateDto company);
    Task<bool> UpdateAsync(int id, CompanyUpdateDto company);
    Task<bool> DeleteAsync(int id); 
}