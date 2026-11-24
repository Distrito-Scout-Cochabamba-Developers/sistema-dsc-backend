using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DSC.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de EF Core para gestión de Personas y Dirigentes Scouts.
/// </summary>
public class ScoutLeaderRepository(ScoutDbContext dbContext) : IScoutLeaderRepository
{
    public async Task LockCiAsync(string ciNumber, CancellationToken cancellationToken, bool required = true)
    {
        // Proveedores no relacionales (pruebas en memoria) no tienen concurrencia real que serializar.
        if (!required && !dbContext.Database.IsNpgsql()) return;
        if (!dbContext.Database.IsNpgsql() || dbContext.Database.CurrentTransaction is null)
            throw new InvalidOperationException("La inscripción asistida requiere una transacción PostgreSQL.");
        if (dbContext.Database.CurrentTransaction.GetDbTransaction().IsolationLevel != System.Data.IsolationLevel.ReadCommitted)
            throw new InvalidOperationException("La inscripción asistida requiere READ COMMITTED.");
        var key = 158_000_000_000L + long.Parse(ciNumber, System.Globalization.CultureInfo.InvariantCulture);
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({key})", cancellationToken);
    }

    public async Task<Person> CreateSecretariatLeaderAsync(string ci, string? extension, string firstName, string lastName, string phone, string email, CancellationToken cancellationToken)
    {
        var status = await dbContext.PersonStatuses.SingleAsync(x => x.Code == "ACTIVE", cancellationToken);
        var purpose = await dbContext.ContactPurposeTypes.SingleAsync(x => x.Code == "PERSONAL", cancellationToken);
        var emailType = await dbContext.ContactMechanismTypes.SingleAsync(x => x.Code == "EMAIL", cancellationToken);
        var phoneType = await dbContext.ContactMechanismTypes.SingleAsync(x => x.Code == "CELLPHONE", cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var person = new Person { IdCard = ci, IdCardExtension = extension, FirstName = firstName, FirstLastName = lastName, PersonStatusId = status.Id, ScoutLeader = new ScoutLeader(), ContactMechanisms = [
            new() { ContactMechanismTypeId = phoneType.Id, ContactPurposeTypeId = purpose.Id, ContactValue = phone, IsPrimary = true, ValidFrom = today },
            new() { ContactMechanismTypeId = emailType.Id, ContactPurposeTypeId = purpose.Id, ContactValue = email, IsPrimary = true, ValidFrom = today }
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
