using DSC.Application.DTOs;
using DSC.Domain.Repositories;

namespace DSC.Application.Queries;

public record class GetTrainingModuleByCodeQuery(string Code);

public class GetTrainingModuleByCodeQueryHandler(ITrainingModuleRepository moduleRepository)
{
    public async Task<TrainingModuleDto?> HandleAsync(
        GetTrainingModuleByCodeQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var module = await moduleRepository.GetByCodeAsync(query.Code, cancellationToken);
        if (module is null)
            return null;

        return TrainingModuleDto.FromEntity(module);
    }
}
