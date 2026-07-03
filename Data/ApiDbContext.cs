using Microsoft.EntityFrameworkCore;
using JobTracker.api.Models;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JobApplication>()
            .HasOne<Company>()
            .WithMany()
            .HasForeignKey("CompanyId");
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<JobApplication> Applications { get; set; }
}