namespace DSC.Application.Interfaces;

public enum InvitationDeliveryOutcome { Sent, Pending, AlreadySent, NotEligible, NotFound }

/// <summary>Resumen sin datos personales de una invitación pendiente.</summary>
public sealed record PendingInvitationDto(long EnrollmentId, int Attempts, string? LastFailureCode, DateTime CreatedAt);

/// <summary>Entrega (y reintento manual) de la invitación de activación de una cuenta pendiente.</summary>
public interface IAccountInvitationDelivery
{
    Task<InvitationDeliveryOutcome> DeliverAsync(long enrollmentId, CancellationToken cancellationToken);

    Task<List<PendingInvitationDto>> ListPendingAsync(CancellationToken cancellationToken);
}
