using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

/// <summary>
/// Contrato de persistencia y consulta para usuarios.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Busca un usuario por email o nombre de usuario, incluyendo estado y roles.
    /// </summary>
    Task<User?> GetByIdentifierWithRolesAsync(
        string identifier,
        CancellationToken cancellationToken
    );
}
