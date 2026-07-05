using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobTracker.api.Models;

namespace JobTracker.api.Controllers;

[ApiController]
[Route("[controller]")]
public class JobApplicationController : ControllerBase
{
    private readonly ApiDbContext _context;

    public JobApplicationController(ApiDbContext context)
    {
        _context = context;
    }

    
}

