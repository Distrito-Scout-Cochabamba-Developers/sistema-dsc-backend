using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

/// <summary>
/// Datos mínimos de una inscripción compartida por autoservicio y Secretaría.
/// El origen y el operador los decide el canal; la operación no crea un
/// contador aparte: usa el cupo de <see cref="TrainingModuleSession"/>.
/// </summary>
public record SessionEnrollmentRequest(
    long SessionId,
    long PersonId,
    string EnrollmentSourceCode,
    string EnrollmentSourceName,
    long? RegisteredByUserId,
    bool RoverWithoutAccount = false
);

public interface ITrainingModuleSessionRepository
{
    Task<IReadOnlyList<TrainingModuleSession>> ListForCalendarAsync(
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken = default
    );

    Task<TrainingModuleSession?> GetByIdForCalendarAsync(
        long id,
        CancellationToken cancellationToken = default
    );

    Task<TrainingModuleSessionStatus?> GetStatusByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    );

    Task<TrainingModuleSessionStatus> GetOrCreateStatusByCodeAsync(
        string code,
        string name,
        CancellationToken cancellationToken = default
    );

    Task<EnrollmentSource?> GetEnrollmentSourceByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    );

    Task<EnrollmentSource> GetOrCreateEnrollmentSourceByCodeAsync(
        string code,
        string name,
        CancellationToken cancellationToken = default
    );

    Task<Facility?> GetFacilityByIdAsync(
        long id,
        CancellationToken cancellationToken = default
    );

    Task AddSessionAsync(
        TrainingModuleSession session,
        CancellationToken cancellationToken = default
    );

    Task ExpandCapacityAsync(
        long sessionId,
        int? capacity,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Inscribe a una persona validando ventana, duplicado y cupo en una sola
    /// operación. En PostgreSQL requiere una transacción ambiente (la abre
    /// <c>IUnitOfWork</c>) de nivel READ COMMITTED y bloquea la fila de la
    /// convocatoria para serializar competidores por el último cupo.
    /// </summary>
    Task<TrainingModuleEnrollment> EnrollAsync(
        SessionEnrollmentRequest request,
        CancellationToken cancellationToken = default
    );
}
