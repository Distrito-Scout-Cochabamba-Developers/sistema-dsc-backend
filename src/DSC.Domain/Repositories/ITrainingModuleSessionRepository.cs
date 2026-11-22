using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

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

    Task<bool> ExistsEnrollmentAsync(
        long sessionId,
        long personId,
        CancellationToken cancellationToken = default
    );

    Task AddSessionAsync(
        TrainingModuleSession session,
        CancellationToken cancellationToken = default
    );

    Task AddEnrollmentAsync(
        TrainingModuleEnrollment enrollment,
        CancellationToken cancellationToken = default
    );
}
