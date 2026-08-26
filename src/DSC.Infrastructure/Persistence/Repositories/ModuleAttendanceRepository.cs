using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

public class ModuleAttendanceRepository(ScoutDbContext dbContext) : IModuleAttendanceRepository
{
    public async Task<bool> ExistsAsync(
        long trainingModuleId,
        long scoutLeaderId,
        CancellationToken cancellationToken
    )
    {
        return await dbContext.ModuleAttendances.AnyAsync(
            x => x.TrainingModuleId == trainingModuleId && x.ScoutLeaderId == scoutLeaderId,
            cancellationToken
        );
    }

    public async Task AddAsync(ModuleAttendance attendance, CancellationToken cancellationToken)
    {
        await dbContext.ModuleAttendances.AddAsync(attendance, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
