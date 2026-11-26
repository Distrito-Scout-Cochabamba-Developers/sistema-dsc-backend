using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de EF Core para gestión de Personas y Dirigentes Scouts.
/// </summary>
public class ScoutLeaderRepository(ScoutDbContext dbContext) : IScoutLeaderRepository
{
    public Task LockCiAsync(string ciNumber, CancellationToken cancellationToken, bool required = true) =>
        PersonCiLock.AcquireAsync(dbContext, ciNumber, required, cancellationToken);

    public Task<PersonCatalogIds> GetPersonCatalogIdsAsync(CancellationToken cancellationToken) =>
        PersonCatalogs.ResolveAsync(dbContext, cancellationToken);

    public async Task<Person> CreateSecretariatLeaderAsync(string ci, string? extension, string firstName, string lastName, string phone, string email, CancellationToken cancellationToken)
    {
        var catalogs = await PersonCatalogs.ResolveAsync(dbContext, cancellationToken);
        var emailTypeId = await PersonCatalogs.EmailAsync(dbContext, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var person = new Person { IdCard = ci, IdCardExtension = extension, FirstName = firstName, FirstLastName = lastName, PersonStatusId = catalogs.ActiveStatusId, FormationCondition = "LEADER", ScoutLeader = new ScoutLeader(), ContactMechanisms = [
            new() { ContactMechanismTypeId = catalogs.CellphoneTypeId, ContactPurposeTypeId = catalogs.PersonalPurposeId, ContactValue = phone, IsPrimary = true, ValidFrom = today },
            new() { ContactMechanismTypeId = emailTypeId, ContactPurposeTypeId = catalogs.PersonalPurposeId, ContactValue = email, IsPrimary = true, ValidFrom = today }
        ] };
        await AddAsync(person, cancellationToken);
        return person;
    }
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

    public async Task<IReadOnlyList<string?>> ExtensionsOfCiAsync(string ciNumber, CancellationToken cancellationToken) =>
        await dbContext.Persons.AsNoTracking().Where(p => p.IdCard == ciNumber).Select(p => p.IdCardExtension).ToListAsync(cancellationToken);

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
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505", ConstraintName: "ux_person_ci_extension" })
        {
            dbContext.Entry(person).State = EntityState.Detached;
            throw new PersonCiConflictException();
        }
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
