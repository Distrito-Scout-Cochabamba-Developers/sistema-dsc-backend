namespace DSC.Api.Configuration;

/// <summary>Registra la política CORS explícita del frontend Angular.</summary>
public static class CorsConfiguration
{
    /// <summary>Nombre compartido por el registro de servicios y el middleware.</summary>
    public const string AngularPolicy = "AngularFrontend";

    /// <summary>Configura orígenes exactos, métodos HTTP y cabeceras del cliente Bearer.</summary>
    /// <param name="services">Contenedor de servicios de la API.</param>
    /// <param name="configuration">Configuración Cors:AllowedOrigins, incluida su inyección por entorno.</param>
    /// <param name="isDevelopment">Permite los orígenes localhost sólo como valores locales predeterminados.</param>
    /// <returns>Contenedor con la política CORS registrada.</returns>
    /// <exception cref="InvalidOperationException">Faltan orígenes de despliegue o alguno es inseguro.</exception>
    public static IServiceCollection AddAngularCors(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment
    )
    {
        var origins = configuration
            .GetSection("Cors:AllowedOrigins")
            .GetChildren()
            .Select(item => item.Value?.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (origins.Length == 0)
        {
            if (!isDevelopment)
                throw new InvalidOperationException(
                    "Configure Cors:AllowedOrigins para el frontend."
                );

            origins = ["http://localhost:4200", "http://127.0.0.1:4200"];
        }

        foreach (var origin in origins)
        {
            if (
                origin.Contains('*')
                || origin.EndsWith('/')
                || !Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                || (
                    uri.Scheme != Uri.UriSchemeHttps
                    && !(isDevelopment && uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback)
                )
                || uri.AbsolutePath != "/"
                || uri.UserInfo.Length != 0
                || uri.Query.Length != 0
                || uri.Fragment.Length != 0
            )
                throw new InvalidOperationException(
                    "Cors:AllowedOrigins requiere orígenes exactos HTTPS sin ruta ni comodines."
                );
        }

        services.AddCors(options =>
            options.AddPolicy(
                AngularPolicy,
                policy =>
                    policy
                        .WithOrigins(origins)
                        .WithHeaders("Authorization", "Content-Type", "Accept")
                        .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
            )
        );

        // La sesión usa Bearer; no se habilitan cookies ni credenciales CORS.
        return services;
    }
}
