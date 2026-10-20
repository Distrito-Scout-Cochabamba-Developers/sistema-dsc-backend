using DSC.Application.DTOs;
using DSC.Domain.Repositories;

namespace DSC.Application.Queries;

public record class ListTrainingModulesQuery(bool OnlyActive = false);

public class ListTrainingModulesQueryHandler(ITrainingModuleRepository moduleRepository)
{
    public async Task<IReadOnlyList<TrainingModuleDto>> HandleAsync(
        ListTrainingModulesQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var modules = await moduleRepository.ListAsync(query.OnlyActive, cancellationToken);
        return modules.Select(TrainingModuleDto.FromEntity).ToArray();
    }
}
