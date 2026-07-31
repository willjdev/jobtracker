using JobTracker.Api.Models;
using JobTracker.Api.Dtos.Common;
using JobTracker.Api.Dtos.CompanyDto;
using JobTracker.Api.Services.Interfaces;
using JobTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Services;

public class CompanyServices : ICompanyService
{
    private readonly ApiDbContext _context;

    public CompanyServices(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<CompanyResponseDto>> GetAllAsync(CompanySearchDto search)
    {
        IQueryable<Company> query = _context.Companies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.Name))
            query = query.Where(c => c.Name == search.Name);
        if (!string.IsNullOrWhiteSpace(search.Location))
            query = query.Where(c => c.Location == search.Location);
        if (search.CreatedAt != null)
        {
            var startDate = search.CreatedAt.Value.Date;
            var endDate = startDate.AddDays(1);

            query = query.Where(c => c.CreatedAt >= startDate && c.CreatedAt < endDate);
        }
        if (!string.IsNullOrWhiteSpace(search.JobApplicationPosition))
            query = query.Where(c => c.JobApplications.Any(j => j.Position.Contains(search.JobApplicationPosition)));
        
        query = search.FieldName?.ToLower() switch
        {
            "name" => search.SortByType == "desc" ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "location" => search.SortByType == "desc" ? query.OrderByDescending(c => c.Location) : query.OrderBy(c => c.Location),
            "createdat" => search.SortByType == "desc" ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderBy(c => c.Id)
        };

        var totalRecords = await query.CountAsync();

        if (search.Page < 1)
            search.Page = 1;
        if (search.Records < 1)
            search.Records = 4;
        if (search.Records > 50)
            search.Records = 50;
        
        var companiesList = await query
            .Skip((search.Page - 1) * search.Records)
            .Take(search.Records)
            .Select(c => new CompanyResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Website = c.Website,
                Location = c.Location
            })
            .ToListAsync();
        
        var response = new PagedResponse<CompanyResponseDto>
        {
            Items = companiesList,
            Page = search.Page,
            Records = search.Records,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)search.Records)
        };

        return response;
    }

    public async Task<CompanyResponseDto?> GetByIdAsync(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company is null)
            return default;
        
        return new CompanyResponseDto
        {
            Id = company.Id,
            Name = company.Name,
            Description = company.Description,
            Website = company.Website,
            Location = company.Location
        };
    }

    public async Task<CompanyResponseDto?> CreateAsync(CompanyCreateDto company)
    {
        var newCompany = new Company
        {
            Name = company.Name,
            Description = company.Description,
            Website = company.Website,
            Location = company.Location
        };

        await _context.Companies.AddAsync(newCompany);
        await _context.SaveChangesAsync();

        return new CompanyResponseDto
        {
            Id = newCompany.Id,
            Name = newCompany.Name,
            Description = newCompany.Description,
            Website = newCompany.Website,
            Location = newCompany.Location
        };
    }

    public async Task<bool> UpdateAsync(int id, CompanyUpdateDto company)
    {
        var companyDb = await _context.Companies.FindAsync(id);
        if (companyDb is null)
            return false;
        
        companyDb.Name = company.Name;
        companyDb.Description = company.Description;

        if (company.Website != null)
            companyDb.Website = company.Website;
        if (companyDb.Location != null)
            companyDb.Location = company.Location;
        
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company is null)
            return false;
        
        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();

        return true;
    }
}