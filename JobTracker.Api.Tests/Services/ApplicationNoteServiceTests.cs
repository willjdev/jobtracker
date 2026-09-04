using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Services;
using JobTracker.Api.Data;
using JobTracker.Api.Models;
using JobTracker.Api.Dtos.ApplicationNoteDto;

namespace JobTracker.Api.Services;

public class ApplicationNoteServiceTests
{
    private async Task SeedDatabaseAsync(ApiDbContext context)
    {
        var jobApplications = new List<JobApplication>
        {
            new()
            {
                Id = 1,
                Position = "Fullstack Developer",
                JobUrl = "https://www.job.com",
                AppliedAt = new DateTime(2026, 8, 17, 6, 20, 0),
                CompanyId = 1,
            },
            new()
            {
                Id = 2,
                Position = "Game Developer",
                AppliedAt = new DateTime(2026, 8, 18, 0, 0, 0),
                JobUrl = "https//www.job.com",
                CompanyId = 2
            },
            new()
            {
                Id = 3,
                Position = ".NET Developer",
                Status = "Meeting",
                AppliedAt = new DateTime(2026, 8, 20, 6, 40, 0),
                JobUrl = "https//www.job.com",
                CompanyId = 1
            },
            new()
            {
                Id = 4,
                Position = ".NET Developer",
                Status = "Interview",
                AppliedAt = new DateTime(2026, 8, 10, 6, 40, 0),
                JobUrl = "https//www.job.com",
                CompanyId = 4
            }
        };
        var notes = new List<ApplicationNote>
        {
            new()
            {
                Id = 1,
                Content = "I like this company",
                CreatedAt = new DateTime(2026, 7, 10, 6, 20, 0),
                JobApplicationId = 1
            },
            new()
            {
                Id = 2,
                Content = "Sent the email, let's wait for response",
                CreatedAt = new DateTime(2026, 8, 2, 10, 40, 0),
                JobApplicationId = 2
            },
            new()
            {
                Id = 3,
                Content = "Game company, sent resume",
                CreatedAt = new DateTime(2026, 8, 14, 7, 40, 0),
                JobApplicationId = 3
            },
            new()
            {
                Id = 4,
                Content = "Sent the email to the recruiter",
                CreatedAt = new DateTime(2026, 8, 18, 9, 20 ,0),
                JobApplicationId = 4
            }
        };
    
        context.Applications.AddRange(jobApplications);
        context.Notes.AddRange(notes);
        await context.SaveChangesAsync();
    }

    private ApiDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ApiDbContext(options);
    }

    private async Task<ApiDbContext> CreateSeededContextAsync()
    {
        var context = CreateContext();
        await SeedDatabaseAsync(context);
        return context;
    }

    [Fact]
    public async Task GetAllAsync_WhenApplicationNotesExists_ReturnsApplicationNoteResponseDtoList()
    {
        // Arrange
        using var context = await CreateSeededContextAsync();

        var service = new ApplicationNoteService(context);    

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.All(result, Assert.NotNull);
        Assert.All(result, item => Assert.True(item.Id > 0));
    }

    [Fact]
    public async Task GetAllAsync_WhenApplicationNotesDoesNotExist_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateContext();

        var service = new ApplicationNoteService(context);
        
        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(99, null)]
    public async Task GetByIdAsync_WhenFilteringById_ReturnsExpectedResponse(
        int id,
        int? expectedId
        )
    {
        // Arrange
        using var context = await CreateSeededContextAsync();

        var service = new ApplicationNoteService(context);
        var noteId = id;

        // Act
        var result = await service.GetByIdAsync(noteId);

        // Assert
        Assert.Equal(expectedId, result?.Id);
    }

    [Fact]
    public async Task CreateAsync_WhenJobApplicationExists_ReturnsCreatedNote()
    {
        // Arrange
        using var context = await CreateSeededContextAsync();

        var service = new ApplicationNoteService(context);
        var noteDto = new ApplicationNoteCreateDto
        { 
            Content = "This is a good company. This is a good job",
            JobApplicationId = 1 
        };

        // Act
        var result = await service.CreateAsync(noteDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(noteDto.Content, result.Content);

        var createdNote = await context.Notes.FirstOrDefaultAsync(n => n.Content == noteDto.Content);
        Assert.NotNull(createdNote);
        Assert.Equal(noteDto.JobApplicationId, createdNote.JobApplicationId);
    }

    [Fact]
    public async Task CreateAsync_WhenJobApplicationDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = await CreateSeededContextAsync();

        var service = new ApplicationNoteService(context);
        var noteDto = new ApplicationNoteCreateDto
        {
            Content = "Pretty well located job",
            JobApplicationId = 99
        };

        // Act
        var result = await service.CreateAsync(noteDto);

        // Assert
        Assert.Null(result);
    }
}
