namespace DSC.Application.Interfaces;

/// <summary>Contenido de la invitación. Nunca incluye contraseña.</summary>
public sealed record AccountInvitationMessage(string To, string Username, string ActivationUrl);

/// <summary>Transporte del correo de invitación (SMTP en producción, ficticio en pruebas).</summary>
public interface IAccountInvitationMailer
{
    /// <summary>Entrega al menos una vez; un fallo se señala con una excepción.</summary>
    Task SendAsync(AccountInvitationMessage message, CancellationToken cancellationToken);
}
