using DSC.Application.DTOs;
using DSC.Domain.Repositories;

namespace DSC.Application.Queries;

public class ListTrainingModulesQueryHandler(ITrainingModuleRepository moduleRepository)
{
    public async Task<IReadOnlyList<TrainingModuleDto>> HandleAsync(
        CancellationToken cancellationToken = default
    )
    {
        var modules = await moduleRepository.ListAsync(cancellationToken);
        return modules
            .Select(module => new TrainingModuleDto(
                module.Id,
                module.Code,
                module.Name,
                module.Description,
                module.ModuleType?.Code ?? string.Empty,
                module.ModuleType?.Name ?? string.Empty
            ))
            .ToArray();
    }
}
