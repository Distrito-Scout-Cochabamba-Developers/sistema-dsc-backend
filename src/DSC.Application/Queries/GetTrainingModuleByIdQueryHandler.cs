using DSC.Application.DTOs;
using DSC.Domain.Repositories;

namespace DSC.Application.Queries;

public record class GetTrainingModuleByIdQuery(long Id);

public class GetTrainingModuleByIdQueryHandler(ITrainingModuleRepository moduleRepository)
{
    public async Task<TrainingModuleDto?> HandleAsync(
        GetTrainingModuleByIdQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var module = await moduleRepository.GetByIdAsync(query.Id, cancellationToken);
        if (module is null)
            return null;

        return TrainingModuleMapping.ToDto(module);
    }
}
