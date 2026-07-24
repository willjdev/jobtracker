using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Models;
using JobTracker.Api.Dtos.CompanyDto;
using JobTracker.Api.Data;
using System.IO.Compression;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(ApiDbContext context, ILogger<CompaniesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompanyResponseDto>>> GetAll([FromQuery] CompanySearchDto search)
    {
        try
        {
            IQueryable<Company> query = _context.Companies.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.Name))
                query = query.Where(c => c.Name.Contains(search.Name));
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
                "name" => search.SortByType?.ToLower() == "desc" ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "location" => search.SortByType?.ToLower() == "desc" ? query.OrderByDescending(c => c.Location) : query.OrderBy(c => c.Location),
                "createdat" => search.SortByType?.ToLower() == "desc" ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                _ => query.OrderBy(c => c.Id)
            };

            query = query.Skip((search.Page - 1) * search.Records).Take(search.Records);

            var companiesList = await query.Select(c => new CompanyResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Website = c.Website,
                Location = c.Location
            })
            .ToListAsync();

            return companiesList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting search");
            return StatusCode(500, new { message = "An error occurred while getting companies searched"});
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyResponseDto>> Get(int id)
    {
        try
        {
            var company = await _context.Companies.FindAsync(id);
            if (company is null)
                return NotFound();
            
            return new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description,
                Website = company.Website,
                Location = company.Location
            };
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error getting company");
            return StatusCode(500, new { message = "An error occurred while getting company"});
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CompanyCreateDto company)
    {
        try
        {
            var newCompany = new Company{ Name = company.Name, Description = company.Description, Website = company.Website, Location = company.Location };
            var response = new CompanyResponseDto
            {
                Id = newCompany.Id,
                Name = newCompany.Name,
                Description = newCompany.Description,
                Website = newCompany.Website,
                Location = newCompany.Location
            };
            await _context.Companies.AddAsync(newCompany);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = newCompany.Id}, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company");
            return StatusCode(500, new { message = "An error occurred while creating company"});
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CompanyUpdateDto company)
    {           
        try
        {
            var companyDB = await _context.Companies.FindAsync(id);
            if (companyDB == null || companyDB.Id != id) return NotFound();
            
            companyDB.Name = company.Name;
            companyDB.Description = company.Description;

            if (company.Website != null)
                companyDB.Website = company.Website;
            if (company.Location != null)
                companyDB.Location = company.Location;

            await _context.SaveChangesAsync();

            return NoContent();

        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Error updating company");
            return StatusCode(500, new { message = "An error occurred while updating company"});
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var company = await _context.Companies.FindAsync(id);

            if (company is null)
                return NotFound();

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            
            return NoContent();    
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company");
            return StatusCode(500, new { message = "An error occurred while deleting company"});
        }
    }

    // *****************************************************************************************************

    



}