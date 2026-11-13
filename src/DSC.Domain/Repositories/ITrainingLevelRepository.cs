using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

public interface ITrainingLevelRepository
{
    Task<IReadOnlyList<TrainingLevel>> ListAsync(CancellationToken cancellationToken);
    Task<TrainingLevel?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<TrainingLevel?> GetTrackedByCodeAsync(string code, CancellationToken cancellationToken);
    Task<IReadOnlyList<TrainingModule>> GetTrackedModulesByCodesAsync(
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken
    );
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
