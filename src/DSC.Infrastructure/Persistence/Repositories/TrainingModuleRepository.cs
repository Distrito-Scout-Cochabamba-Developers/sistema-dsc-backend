using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

public class TrainingModuleRepository(ScoutDbContext dbContext) : ITrainingModuleRepository
{
    public async Task<TrainingModule?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .TrainingModules.AsNoTracking()
            .Include(x => x.ModuleLines)
                .ThenInclude(x => x.TrainingLine)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<TrainingModule?> GetTrackedByIdAsync(
        long id,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .TrainingModules.Include(x => x.ModuleLines)
                .ThenInclude(x => x.TrainingLine)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<TrainingModule?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        var trimmed = name.Trim();
        return await dbContext
            .TrainingModules.AsNoTracking()
            .Include(x => x.ModuleLines)
                .ThenInclude(x => x.TrainingLine)
            .FirstOrDefaultAsync(x => x.Name.ToLower() == trimmed.ToLower(), cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        long? excludeId = null,
        CancellationToken cancellationToken = default
    )
    {
        var trimmed = name.Trim();
        return await dbContext.TrainingModules.AnyAsync(
            x =>
                x.Name.ToLower() == trimmed.ToLower()
                && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<TrainingModule>> ListAsync(
        string? search = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .TrainingModules.AsNoTracking()
            .Include(x => x.ModuleLines)
                .ThenInclude(x => x.TrainingLine)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = search.Trim();
            query =
                dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory"
                    ? query.Where(x => x.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    : query.Where(x => EF.Functions.ILike(x.Name, $"%{pattern}%"));
        }

        return await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TrainingModule trainingModule,
        CancellationToken cancellationToken = default
    )
    {
        await dbContext.TrainingModules.AddAsync(trainingModule, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task<bool> HasDependenciesAsync(
        long id,
        CancellationToken cancellationToken = default
    )
    {
        if (
            await dbContext.TrainingLevelMandatoryModules.AnyAsync(
                link => link.TrainingModuleId == id,
                cancellationToken
            )
        )
            return true;

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

    public async Task RemoveAsync(
        TrainingModule trainingModule,
        CancellationToken cancellationToken = default
    )
    {
        dbContext.TrainingModules.Remove(trainingModule);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException(
                $"No se puede eliminar el módulo '{trainingModule.Name}' porque tiene requisitos de nivel, asistencia, sesiones o plantillas asociadas."
            );
        }
    }
}
