using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

    public async Task AddSessionAsync(
        TrainingModuleSession session,
        CancellationToken cancellationToken = default
    )
    {
        await dbContext.TrainingModuleSessions.AddAsync(session, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TrainingModuleEnrollment> EnrollAsync(
        SessionEnrollmentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (dbContext.Database.IsRelational())
        {
            if (dbContext.Database.CurrentTransaction is null)
                throw new InvalidOperationException(
                    "La inscripción atómica requiere una transacción ambiente."
                );

            // Serializa a los competidores por el último cupo antes de leer el
            // conteo. La fila de la convocatoria es también la fuente del cupo.
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT 1 FROM training_module_session WHERE id = {request.SessionId} FOR UPDATE",
                cancellationToken
            );
        }

        var session =
            await GetByIdForCalendarAsync(request.SessionId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"No se encontró ninguna convocatoria con el id '{request.SessionId}'."
            );

        session.EnsureCanEnroll(request.PersonId);

        var source = await GetOrCreateEnrollmentSourceByCodeAsync(
            request.EnrollmentSourceCode,
            request.EnrollmentSourceName,
            cancellationToken
        );

        var enrollment = new TrainingModuleEnrollment
        {
            TrainingModuleSessionId = session.Id,
            PersonId = request.PersonId,
            EnrollmentSourceId = source.Id,
            RegisteredByUserId = request.RegisteredByUserId,
        };

        dbContext.TrainingModuleEnrollments.Add(enrollment);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateEnrollment(exception))
        {
            throw new AlreadyEnrolledException();
        }

        return enrollment;
    }

    private static bool IsDuplicateEnrollment(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        } postgres
        && postgres.ConstraintName == "IX_training_module_enrollment_training_module_session_id_perso~";

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
