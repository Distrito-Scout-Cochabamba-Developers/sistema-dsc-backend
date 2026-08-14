using DSC.Application.Commands;
using DSC.Application.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace DSC.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LookupLeaderByCiQueryHandler>();
        services.AddScoped<RegisterDigitalAttendanceCommandHandler>();

        return services;
    }
}
