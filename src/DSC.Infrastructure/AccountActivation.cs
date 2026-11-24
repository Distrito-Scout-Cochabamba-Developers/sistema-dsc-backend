using System.Security.Cryptography;
using System.Text;
using DSC.Application.Interfaces;
using DSC.Domain.Entities;
using DSC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure;

/// <summary>Emite y canjea invitaciones de cuentas pendientes sin almacenar el secreto del enlace.</summary>
public sealed class AccountActivation(
    ScoutDbContext db,
    IPasswordHasher passwords,
    TimeProvider clock
)
{
    /// <summary>Reemplaza la invitación de una cuenta pendiente ya persistida. Devuelve el secreto solo al emisor.</summary>
    /// <param name="userId">Cuenta pendiente sin contraseña válida.</param>
    /// <param name="cancellationToken">Cancelación de la operación.</param>
    /// <returns>Token para un enlace de 24 horas, o null si la cuenta no es elegible.</returns>
    public async Task<string?> IssueAsync(
        long userId,
        CancellationToken cancellationToken = default
    )
    {
        var token = Convert
            .ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        var digest = HashToken(token);
        var expiresAt = clock.GetUtcNow().UtcDateTime.AddHours(24);
        var updated = await PendingUsers()
            .Where(user => user.Id == userId)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(user => user.ActivationTokenHash, digest)
                        .SetProperty(user => user.ActivationExpiresAt, expiresAt),
                cancellationToken
            );
        return updated == 1 ? token : null;
    }

    /// <summary>Fija la contraseña, activa la cuenta y consume el enlace en un solo UPDATE condicional.</summary>
    /// <param name="token">Secreto recibido por la persona invitada.</param>
    /// <param name="password">Nueva contraseña de 12 caracteres o más, hasta 72 bytes UTF-8.</param>
    /// <param name="cancellationToken">Cancelación de la operación.</param>
    /// <returns>True exclusivamente para el canje que activa la cuenta.</returns>
    public async Task<bool> RedeemAsync(
        string? token,
        string? password,
        CancellationToken cancellationToken
    )
    {
        if (
            token is not { Length: 43 }
            || token.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_')
            || string.IsNullOrWhiteSpace(password)
            || Encoding.UTF8.GetByteCount(password) > 72
            || password.EnumerateRunes().Count() < 12
        )
            return false;

        var digest = HashToken(token);
        var now = clock.GetUtcNow().UtcDateTime;
        var matching = PendingUsers().Where(user => user.ActivationTokenHash == digest);
        if (!await matching.AnyAsync(user => user.ActivationExpiresAt > now, cancellationToken))
            return false;

        var activeId = await db
            .UserStatuses.Where(status => status.Code == "ACTIVE")
            .Select(status => status.Id)
            .SingleAsync(cancellationToken);
        var passwordHash = passwords.Hash(password);
        now = clock.GetUtcNow().UtcDateTime;
        var updated = await matching
            .Where(user => user.ActivationExpiresAt > now)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(user => user.Password, passwordHash)
                        .SetProperty(user => user.UserStatusId, activeId)
                        .SetProperty(user => user.IsEmailVerified, true)
                        .SetProperty(user => user.ActivationTokenHash, (string?)null)
                        .SetProperty(user => user.ActivationExpiresAt, (DateTime?)null)
                        .SetProperty(user => user.UpdatedAt, now),
                cancellationToken
            );
        return updated == 1;
    }

    private IQueryable<User> PendingUsers() =>
        db.Users.Where(user =>
            user.Password == ""
            && db.UserStatuses.Any(status =>
                status.Id == user.UserStatusId && status.Code == "PENDING_VERIFICATION"
            )
        );

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(token)));
}
