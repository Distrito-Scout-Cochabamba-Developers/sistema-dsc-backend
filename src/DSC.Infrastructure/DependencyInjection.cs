using DSC.Domain.Repositories;
using DSC.Infrastructure.Persistence;
using DSC.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DSC.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString = "")
    {
        services.AddDbContext<ScoutDbContext>(options =>
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseInMemoryDatabase("DscInMemoryDb");
            }
            else
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ScoutDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                });
            }
        });

        services.AddScoped<IScoutLeaderRepository, ScoutLeaderRepository>();
        services.AddScoped<ITrainingModuleRepository, TrainingModuleRepository>();
        services.AddScoped<IModuleAttendanceRepository, ModuleAttendanceRepository>();

        return services;
    }
}
