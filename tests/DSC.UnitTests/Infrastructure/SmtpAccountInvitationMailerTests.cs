using System.Net;
using System.Net.Sockets;
using System.Text;
using DSC.Application.Interfaces;
using DSC.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace DSC.UnitTests.Infrastructure;

/// <summary>Adaptador SMTP contra un servidor local mínimo (loopback, sin credenciales ni red externa).</summary>
public class SmtpAccountInvitationMailerTests
{
    private static IConfiguration Config(int? port, bool withHost = true) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Smtp:Host"] = withHost ? "127.0.0.1" : null,
            ["Smtp:Port"] = port?.ToString(),
            ["Smtp:EnableSsl"] = "false",
            ["Smtp:From"] = "no-responder@example.test",
        }).Build();

    [Fact]
    public async Task SendsUsernameAndLinkInPlainTextWithoutPassword()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var received = Serve(listener);

        await new SmtpAccountInvitationMailer(Config(port)).SendAsync(
            new("ana@example.test", "dsc-abc234", "https://dsc.example.test/auth/activar#token=ficticio"), CancellationToken.None);

        var data = await received;
        Assert.Contains("RCPT TO:<ana@example.test>", data);
        Assert.Contains("dsc-abc234", data);
        Assert.Contains("https://dsc.example.test/auth/activar#token=ficticio", data);
        Assert.DoesNotContain("text/html", data);
        Assert.DoesNotContain("contraseña:", data, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MissingConfigurationFailsWithoutSending() =>
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new SmtpAccountInvitationMailer(Config(25, withHost: false)).SendAsync(new("a@example.test", "u", "https://x.test"), CancellationToken.None));

    [Fact]
    public async Task UnresponsiveServerFailsWithinConfiguredTimeoutInsteadOfBlocking()
    {
        // Servidor que acepta la conexión pero nunca saluda: sin tope, la solicitud de alta quedaría colgada.
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Smtp:Host"] = "127.0.0.1",
            ["Smtp:Port"] = port.ToString(),
            ["Smtp:EnableSsl"] = "false",
            ["Smtp:From"] = "no-responder@example.test",
            ["Smtp:TimeoutSeconds"] = "1",
        }).Build();

        var started = System.Diagnostics.Stopwatch.StartNew();
        await Assert.ThrowsAnyAsync<Exception>(() =>
            new SmtpAccountInvitationMailer(config).SendAsync(new("a@example.test", "u", "https://x.test"), CancellationToken.None));

        Assert.True(started.Elapsed < TimeSpan.FromSeconds(6), $"Tardó {started.Elapsed}.");
    }

    private static async Task<string> Serve(TcpListener listener)
    {
        using var client = await listener.AcceptTcpClientAsync();
        using var stream = client.GetStream();
        var log = new StringBuilder();
        async Task Say(string line) => await stream.WriteAsync(Encoding.ASCII.GetBytes(line + "\r\n"));
        await Say("220 fake");
        var buffer = new byte[8192];
        var inData = false;
        while (true)
        {
            var n = await stream.ReadAsync(buffer);
            if (n == 0) break;
            var text = Encoding.UTF8.GetString(buffer, 0, n);
            log.Append(text);
            if (inData)
            {
                if (text.EndsWith("\r\n.\r\n")) { inData = false; await Say("250 ok"); }
                continue;
            }
            foreach (var line in text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("EHLO", StringComparison.OrdinalIgnoreCase) || line.StartsWith("HELO", StringComparison.OrdinalIgnoreCase)) await Say("250 fake");
                else if (line.StartsWith("DATA", StringComparison.OrdinalIgnoreCase)) { await Say("354 go"); inData = true; }
                else if (line.StartsWith("QUIT", StringComparison.OrdinalIgnoreCase)) { await Say("221 bye"); return DecodeBody(log.ToString()); }
                else await Say("250 ok");
            }
        }
        return DecodeBody(log.ToString());
    }

    // Cuerpo en UTF-8 puede venir en base64; se decodifica para poder inspeccionarlo.
    private static string DecodeBody(string raw)
    {
        var idx = raw.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        if (idx < 0 || !raw.Contains("Content-Transfer-Encoding: base64", StringComparison.OrdinalIgnoreCase)) return raw;
        var b64 = raw[(idx + 4)..].Split("\r\n.\r\n")[0].Replace("\r\n", "");
        try { return raw + "\n" + Encoding.UTF8.GetString(Convert.FromBase64String(b64)); } catch (FormatException) { return raw; }
    }
}
