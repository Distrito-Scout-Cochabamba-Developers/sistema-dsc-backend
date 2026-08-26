using DSC.Domain.Entities;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de EF Core para gestión de Usuarios
/// </summary>
public class UserRepository(ScoutDbContext dbContext) : IUserRepository
{
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

    public async Task<User?> GetByEmailWithRolesAsync(
        string email,
        CancellationToken cancellationToken
    )
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await dbContext
            .Users.Include(user => user.UserStatus)
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(
                user => user.Email.ToLower() == normalizedEmail,
                cancellationToken
            );
    }

    public async Task<User?> GetByUsernameWithRolesAsync(
        string username,
        CancellationToken cancellationToken
    )
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();

        return await dbContext
            .Users.Include(user => user.UserStatus)
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .FirstOrDefaultAsync(
                user => user.Username.ToLower() == normalizedUsername,
                cancellationToken
            );
    }
}
