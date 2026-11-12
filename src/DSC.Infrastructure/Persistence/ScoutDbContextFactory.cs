using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DSC.Infrastructure.Persistence;

/// <summary>
/// Permite generar migraciones sin cargar .env ni arrancar la API.
/// Para aplicarlas, proporcionar la conexión mediante --connection.
/// </summary>
public class ScoutDbContextFactory : IDesignTimeDbContextFactory<ScoutDbContext>
{
    public ScoutDbContext CreateDbContext(string[] args) =>
        new(
            new DbContextOptionsBuilder<ScoutDbContext>()
                .UseNpgsql(
                    "Host=localhost;Database=dsc_schema_generation;Username=unused;Password=unused",
                    options => options.MapEnum<DSC.Domain.Enums.GenderType>("gender_type")
                )
                .Options
        );
}
