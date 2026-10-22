using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

public interface ITrainingModuleRepository
{
    Task<TrainingModule?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<TrainingModule?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TrainingModule>> ListAsync(CancellationToken cancellationToken);
    Task AddAsync(TrainingModule trainingModule, CancellationToken cancellationToken);

    /// <summary>
    /// Obtiene el módulo rastreado por EF para mutarlo (sin <c>AsNoTracking</c>).
    /// </summary>
    Task<TrainingModule?> GetTrackedByCodeAsync(string code, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// True si hay asistencia, sesiones o plantillas que impiden borrar el módulo.
    /// </summary>
    Task<bool> HasDependenciesAsync(long id, CancellationToken cancellationToken);

    Task RemoveAsync(TrainingModule trainingModule, CancellationToken cancellationToken);
}
