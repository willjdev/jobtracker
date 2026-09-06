using Microsoft.EntityFrameworkCore;
using JobTracker.Api.Services;
using JobTracker.Api.Data;
using JobTracker.Api.Models;
using JobTracker.Api.Dtos.ApplicationNoteDto;

namespace JobTracker.Api.Tests.Services;

public class ApplicationNoteServiceTests
{
    private async Task SeedDatabaseAsync(ApiDbContext context)
    {
        var companies = new List<Company>
        {
            new()
            {
                Id = 1,
                Name = "Microsoft",
                Description = "Big Company",
                Website = "www.microsoft.com",
                Location = "Holand",
                CreatedAt = new DateTime(2026, 7, 10, 6, 10, 0),
            },
            new()
            {
                Id = 2,
                Name = "Santa Monica",
                Description = "Game Company",
                Website = "www.santamonica.com",
                Location = "Remote",
                CreatedAt = new DateTime(2026, 7, 21, 0, 0, 0),
            },
            new()
            {
                Id = 4,
                Name = "Microsoft Netherlands",
                Description = "Big Company",
                Website = "www.microsoft.com",
                Location = "Netherlands",
                CreatedAt = new DateTime(2026, 8, 4, 8, 20, 0)
            }
        };

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

        var resultOrder = result.Select(item => item.Id).ToArray();
        Assert.Equal(new[] {4, 3, 2, 1}, resultOrder);
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
    public async Task GetByIdAsync_GivenId_ReturnsExpectedResponse(
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
        if (expectedId is not null)
        {
            Assert.NotNull(result);
            Assert.Equal(expectedId, result.Id);
        }
        else
        {
            Assert.Null(result);
        }
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

    [Theory]
    [InlineData(1, true)]
    [InlineData(99, false)]
    public async Task UpdateAsync_WhenUpdatingApplicationNote_ReturnsExpectedResult(
        int inputNoteId,
        bool expectedResult
        )
    {
        // Arrange
        using var context = await CreateSeededContextAsync();

        var service = new ApplicationNoteService(context);
        var noteId = inputNoteId;
        var noteDto = new ApplicationNoteUpdateDto { Content = "Searched this company and it is good" };

        // Act
        var result = await service.UpdateAsync(noteId, noteDto);

        // Assert
        Assert.Equal(expectedResult, result);

        if (expectedResult)
        {
            var noteInDb = await context.Notes.FindAsync(noteId);
            Assert.Equal(noteDto.Content, noteInDb?.Content);
        }
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(99, false)]
    public async Task DeleteAsync_WhenDeletingApplicationNote_ReturnsExpectedResult(
        int inputNoteId,
        bool expectedResult
        )
    {
        // Arrange
        using var context = await CreateSeededContextAsync();

        var service = new ApplicationNoteService(context);
        var noteId = inputNoteId;
        
        // Act
        var notesCountBefore = await context.Notes.CountAsync();
        var result = await service.DeleteAsync(noteId);
        var notesCountAfter = await context.Notes.CountAsync();

        // Assert
        Assert.Equal(expectedResult, result);

        if (expectedResult)
        {
            var noteInDb = await context.Notes.FirstOrDefaultAsync(n => n.Id == noteId);
            Assert.Null(noteInDb);
            Assert.NotEqual(notesCountBefore, notesCountAfter);
        }
        else
        {
            Assert.Equal(notesCountBefore, notesCountAfter);
        }
    }
}
