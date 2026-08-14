using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

public class TrainingModuleRepository(ScoutDbContext dbContext) : ITrainingModuleRepository
{
    public async Task<TrainingModule?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var trimmed = code.Trim();
        return await dbContext.TrainingModules
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == trimmed, cancellationToken);
    }

    public async Task<TrainingModule?> GetByIdAsync(long id, CancellationToken cancellationToken)
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
