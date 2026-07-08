using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobTracker.api.Models;
using JobTracker.api.Dtos.JobApplication;
using JobTracker.api.Dtos.ApplicationNote;

namespace JobTracker.api.Controllers;

[ApiController]
[Route("[controller]")]
public class ApplicationNoteController : ControllerBase
{
    private readonly ApiDbContext _context;

    public ApplicationNoteController(ApiDbContext context)
    {
        _context = context;
    }

    
}