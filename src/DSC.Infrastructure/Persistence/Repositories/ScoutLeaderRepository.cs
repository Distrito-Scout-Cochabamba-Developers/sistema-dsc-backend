using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de EF Core para gestión de Personas y Dirigentes Scouts.
/// </summary>
public class ScoutLeaderRepository(ScoutDbContext dbContext) : IScoutLeaderRepository
{
    public async Task<IReadOnlyList<Person>> FindByCiAsync(
        string ciNumber,
        string? extension,
        CancellationToken cancellationToken
    )
    {
        var query = dbContext.Persons.AsNoTracking().Where(person => person.IdCard == ciNumber);
        if (extension is not null)
            query = query.Where(person =>
                person.IdCardExtension != null && person.IdCardExtension.ToUpper() == extension
            );

        return await query
            .Include(person => person.ContactMechanisms)
            .Include(person => person.User)
            .Include(person => person.ScoutLeader)
            .OrderBy(person => person.Id)
            .Take(2)
            .ToListAsync(cancellationToken);
    }

    public async Task<Person?> GetByCiNumberAsync(
        string ciNumber,
        CancellationToken cancellationToken
    )
    {
        var trimmed = ciNumber.Trim();
        return await dbContext
            .Persons.Include(p => p.ContactMechanisms)
            .Include(p => p.ScoutLeader)
            .FirstOrDefaultAsync(x => x.IdCard == trimmed, cancellationToken);
    }

    public async Task<Person?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await dbContext
            .Persons.Include(p => p.ContactMechanisms)
            .Include(p => p.ScoutLeader)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(Person person, CancellationToken cancellationToken)
    {
        await dbContext.Persons.AddAsync(person, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Person person, CancellationToken cancellationToken)
    {
        var tracked = dbContext.Persons.Local.FirstOrDefault(x => x.Id == person.Id);
        if (tracked != null && !ReferenceEquals(tracked, person))
        {
            dbContext.Entry(tracked).State = EntityState.Detached;
        }

        dbContext.Persons.Update(person);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
