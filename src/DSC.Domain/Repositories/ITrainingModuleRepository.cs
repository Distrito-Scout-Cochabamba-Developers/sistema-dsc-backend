using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

public interface ITrainingModuleRepository
{
    Task<TrainingModule?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<TrainingModule?> GetByIdAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    /// Carga el módulo con seguimiento de cambios para modificarlo y luego llamar a <see cref="SaveChangesAsync"/>.
    /// </summary>
    Task<TrainingModule?> GetByIdForUpdateAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TrainingModule>> ListAsync(
        bool onlyActive,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Indica si otro módulo (activo o no) ya usa el código. Sin distinguir mayúsculas.
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, long? excludeId, CancellationToken cancellationToken);

    /// <summary>
    /// Indica si otro módulo activo ya usa el nombre. Sin distinguir mayúsculas.
    /// </summary>
    Task<bool> ExistsActiveByNameAsync(
        string name,
        long? excludeId,
        CancellationToken cancellationToken
    );

    Task<bool> ModuleTypeExistsAsync(short moduleTypeId, CancellationToken cancellationToken);
    Task AddAsync(TrainingModule trainingModule, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
