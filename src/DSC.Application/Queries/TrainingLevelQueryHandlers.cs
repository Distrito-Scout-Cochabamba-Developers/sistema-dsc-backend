using DSC.Application.DTOs;
using DSC.Domain.Repositories;

namespace DSC.Application.Queries;

public class ListTrainingLevelsQueryHandler(ITrainingLevelRepository repository)
{
    public async Task<IReadOnlyList<TrainingLevelDto>> HandleAsync(
        CancellationToken cancellationToken = default
    )
    {
        var levels = await repository.ListAsync(cancellationToken);
        return levels
            .Select(level => new TrainingLevelDto(
                level.Code,
                level.Name,
                level.TrainingLine.Code,
                level.IsActive
            ))
            .ToArray();
    }
}

public class GetTrainingLevelRequirementsQueryHandler(ITrainingLevelRepository repository)
{
    public async Task<TrainingLevelRequirementsDto> HandleAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var level =
            await repository.GetByCodeAsync(code.Trim(), cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró el nivel '{code}'.");
        return TrainingLevelRequirementsDto.From(level);
    }
}
