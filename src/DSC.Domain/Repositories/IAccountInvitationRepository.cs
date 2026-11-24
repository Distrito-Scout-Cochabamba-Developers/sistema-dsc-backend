using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

public interface IAccountInvitationRepository
{
    /// <summary>
    /// Crea la cuenta pendiente (sin contraseña) y su invitación PENDING en la transacción actual.
    /// Lanza <see cref="Exceptions.EmailInUseException"/> si el correo ya pertenece a otra cuenta.
    /// </summary>
    Task CreatePendingAsync(long personId, long enrollmentId, string email, CancellationToken cancellationToken);
}
