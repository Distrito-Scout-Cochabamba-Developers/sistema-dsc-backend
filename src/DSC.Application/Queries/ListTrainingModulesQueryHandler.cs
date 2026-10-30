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
        return modules.Select(TrainingModuleMapping.ToDto).ToArray();
    }
}
