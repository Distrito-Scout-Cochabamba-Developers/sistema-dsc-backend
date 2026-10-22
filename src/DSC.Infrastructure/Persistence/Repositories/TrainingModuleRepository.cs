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
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TrainingModule>> ListAsync(CancellationToken cancellationToken)
    {
        return await dbContext
            .TrainingModules.AsNoTracking()
            .Include(x => x.ModuleType)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TrainingModule trainingModule, CancellationToken cancellationToken)
    {
        await dbContext.TrainingModules.AddAsync(trainingModule, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TrainingModule?> GetTrackedByCodeAsync(
        string code,
        CancellationToken cancellationToken
    )
    {
        var trimmed = code.Trim();
        return await dbContext.TrainingModules.FirstOrDefaultAsync(
            x => x.Code == trimmed,
            cancellationToken
        );
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task<bool> HasDependenciesAsync(long id, CancellationToken cancellationToken)
    {
        var hasAttendance = await dbContext.ModuleAttendances.AnyAsync(
            attendance => attendance.TrainingModuleId == id,
            cancellationToken
        );
        if (hasAttendance)
            return true;

        var hasSessions = await dbContext.TrainingModuleSessions.AnyAsync(
            session => session.TrainingModuleId == id,
            cancellationToken
        );
        if (hasSessions)
            return true;

        return await dbContext.TrainingModuleLessonPlanTemplates.AnyAsync(
            template => template.TrainingModuleId == id,
            cancellationToken
        );
    }

    public async Task RemoveAsync(TrainingModule trainingModule, CancellationToken cancellationToken)
    {
        dbContext.TrainingModules.Remove(trainingModule);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException(
                $"No se puede eliminar el módulo '{trainingModule.Code}' porque tiene asistencia, sesiones o plantillas asociadas."
            );
        }
    }
}
