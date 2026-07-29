using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

public interface ITrainingModuleRepository
{
    Task<TrainingModule?> GetBySessionTokenAsync(string sessionToken, CancellationToken cancellationToken);
    Task<TrainingModule?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(TrainingModule trainingModule, CancellationToken cancellationToken);
}
