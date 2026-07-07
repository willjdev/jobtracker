using Microsoft.EntityFrameworkCore;
using JobTracker.api.Models;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JobApplication>()
            .HasOne(j => j.Company)
            //Cada JobApplication tiene una empresa asociada, la cual se guarda en la propiedad Company, que esta definido en el modelo de JobApplication
            .WithMany(c => c.JobApplications)
            // Cada COmpany puede tener muchas solicitudes, las cuales se agrupan en su lista JobApplications, la cual se define en el modelo de Company
            .HasForeignKey(ja => ja.CompanyId);
            // Para amarrar este puente en la base de datos, se utiliza la columna CompanyId como la clave foránea
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<JobApplication> Applications { get; set; }
}