using System.Net;
using System.Net.Mail;
using DSC.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DSC.Infrastructure;

/// <summary>
/// Transporte SMTP. Configuración (variables de entorno / .env fuera de Git):
/// Smtp__Host, Smtp__Port (587), Smtp__EnableSsl (true), Smtp__Username, Smtp__Password, Smtp__From.
/// Texto plano sin enlaces de seguimiento ni contraseñas.
/// </summary>
public sealed class SmtpAccountInvitationMailer(IConfiguration configuration) : IAccountInvitationMailer
{
    private const int DefaultTimeoutSeconds = 5;

    public async Task SendAsync(AccountInvitationMessage message, CancellationToken cancellationToken)
    {
        var host = configuration["Smtp:Host"];
        var from = configuration["Smtp:From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("SMTP no configurado.");
        var port = int.TryParse(configuration["Smtp:Port"], out var p) ? p : 587;
        var ssl = !bool.TryParse(configuration["Smtp:EnableSsl"], out var s) || s;

        var timeoutSeconds = int.TryParse(configuration["Smtp:TimeoutSeconds"], out var t) ? Math.Clamp(t, 1, 30) : DefaultTimeoutSeconds;
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        using var client = new SmtpClient(host, port) { EnableSsl = ssl, Timeout = (int)timeout.TotalMilliseconds };
        var user = configuration["Smtp:Username"];
        if (!string.IsNullOrEmpty(user))
            client.Credentials = new NetworkCredential(user, configuration["Smtp:Password"]);
        using var mail = new MailMessage(from, message.To)
        {
            Subject = "Activa tu cuenta del Distrito Scout Cochabamba",
            IsBodyHtml = false,
            Body = string.Join(
                Environment.NewLine,
                "Se registró tu inscripción y se creó tu cuenta.",
                "",
                $"Tu nombre de usuario: {message.Username}",
                "",
                "Para elegir tu contraseña abre este enlace (vence en 24 horas y solo funciona una vez):",
                message.ActivationUrl,
                "",
                "Si no esperabas este correo, ignóralo."
            ),
        };
        // SmtpClient.Timeout no cubre todas las fases asíncronas: el tope se impone también con cancelación.
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limit.CancelAfter(timeout);
        try
        {
            await client.SendMailAsync(mail, limit.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("Se agotó el tiempo de espera del envío SMTP.");
        }
    }
}
