using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

public class ScoutAdultRepository(ScoutDbContext dbContext) : IScoutAdultRepository
{
    public async Task<ScoutAdult?> GetByCiNumberAsync(string ciNumber, CancellationToken cancellationToken)
    {
        var trimmed = ciNumber.Trim();
        return await dbContext.ScoutAdults
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Ci.Number == trimmed, cancellationToken);
    }

    public async Task<ScoutAdult?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.ScoutAdults
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(ScoutAdult scoutAdult, CancellationToken cancellationToken)
    {
        await dbContext.ScoutAdults.AddAsync(scoutAdult, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ScoutAdult scoutAdult, CancellationToken cancellationToken)
    {
        var tracked = dbContext.ScoutAdults.Local.FirstOrDefault(x => x.Id == scoutAdult.Id);
        if (tracked != null && !ReferenceEquals(tracked, scoutAdult))
        {
            dbContext.Entry(tracked).State = EntityState.Detached;
        }

        dbContext.ScoutAdults.Update(scoutAdult);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
