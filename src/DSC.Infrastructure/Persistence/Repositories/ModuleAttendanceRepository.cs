using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

public class ModuleAttendanceRepository(ScoutDbContext dbContext) : IModuleAttendanceRepository
{
    public async Task<bool> ExistsAsync(Guid trainingModuleId, Guid scoutAdultId, CancellationToken cancellationToken)
    {
        return await dbContext.ModuleAttendances
            .AsNoTracking()
            .AnyAsync(x => x.TrainingModuleId == trainingModuleId && x.ScoutAdultId == scoutAdultId, cancellationToken);
    }

    public async Task AddAsync(ModuleAttendance attendance, CancellationToken cancellationToken)
    {
        await dbContext.ModuleAttendances.AddAsync(attendance, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
