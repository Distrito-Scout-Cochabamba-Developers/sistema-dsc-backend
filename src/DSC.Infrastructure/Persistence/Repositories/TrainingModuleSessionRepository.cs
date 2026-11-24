using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

public class TrainingModuleSessionRepository(ScoutDbContext dbContext)
    : ITrainingModuleSessionRepository
{
    public async Task<IReadOnlyList<TrainingModuleSession>> ListForCalendarAsync(
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default
    )
    {
        var query = CalendarQuery();

        if (from.HasValue)
            query = query.Where(session =>
                session.Schedules.Any(block => block.SessionDate >= from.Value)
            );

        if (to.HasValue)
            query = query.Where(session =>
                session.Schedules.Any(block => block.SessionDate <= to.Value)
            );

        return await query
            .OrderBy(session => session.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<TrainingModuleSession?> GetByIdForCalendarAsync(
        long id,
        CancellationToken cancellationToken = default
    ) => CalendarQuery().FirstOrDefaultAsync(session => session.Id == id, cancellationToken);

    public Task<TrainingModuleSessionStatus?> GetStatusByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var normalized = code.Trim();
        return dbContext
            .TrainingModuleSessionStatuses.AsNoTracking()
            .FirstOrDefaultAsync(
                status => status.Code.ToLower() == normalized.ToLower(),
                cancellationToken
            );
    }

    public async Task<TrainingModuleSessionStatus> GetOrCreateStatusByCodeAsync(
        string code,
        string name,
        CancellationToken cancellationToken = default
    )
    {
        var normalized = code.Trim();
        var existing = await dbContext.TrainingModuleSessionStatuses.FirstOrDefaultAsync(
            status => status.Code.ToLower() == normalized.ToLower(),
            cancellationToken
        );
        if (existing is not null)
            return existing;

        var created = new TrainingModuleSessionStatus
        {
            Code = normalized.ToUpperInvariant(),
            Name = name,
            Description = name,
        };
        dbContext.TrainingModuleSessionStatuses.Add(created);
        await dbContext.SaveChangesAsync(cancellationToken);
        return created;
    }

    public Task<EnrollmentSource?> GetEnrollmentSourceByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    )
    {
        var normalized = code.Trim();
        return dbContext
            .EnrollmentSources.AsNoTracking()
            .FirstOrDefaultAsync(
                source => source.Code.ToLower() == normalized.ToLower(),
                cancellationToken
            );
    }

    public async Task<EnrollmentSource> GetOrCreateEnrollmentSourceByCodeAsync(
        string code,
        string name,
        CancellationToken cancellationToken = default
    )
    {
        var normalized = code.Trim();
        var existing = await dbContext.EnrollmentSources.FirstOrDefaultAsync(
            source => source.Code.ToLower() == normalized.ToLower(),
            cancellationToken
        );
        if (existing is not null)
            return existing;

        var created = new EnrollmentSource
        {
            Code = normalized.ToUpperInvariant(),
            Name = name,
            Description = name,
        };
        dbContext.EnrollmentSources.Add(created);
        await dbContext.SaveChangesAsync(cancellationToken);
        return created;
    }

    public Task<Facility?> GetFacilityByIdAsync(
        long id,
        CancellationToken cancellationToken = default
    ) =>
        dbContext
            .Facilities.AsNoTracking()
            .FirstOrDefaultAsync(facility => facility.Id == id, cancellationToken);

    public Task<bool> ExistsEnrollmentAsync(
        long sessionId,
        long personId,
        CancellationToken cancellationToken = default
    ) =>
        dbContext.TrainingModuleEnrollments.AnyAsync(
            enrollment =>
                enrollment.TrainingModuleSessionId == sessionId && enrollment.PersonId == personId,
            cancellationToken
        );

    public async Task AddSessionAsync(
        TrainingModuleSession session,
        CancellationToken cancellationToken = default
    )
    {
        await dbContext.TrainingModuleSessions.AddAsync(session, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddEnrollmentAsync(
        TrainingModuleEnrollment enrollment,
        CancellationToken cancellationToken = default
    )
    {
        await dbContext.TrainingModuleEnrollments.AddAsync(enrollment, cancellationToken);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Ya estás inscrito en esta sesión.");
        }
    }

    public async Task ExpandCapacityAsync(
        long sessionId,
        int? capacity,
        CancellationToken cancellationToken = default
    )
    {
        var session = dbContext.TrainingModuleSessions.Where(item => item.Id == sessionId);
        var updated = await session
            .Where(item => capacity == null || (item.Capacity != null && item.Capacity <= capacity))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(item => item.Capacity, capacity),
                cancellationToken
            );
        if (updated != 0)
            return;

        if (!await session.AnyAsync(cancellationToken))
            throw new KeyNotFoundException(
                $"No se encontró la convocatoria con el id '{sessionId}'."
            );

        throw new InvalidOperationException("La ampliación no puede reducir el cupo actual.");
    }

    private IQueryable<TrainingModuleSession> CalendarQuery() =>
        dbContext
            .TrainingModuleSessions.AsNoTracking()
            .Include(session => session.Status)
            .Include(session => session.TrainingModule)
                .ThenInclude(module => module.ModuleLines)
                    .ThenInclude(link => link.TrainingLine)
            .Include(session => session.Schedules)
                .ThenInclude(block => block.Facility)
            .Include(session => session.Enrollments)
            .Where(session => session.Schedules.Any());
}
