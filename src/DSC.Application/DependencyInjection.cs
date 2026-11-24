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
        services.AddScoped<LookupSecretariatParticipantByCiQueryHandler>();
        services.AddScoped<ListTrainingModulesQueryHandler>();
        services.AddScoped<GetTrainingModuleByIdQueryHandler>();
        services.AddScoped<ListTrainingLinesQueryHandler>();
        services.AddScoped<ListTrainingLevelsQueryHandler>();
        services.AddScoped<GetTrainingLevelRequirementsQueryHandler>();
        services.AddScoped<ConfigureTrainingLevelRequirementsCommandHandler>();
        services.AddScoped<CreateTrainingModuleCommandHandler>();
        services.AddScoped<UpdateTrainingModuleCommandHandler>();
        services.AddScoped<DeleteTrainingModuleCommandHandler>();
        services.AddScoped<ListCalendarSessionsQueryHandler>();
        services.AddScoped<EnrollInCalendarSessionCommandHandler>();
        services.AddScoped<CreateTrainingModuleSessionCommandHandler>();
        services.AddScoped<RegisterDigitalAttendanceCommandHandler>();
        services.AddScoped<LoginCommandHandler>();

        return services;
    }
}
