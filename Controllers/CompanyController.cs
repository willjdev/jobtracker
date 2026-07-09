using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobTracker.api.Models;
using JobTracker.api.Dtos.CompanyDto;

namespace JobTracker.api.Controllers;

[ApiController]
[Route("[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ApiDbContext _context;

    public CompaniesController(ApiDbContext context)
    {
        _context = context;
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
            Console.WriteLine(ex.Message);
            return StatusCode(500, new { message = "Ocurrió un error al obtener las empresas" });
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
            Console.WriteLine(ex.Message);
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CompanyCreateDto company)
    {
        if (ModelState.IsValid)
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
                Console.WriteLine(ex.Message);
            }
        }

        return BadRequest();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CompanyUpdateDto company)
    {           
        if (ModelState.IsValid)
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
            var company = await _context.Companies.FindAsync(id);

            if (company is null)
                return NotFound();

            _context.Companies.Remove(company);
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