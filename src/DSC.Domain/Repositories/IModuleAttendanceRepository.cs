using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

public interface IModuleAttendanceRepository
{
    Task<bool> ExistsAsync(long trainingModuleId, long scoutLeaderId, CancellationToken cancellationToken);
    Task AddAsync(ModuleAttendance attendance, CancellationToken cancellationToken);
}
