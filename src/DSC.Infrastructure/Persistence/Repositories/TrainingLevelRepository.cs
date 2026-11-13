using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

public class TrainingLevelRepository(ScoutDbContext dbContext) : ITrainingLevelRepository
{
    public async Task<IReadOnlyList<TrainingLevel>> ListAsync(
        CancellationToken cancellationToken
    ) =>
        await dbContext
            .TrainingLevels.AsNoTracking()
            .Include(level => level.TrainingLine)
            .OrderBy(level => level.Name)
            .ThenBy(level => level.Code)
            .ToListAsync(cancellationToken);

    public Task<TrainingLevel?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
        WithModuleRequirements(dbContext.TrainingLevels.AsNoTracking())
            .SingleOrDefaultAsync(level => level.Code == code, cancellationToken);

    public Task<TrainingLevel?> GetTrackedByCodeAsync(
        string code,
        CancellationToken cancellationToken
    ) =>
        WithModuleRequirements(dbContext.TrainingLevels)
            .SingleOrDefaultAsync(level => level.Code == code, cancellationToken);

    private static IQueryable<TrainingLevel> WithModuleRequirements(
        IQueryable<TrainingLevel> levels
    ) =>
        levels
            .Include(level =>
                level.Requirements.Where(requirement =>
                    requirement.RequirementType == TrainingLevelRequirement.ModulesType
                )
            )
                .ThenInclude(requirement => requirement.MandatoryModules)
                    .ThenInclude(link => link.TrainingModule);

    public async Task<IReadOnlyList<TrainingModule>> GetTrackedModulesByIdsAsync(
        IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken
    ) =>
        await dbContext
            .TrainingModules.Where(module => ids.Contains(module.Id))
            .ToListAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "No se pudo guardar la configuración. Recargue el nivel y verifique las referencias antes de reintentar.",
                ex
            );
        }
    }
}
