using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

public interface ITrainingLineRepository
{
    Task<IReadOnlyList<TrainingLine>> ListAsync(CancellationToken cancellationToken);

    Task<TrainingLine?> GetByCodeAsync(string code, CancellationToken cancellationToken);
}
