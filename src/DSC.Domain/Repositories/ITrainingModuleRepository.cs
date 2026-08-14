using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

public interface ITrainingModuleRepository
{
    Task<TrainingModule?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<TrainingModule?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task AddAsync(TrainingModule trainingModule, CancellationToken cancellationToken);
}
