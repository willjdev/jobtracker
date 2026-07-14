using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Models;
using JobTracker.Api.Dtos.CompanyDto;
using JobTracker.Api.Data;

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
    public async Task<ActionResult<List<CompanyResponseDto>>> GetAll()
    {
        try
        {
            List<CompanyResponseDto> companiesResponse = [];
            var companies = await _context.Companies.ToListAsync();

            foreach (Company company in companies)
            {
                companiesResponse.Add( new CompanyResponseDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    Description = company.Description,
                    Website = company.Website,
                    Location = company.Location
                });
            }

            return companiesResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting companies");
            return StatusCode(500, new { message = "An error occurred while getting companies" });
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
            await _context.Companies.AddAsync(newCompany);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = newCompany.Id}, company);
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

}