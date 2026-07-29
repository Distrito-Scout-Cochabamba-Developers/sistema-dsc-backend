using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

public class TrainingModuleRepository(ScoutDbContext dbContext) : ITrainingModuleRepository
{
    public async Task<TrainingModule?> GetBySessionTokenAsync(string sessionToken, CancellationToken cancellationToken)
    {
        var trimmed = sessionToken.Trim();
        return await dbContext.TrainingModules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SessionToken == trimmed && x.IsActive, cancellationToken);
    }

    public async Task<TrainingModule?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.TrainingModules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(TrainingModule trainingModule, CancellationToken cancellationToken)
    {
        await dbContext.TrainingModules.AddAsync(trainingModule, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
