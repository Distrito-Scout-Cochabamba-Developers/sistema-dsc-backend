using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DSC.Infrastructure;

/// <summary>Configuración JWT validada y compartida por emisión y validación de tokens.</summary>
/// <remarks>La clave sólo se recibe por configuración externa; nunca se incluye en mensajes de error.</remarks>
public sealed class JwtSettings
{
    private readonly string _secret;

    private JwtSettings(string secret, string issuer, string audience, int expiresMinutes)
    {
        _secret = secret;
        Issuer = issuer;
        Audience = audience;
        ExpiresMinutes = expiresMinutes;
    }

    /// <summary>Emisor que debe coincidir al emitir y validar el token.</summary>
    public string Issuer { get; }

    /// <summary>Audiencia a la que se emiten los tokens.</summary>
    public string Audience { get; }

    /// <summary>Tiempo de vida del token en minutos.</summary>
    public int ExpiresMinutes { get; }

    /// <summary>Lee y valida la configuración obligatoria antes de arrancar la API.</summary>
    /// <param name="configuration">Configuración con Jwt:Secret, Jwt:Issuer y Jwt:Audience.</param>
    /// <returns>Configuración inmutable sin valores secretos predeterminados.</returns>
    /// <exception cref="InvalidOperationException">Falta una clave requerida o el secreto es insuficiente.</exception>
    public static JwtSettings Load(IConfiguration configuration)
    {
        var secret = configuration["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
            throw new InvalidOperationException("Configure Jwt:Secret con al menos 32 bytes.");

        var issuer = configuration["Jwt:Issuer"];
        if (string.IsNullOrWhiteSpace(issuer))
            throw new InvalidOperationException("Configure Jwt:Issuer.");

        var audience = configuration["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Configure Jwt:Audience.");

        var expiresMinutes =
            int.TryParse(configuration["Jwt:ExpiresMinutes"], out var minutes) && minutes > 0
                ? minutes
                : 60;

        return new JwtSettings(secret, issuer.Trim(), audience.Trim(), expiresMinutes);
    }

    /// <summary>Crea la clave simétrica usada para HS256.</summary>
    /// <returns>Clave de al menos 256 bits derivada del secreto externo validado.</returns>
    public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(_secret));
}
