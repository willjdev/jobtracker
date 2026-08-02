using Microsoft.AspNetCore.Mvc;
using JobTracker.Api.Dtos.CompanyDto;
using JobTracker.Api.Dtos.Common;
using JobTracker.Api.Services.Interfaces;

namespace JobTracker.Api.Controllers;

[ApiController]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    private readonly ILogger<CompaniesController> _logger;
    private readonly ICompanyService _services;

    public CompaniesController(ILogger<CompaniesController> logger, ICompanyService services)
    {
        _logger = logger;
        _services = services;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<CompanyResponseDto>>> GetAll([FromQuery] CompanySearchDto search)
    {
        try
        {
            PagedResponse<CompanyResponseDto> response = await _services.GetAllAsync(search);
            return response;
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
            var response = await _services.GetByIdAsync(id);
            if (response is null)
                return NotFound();
            else
                return response;
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
            var response = await _services.CreateAsync(company);
            if (response is null)
                return BadRequest();
            else 
                return CreatedAtAction(nameof(Get), new { id = response.Id}, response);
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
            var response = await _services.UpdateAsync(id, company);

            if (response)
                return NoContent();
            else
                return NotFound();
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
            var response = await _services.DeleteAsync(id);

            if (response)
                return NoContent();
            else
                return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting company");
            return StatusCode(500, new { message = "An error occurred while deleting company"});
        }
    }
}