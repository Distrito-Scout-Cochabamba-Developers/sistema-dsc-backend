using System.Security.Cryptography;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence.Repositories;

public class AccountInvitationRepository(ScoutDbContext db) : IAccountInvitationRepository
{
    private const string Alphabet = "abcdefghijkmnpqrstuvwxyz23456789";

    public async Task CreatePendingAsync(long personId, long enrollmentId, string email, CancellationToken cancellationToken)
    {
        var normalized = email.Trim();
        if (await db.Users.AnyAsync(u => u.Email.ToLower() == normalized.ToLower(), cancellationToken))
            throw new EmailInUseException();
        var pendingId = await db.UserStatuses.Where(s => s.Code == "PENDING_VERIFICATION").Select(s => s.Id).SingleAsync(cancellationToken);
        // Usuario no derivado de datos personales: 12 caracteres aleatorios (~60 bits).
        var chars = new char[12];
        for (var i = 0; i < chars.Length; i++) chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        var user = new User { PersonId = personId, Username = "dsc-" + new string(chars), Email = normalized, Password = string.Empty, UserStatusId = pendingId, IsEmailVerified = false };
        db.AccountInvitations.Add(new AccountInvitation { User = user, EnrollmentId = enrollmentId });
        await db.SaveChangesAsync(cancellationToken);
    }
}
