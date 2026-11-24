namespace DSC.Application.Interfaces;

/// <summary>Emite y canjea invitaciones de activación de cuentas pendientes sin almacenar el secreto del enlace.</summary>
public interface IAccountActivation
{
    /// <summary>Reemplaza la invitación de una cuenta pendiente. Devuelve el secreto solo al emisor, o null si no es elegible.</summary>
    Task<string?> IssueAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Fija la contraseña, activa la cuenta y consume el enlace. True exclusivamente para el canje que activa la cuenta.</summary>
    Task<bool> RedeemAsync(string? token, string? password, CancellationToken cancellationToken);
}
