using JobTracker.api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<List<Company>>> GetAll()
    {
        try
        {
            var companies = await _context.Companies.ToListAsync();
            return companies;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, new { message = "Ocurrió un error al obtener las empresas" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Company>> Get(int id)
    {
        try
        {
            var company = await _context.Companies.FindAsync(id);
            return company == null ? NotFound() : company;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(Company company)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _context.Companies.AddAsync(company);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = company.Id}, company);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        return BadRequest();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Company company)
    {
        if (id != company.Id)
            return BadRequest();
                
        if (ModelState.IsValid)
        {
            try
            {
                var companyDB = await _context.Companies.FindAsync(id);
                if (companyDB == null) return NotFound();
                
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
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest();
        }

        return NoContent();    
    }

}