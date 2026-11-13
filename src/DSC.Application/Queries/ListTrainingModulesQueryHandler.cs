using DSC.Application.DTOs;
using DSC.Domain.Repositories;

namespace DSC.Application.Queries;

public record class ListTrainingModulesQuery(string? Search = null);

public class ListTrainingModulesQueryHandler(ITrainingModuleRepository moduleRepository)
{
    public async Task<IReadOnlyList<TrainingModuleDto>> HandleAsync(
        ListTrainingModulesQuery? query = null,
        CancellationToken cancellationToken = default
    )
    {
        var modules = await moduleRepository.ListAsync(query?.Search, cancellationToken);
        return modules.Select(TrainingModuleMapping.ToDto).ToArray();
    }

    public Task<IReadOnlyList<TrainingModuleDto>> HandleAsync(
        CancellationToken cancellationToken
    ) => HandleAsync(null, cancellationToken);
}
