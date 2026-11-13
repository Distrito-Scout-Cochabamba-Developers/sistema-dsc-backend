using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

public interface ITrainingModuleRepository
{
    Task<TrainingModule?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<TrainingModule?> GetTrackedByIdAsync(
        long id,
        CancellationToken cancellationToken = default
    );
    Task<TrainingModule?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    );
    Task<bool> ExistsByNameAsync(
        string name,
        long? excludeId = null,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<TrainingModule>> ListAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(TrainingModule trainingModule, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// True si hay requisitos de nivel, asistencia, sesiones o plantillas que impiden borrar el módulo.
    /// </summary>
    Task<bool> HasDependenciesAsync(long id, CancellationToken cancellationToken = default);

    Task RemoveAsync(TrainingModule trainingModule, CancellationToken cancellationToken = default);
}
