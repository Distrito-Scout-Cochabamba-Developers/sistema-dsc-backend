using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

/// <summary>
/// Alta mínima de un Rover sin cuenta (#160). Debe usarse dentro de la misma
/// transacción que la inscripción: ninguna operación confirma por sí sola.
/// </summary>
public interface IRoverRegistrationRepository
{
    /// <summary>Serializa altas simultáneas del mismo CI hasta el fin de la transacción ambiente.</summary>
    Task LockCiAsync(string ciNumber, CancellationToken cancellationToken);

    /// <summary>Complementos (nulo = sin complemento) de las personas que ya tienen ese número de CI.</summary>
    Task<IReadOnlyList<string?>> ExtensionsOfCiAsync(string ciNumber, CancellationToken cancellationToken);

    /// <summary>Crea persona activa y, si hay celular, su contacto. No crea cuenta, dirigente ni afiliación.</summary>
    Task<Person> CreateAsync(
        string ciNumber,
        string? extension,
        string firstName,
        string lastName,
        string? phone,
        CancellationToken cancellationToken
    );
}
