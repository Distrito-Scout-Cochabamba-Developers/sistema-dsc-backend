using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

public interface IModuleAttendanceRepository
{
    Task<bool> ExistsAsync(Guid trainingModuleId, Guid scoutAdultId, CancellationToken cancellationToken);
    Task AddAsync(ModuleAttendance attendance, CancellationToken cancellationToken);
}
