using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

public class ModuleTypeRepository(ScoutDbContext dbContext) : IModuleTypeRepository
{
    public async Task<IReadOnlyList<ModuleType>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext
            .ModuleTypes.AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<ModuleType?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        var trimmed = code.Trim();
        return await dbContext
            .ModuleTypes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == trimmed, cancellationToken);
    }
}
