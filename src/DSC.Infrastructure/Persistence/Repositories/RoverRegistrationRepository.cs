using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

/// <summary>Alta de Rover sin cuenta sobre la transacción ambiente del <see cref="ScoutDbContext"/>.</summary>
public class RoverRegistrationRepository(ScoutDbContext dbContext) : IRoverRegistrationRepository
{
    public Task LockCiAsync(string ciNumber, CancellationToken cancellationToken) =>
        PersonCiLock.AcquireAsync(dbContext, ciNumber, required: false, cancellationToken);

    public async Task<IReadOnlyList<string?>> ExtensionsOfCiAsync(string ciNumber, CancellationToken cancellationToken) =>
        await dbContext.Persons.AsNoTracking().Where(p => p.IdCard == ciNumber).Select(p => p.IdCardExtension).ToListAsync(cancellationToken);

    public async Task<Person> CreateAsync(
        string ciNumber,
        string? extension,
        string firstName,
        string lastName,
        string? phone,
        CancellationToken cancellationToken
    )
    {
        var catalogs = await PersonCatalogs.ResolveAsync(dbContext, cancellationToken);
        var person = new Person
        {
            IdCard = ciNumber,
            IdCardExtension = extension,
            FirstName = firstName,
            FirstLastName = lastName,
            PersonStatusId = catalogs.ActiveStatusId,
            FormationCondition = "ROVER",
        };
        if (phone is not null)
        {
            person.ContactMechanisms.Add(new PersonContactMechanism
            {
                ContactMechanismTypeId = catalogs.CellphoneTypeId,
                ContactPurposeTypeId = catalogs.PersonalPurposeId,
                ContactValue = phone,
                IsPrimary = true,
            });
        }
        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync(cancellationToken);
        return person;
    }
}
