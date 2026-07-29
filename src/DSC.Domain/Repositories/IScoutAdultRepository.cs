using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

public interface IScoutAdultRepository
{
    Task<ScoutAdult?> GetByCiNumberAsync(string ciNumber, CancellationToken cancellationToken);
    Task<ScoutAdult?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(ScoutAdult scoutAdult, CancellationToken cancellationToken);
    Task UpdateAsync(ScoutAdult scoutAdult, CancellationToken cancellationToken);
}
