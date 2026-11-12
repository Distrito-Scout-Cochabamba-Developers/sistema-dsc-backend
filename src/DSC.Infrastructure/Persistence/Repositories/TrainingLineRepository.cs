using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

public class TrainingLineRepository(ScoutDbContext dbContext) : ITrainingLineRepository
{
    public async Task<IReadOnlyList<TrainingLine>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext
            .TrainingLines.AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<TrainingLine?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken
    )
    {
        var trimmed = code.Trim();
        return await dbContext
            .TrainingLines.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == trimmed, cancellationToken);
    }
}
