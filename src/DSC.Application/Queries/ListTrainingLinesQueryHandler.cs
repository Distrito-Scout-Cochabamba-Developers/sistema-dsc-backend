using DSC.Application.DTOs;
using DSC.Domain.Repositories;

namespace DSC.Application.Queries;

public class ListTrainingLinesQueryHandler(ITrainingLineRepository trainingLineRepository)
{
    public async Task<IReadOnlyList<TrainingLineDto>> HandleAsync(
        CancellationToken cancellationToken = default
    )
    {
        var lines = await trainingLineRepository.ListAsync(cancellationToken);
        return lines
            .Select(line => new TrainingLineDto(line.Id, line.Code, line.Name, line.Description))
            .ToArray();
    }
}
