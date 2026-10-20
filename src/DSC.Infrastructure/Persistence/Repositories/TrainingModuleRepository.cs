using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

public class TrainingModuleRepository(ScoutDbContext dbContext) : ITrainingModuleRepository
{
    public async Task<TrainingModule?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken
    )
    {
        var trimmed = code.Trim();
        return await dbContext
            .TrainingModules.AsNoTracking()
            .Include(x => x.ModuleType)
            .FirstOrDefaultAsync(x => x.Code == trimmed, cancellationToken);
    }

    public async Task<TrainingModule?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await dbContext
            .TrainingModules.AsNoTracking()
            .Include(x => x.ModuleType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<TrainingModule?> GetByIdForUpdateAsync(
        long id,
        CancellationToken cancellationToken
    )
    {
        return await dbContext
            .TrainingModules.Include(x => x.ModuleType)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TrainingModule>> ListAsync(
        bool onlyActive,
        CancellationToken cancellationToken
    )
    {
        var query = dbContext.TrainingModules.AsNoTracking().Include(x => x.ModuleType);

        return await (onlyActive ? query.Where(x => x.IsActive) : query)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(
        string code,
        long? excludeId,
        CancellationToken cancellationToken
    )
    {
        var normalizedCode = code.Trim().ToLowerInvariant();
        return await dbContext
            .TrainingModules.AsNoTracking()
            .AnyAsync(
                x => x.Code.ToLower() == normalizedCode && (excludeId == null || x.Id != excludeId),
                cancellationToken
            );
    }

    public async Task<bool> ExistsActiveByNameAsync(
        string name,
        long? excludeId,
        CancellationToken cancellationToken
    )
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return await dbContext
            .TrainingModules.AsNoTracking()
            .AnyAsync(
                x =>
                    x.IsActive
                    && x.Name.ToLower() == normalizedName
                    && (excludeId == null || x.Id != excludeId),
                cancellationToken
            );
    }

    public async Task<bool> ModuleTypeExistsAsync(
        short moduleTypeId,
        CancellationToken cancellationToken
    )
    {
        return await dbContext
            .ModuleTypes.AsNoTracking()
            .AnyAsync(x => x.Id == moduleTypeId, cancellationToken);
    }

    public async Task AddAsync(TrainingModule trainingModule, CancellationToken cancellationToken)
    {
        await dbContext.TrainingModules.AddAsync(trainingModule, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
