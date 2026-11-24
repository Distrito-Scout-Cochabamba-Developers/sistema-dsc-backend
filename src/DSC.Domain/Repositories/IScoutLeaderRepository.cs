using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

/// <summary>
/// Contrato de persistencia y consulta para personas y dirigentes scouts.
/// </summary>
public interface IScoutLeaderRepository
{
    Task<Person?> GetByCiNumberAsync(string ciNumber, CancellationToken cancellationToken);
    Task<IReadOnlyList<Person>> FindByCiAsync(
        string ciNumber,
        string? extension,
        CancellationToken cancellationToken
    );
    /// <summary>Complementos (nulo = sin complemento) de las personas que ya tienen ese número de CI.</summary>
    Task<IReadOnlyList<string?>> ExtensionsOfCiAsync(string ciNumber, CancellationToken cancellationToken);
    Task<Person?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task AddAsync(Person person, CancellationToken cancellationToken);
    Task UpdateAsync(Person person, CancellationToken cancellationToken);
    /// <summary>Serializa altas por número de CI; con <paramref name="required"/> falso se omite en proveedores no PostgreSQL.</summary>
    Task LockCiAsync(string ciNumber, CancellationToken cancellationToken, bool required = true);
    /// <summary>Resuelve por código los catálogos de alta de persona; lanza <see cref="DSC.Domain.Exceptions.CatalogMissingException"/> si faltan.</summary>
    Task<PersonCatalogIds> GetPersonCatalogIdsAsync(CancellationToken cancellationToken);
    Task<Person> CreateSecretariatLeaderAsync(string ci, string? extension, string firstName, string lastName, string phone, string email, CancellationToken cancellationToken);
}
