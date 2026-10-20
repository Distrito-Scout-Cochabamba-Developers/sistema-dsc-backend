using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de EF Core para gestión de Usuarios
/// </summary>
public class UserRepository(ScoutDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdentifierWithRolesAsync(
        string identifier,
        CancellationToken cancellationToken
    )
    {
        var normalized = identifier.Trim().ToLowerInvariant();

        return await dbContext
            .Users.AsNoTracking()
            .Include(user => user.UserStatus)
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(
                user =>
                    user.Email.ToLower() == normalized
                    || (user.Username != null && user.Username.ToLower() == normalized),
                cancellationToken
            );
    }
}
