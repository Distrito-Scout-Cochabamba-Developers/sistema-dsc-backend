using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

/// <summary>
/// Contrato de persistencia y consulta para usuarios.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetByUsernameWithRolesAsync(string username, CancellationToken cancellationToken);
}
