using DSC.Application.Interfaces;
using DSC.Domain.Entities;
using DSC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DSC.Infrastructure;

public enum InvitationDeliveryOutcome { Sent, Pending, AlreadySent, NotEligible, NotFound }

/// <summary>Resumen sin datos personales de una invitación pendiente.</summary>
public sealed record PendingInvitationDto(long EnrollmentId, int Attempts, string? LastFailureCode, DateTime CreatedAt);

/// <summary>
/// Entrega la invitación de una cuenta pendiente. Serializa por invitación con un bloqueo de fila,
/// de modo que reintentos simultáneos no envían dos veces. La entrega es "al menos una vez": si el
/// envío tiene éxito pero la confirmación en base de datos falla, la invitación sigue PENDING y un
/// reintento manual enviará otro enlace (el anterior queda invalidado). Nunca registra correo, enlace ni token.
/// </summary>
public sealed class AccountInvitationDelivery(
    ScoutDbContext db,
    AccountActivation activation,
    IAccountInvitationMailer mailer,
    IConfiguration configuration,
    TimeProvider clock,
    ILogger<AccountInvitationDelivery> logger
)
{
    public async Task<InvitationDeliveryOutcome> DeliverAsync(long enrollmentId, CancellationToken cancellationToken)
    {
        var attempted = false;
        string? failure = null;
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
            var locked = await db.Database
                .SqlQuery<long>($"SELECT id AS \"Value\" FROM account_invitation WHERE enrollment_id = {enrollmentId} FOR UPDATE")
                .ToListAsync(cancellationToken);
            if (locked.Count == 0) return InvitationDeliveryOutcome.NotFound;
            var invitation = await db.AccountInvitations.Include(i => i.User).SingleAsync(i => i.Id == locked[0], cancellationToken);
            if (invitation.Status == AccountInvitation.Sent) return InvitationDeliveryOutcome.AlreadySent;
            if (invitation.Status == AccountInvitation.Closed) return InvitationDeliveryOutcome.NotEligible;
            var now = clock.GetUtcNow().UtcDateTime;

            // Un reintento de la estrategia de EF no debe reenviar el correo ya intentado.
            if (!attempted)
            {
                attempted = true;
                var origin = ActivationOrigin();
                if (origin is null)
                    failure = "InvalidActivationOrigin";
                else
                {
                    var token = await activation.IssueAsync(invitation.UserId, cancellationToken);
                    if (token is null)
                    {
                        invitation.Status = AccountInvitation.Closed;
                        invitation.UpdatedAt = now;
                        await db.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                        return InvitationDeliveryOutcome.NotEligible;
                    }
                    try
                    {
                        await mailer.SendAsync(new(invitation.User.Email, invitation.User.Username, $"{origin}/auth/activar#token={token}"), cancellationToken);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        failure = ex.GetType().Name;
                    }
                }
                invitation.Attempts++;
            }

            invitation.UpdatedAt = now;
            if (failure is null)
            {
                invitation.Status = AccountInvitation.Sent;
                invitation.SentAt = now;
                invitation.LastFailureCode = null;
            }
            else
                invitation.LastFailureCode = failure[..Math.Min(failure.Length, 64)];
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            if (failure is not null)
                logger.LogWarning("Falló la entrega de la invitación {InvitationId}: {FailureCode}", invitation.Id, failure);
            return failure is null ? InvitationDeliveryOutcome.Sent : InvitationDeliveryOutcome.Pending;
        });
    }

    public Task<List<PendingInvitationDto>> ListPendingAsync(CancellationToken cancellationToken) =>
        db.AccountInvitations.AsNoTracking()
            .Where(i => i.Status == AccountInvitation.Pending)
            .OrderBy(i => i.Id)
            .Select(i => new PendingInvitationDto(i.EnrollmentId, i.Attempts, i.LastFailureCode, i.CreatedAt))
            .ToListAsync(cancellationToken);

    /// <summary>Origen HTTPS configurado en el servidor; nunca derivado de la solicitud.</summary>
    private string? ActivationOrigin()
    {
        var configured = configuration["Activation:FrontendOrigin"];
        return Uri.TryCreate(configured, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps
            && string.IsNullOrEmpty(uri.UserInfo)
            && uri.AbsolutePath == "/"
            && uri.Query.Length == 0
            && uri.Fragment.Length == 0
            ? uri.GetLeftPart(UriPartial.Authority)
            : null;
    }
}
