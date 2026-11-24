using DSC.Application.Interfaces;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace DSC.Infrastructure;

/// <summary>
/// Transporte SMTP con MailKit. Configuración (variables de entorno / .env fuera de Git):
/// Smtp__Host, Smtp__Port (587), Smtp__EnableSsl (true), Smtp__Username, Smtp__Password, Smtp__From, Smtp__TimeoutSeconds (5).
/// Con EnableSsl=true: puerto 465 usa TLS implícito; cualquier otro puerto exige STARTTLS (sin STARTTLS falla, no envía en claro).
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
        var security = !ssl
            ? SecureSocketOptions.None
            : port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

        var timeoutSeconds = int.TryParse(configuration["Smtp:TimeoutSeconds"], out var t) ? Math.Clamp(t, 1, 30) : DefaultTimeoutSeconds;
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);

        var mail = new MimeMessage();
        mail.From.Add(MailboxAddress.Parse(from));
        mail.To.Add(MailboxAddress.Parse(message.To));
        mail.Subject = "Activa tu cuenta del Distrito Scout Cochabamba";
        mail.Body = new TextPart("plain")
        {
            Text = string.Join(
                "\r\n",
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

        // El timeout propio de MailKit cubre las lecturas/escrituras; el token acota además el total.
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limit.CancelAfter(timeout);
        using var client = new SmtpClient { Timeout = (int)timeout.TotalMilliseconds };
        try
        {
            await client.ConnectAsync(host, port, security, limit.Token);
            var user = configuration["Smtp:Username"];
            if (!string.IsNullOrEmpty(user))
                await client.AuthenticateAsync(user, configuration["Smtp:Password"] ?? string.Empty, limit.Token);
            await client.SendAsync(mail, limit.Token);
            await client.DisconnectAsync(true, CancellationToken.None);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("Se agotó el tiempo de espera del envío SMTP.");
        }
    }
}
