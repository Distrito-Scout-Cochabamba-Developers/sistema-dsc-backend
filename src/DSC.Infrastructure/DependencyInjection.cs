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
                // Sqlite or SqlServer can be passed here
                options.UseInMemoryDatabase("DscInMemoryDb");
            }
        });

        services.AddScoped<IScoutAdultRepository, ScoutAdultRepository>();
        services.AddScoped<ITrainingModuleRepository, TrainingModuleRepository>();
        services.AddScoped<IModuleAttendanceRepository, ModuleAttendanceRepository>();

        return services;
    }
}
