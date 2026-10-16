using DSC.Application.Commands;
using DSC.Application.Interfaces;
using DSC.Application.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace DSC.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LookupLeaderByCiQueryHandler>();
        services.AddScoped<ListTrainingModulesQueryHandler>();
        services.AddScoped<GetTrainingModuleByCodeQueryHandler>();
        services.AddScoped<RegisterDigitalAttendanceCommandHandler>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<LoginWithUsernameCommandHandler>();

        return services;
    }
}
