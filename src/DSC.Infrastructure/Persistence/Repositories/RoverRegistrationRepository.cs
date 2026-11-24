using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

/// <summary>Alta de Rover sin cuenta sobre la transacción ambiente del <see cref="ScoutDbContext"/>.</summary>
public class RoverRegistrationRepository(ScoutDbContext dbContext) : IRoverRegistrationRepository
{
    public async Task LockCiAsync(string ciNumber, CancellationToken cancellationToken)
    {
        if (!dbContext.Database.IsRelational())
            return;
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({PersonCiLock.Namespace}, hashtext({PersonCiLock.KeyFor(ciNumber)}))",
            cancellationToken
        );
    }

    public Task<bool> CiExistsAsync(string ciNumber, CancellationToken cancellationToken) =>
        dbContext.Persons.AnyAsync(p => p.IdCard == ciNumber, cancellationToken);

    public async Task<Person> CreateAsync(
        string ciNumber,
        string? extension,
        string firstName,
        string lastName,
        string? phone,
        CancellationToken cancellationToken
    )
    {
        var status = await GetOrCreateAsync(
            dbContext.PersonStatuses, "person_status", "ACTIVE", "Activa", cancellationToken);
        var person = new Person
        {
            IdCard = ciNumber,
            IdCardExtension = extension,
            FirstName = firstName,
            FirstLastName = lastName,
            PersonStatusId = status.Id,
        };
        if (phone is not null)
        {
            var mechanism = await GetOrCreateAsync(
                dbContext.ContactMechanismTypes, "contact_mechanism_type", "CELLPHONE", "Celular", cancellationToken);
            var purpose = await GetOrCreateAsync(
                dbContext.ContactPurposeTypes, "contact_purpose_type", "PERSONAL", "Personal", cancellationToken);
            person.ContactMechanisms.Add(new PersonContactMechanism
            {
                ContactMechanismTypeId = mechanism.Id,
                ContactPurposeTypeId = purpose.Id,
                ContactValue = phone,
                IsPrimary = true,
            });
        }
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync(cancellationToken);
        return person;
    }

    // Instalaciones nuevas pueden no tener los catálogos: se crean de forma idempotente dentro de la transacción.
    private async Task<T> GetOrCreateAsync<T>(
        DbSet<T> set, string table, string code, string name, CancellationToken cancellationToken)
        where T : class
    {
        if (dbContext.Database.IsRelational())
#pragma warning disable EF1002 // table es una constante interna; code y name van parametrizados.
            await dbContext.Database.ExecuteSqlRawAsync(
                $"INSERT INTO {table} (code, name) VALUES ({{0}}, {{1}}) ON CONFLICT (code) DO NOTHING",
                [code, name],
                cancellationToken);
#pragma warning restore EF1002
        return await set.AsNoTracking().SingleAsync(e => EF.Property<string>(e, "Code") == code, cancellationToken);
    }
}
