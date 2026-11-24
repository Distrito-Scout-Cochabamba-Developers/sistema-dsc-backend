using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

/// <summary>
/// Resolución única, por código, de los catálogos que requiere el alta de personas. Los códigos los siembra
/// la migración <c>SecretariatCatalogs</c>; si falta alguno se lanza <see cref="CatalogMissingException"/>
/// (error del servidor) en lugar de crearlo durante la solicitud.
/// </summary>
internal static class PersonCatalogs
{
    public const string ActiveStatus = "ACTIVE";
    public const string CellphoneMechanism = "CELLPHONE";
    public const string EmailMechanism = "EMAIL";
    public const string PersonalPurpose = "PERSONAL";

    public static async Task<PersonCatalogIds> ResolveAsync(ScoutDbContext db, CancellationToken cancellationToken)
    {
        var status = await db.PersonStatuses.AsNoTracking().Where(x => x.Code == ActiveStatus).Select(x => (short?)x.Id).SingleOrDefaultAsync(cancellationToken)
            ?? throw new CatalogMissingException("person_status", ActiveStatus);
        var phone = await db.ContactMechanismTypes.AsNoTracking().Where(x => x.Code == CellphoneMechanism).Select(x => (short?)x.Id).SingleOrDefaultAsync(cancellationToken)
            ?? throw new CatalogMissingException("contact_mechanism_type", CellphoneMechanism);
        var purpose = await db.ContactPurposeTypes.AsNoTracking().Where(x => x.Code == PersonalPurpose).Select(x => (short?)x.Id).SingleOrDefaultAsync(cancellationToken)
            ?? throw new CatalogMissingException("contact_purpose_type", PersonalPurpose);
        return new PersonCatalogIds(status, phone, purpose);
    }

    public static async Task<short> EmailAsync(ScoutDbContext db, CancellationToken cancellationToken) =>
        await db.ContactMechanismTypes.AsNoTracking().Where(x => x.Code == EmailMechanism).Select(x => (short?)x.Id).SingleOrDefaultAsync(cancellationToken)
            ?? throw new CatalogMissingException("contact_mechanism_type", EmailMechanism);
}
