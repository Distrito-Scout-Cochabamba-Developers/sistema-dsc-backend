using DSC.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence;

public class ScoutDbContext(DbContextOptions<ScoutDbContext> options) : DbContext(options)
{
    public DbSet<ScoutAdult> ScoutAdults => Set<ScoutAdult>();
    public DbSet<TrainingModule> TrainingModules => Set<TrainingModule>();
    public DbSet<ModuleAttendance> ModuleAttendances => Set<ModuleAttendance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScoutDbContext).Assembly);
    }
}
