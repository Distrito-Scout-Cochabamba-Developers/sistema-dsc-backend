using DSC.Api.Configuration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DSC.UnitTests.Api;

/// <summary>Valida la lista explícita de orígenes y las restricciones de la política CORS.</summary>
public class CorsConfigurationTests
{
    /// <summary>Impide un despliegue sin lista explícita de orígenes autorizados.</summary>
    [Fact]
    public void Configure_RequiresOriginsOutsideDevelopment()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddAngularCors(Configuration(), isDevelopment: false)
        );
    }

    /// <summary>Rechaza orígenes inseguros o que no representan un origen HTTPS exacto.</summary>
    /// <param name="origin">Origen no permitido en el escenario.</param>
    [Theory]
    [InlineData("*")]
    [InlineData("https://*.example.test")]
    [InlineData("https://app.example.test/path")]
    [InlineData("https://app.example.test/")]
    [InlineData("http://app.example.test")]
    public void Configure_RejectsUnsafeProductionOrigins(string origin)
    {
        Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddAngularCors(Configuration(origin), false)
        );
    }

    /// <summary>Comprueba que la política Bearer sólo acepta el origen configurado y no cookies.</summary>
    [Fact]
    public void Configure_AllowsOnlyTheExplicitAngularOriginWithoutCookies()
    {
        var services = new ServiceCollection();
        services.AddAngularCors(Configuration("https://app.example.test"), false);
        using var provider = services.BuildServiceProvider();
        var policy = provider
            .GetRequiredService<IOptions<CorsOptions>>()
            .Value.GetPolicy(CorsConfiguration.AngularPolicy);

        Assert.NotNull(policy);
        Assert.True(policy.IsOriginAllowed("https://app.example.test"));
        Assert.False(policy.IsOriginAllowed("https://evil.example.test"));
        Assert.False(policy.SupportsCredentials);
        Assert.Contains("Authorization", policy.Headers);
        Assert.Contains("POST", policy.Methods);
    }

    /// <summary>Limita los valores predeterminados locales al entorno de desarrollo.</summary>
    [Fact]
    public void Configure_ProvidesOnlyLoopbackDefaultsInDevelopment()
    {
        var services = new ServiceCollection();
        services.AddAngularCors(Configuration(), true);
        using var provider = services.BuildServiceProvider();
        var policy = provider
            .GetRequiredService<IOptions<CorsOptions>>()
            .Value.GetPolicy(CorsConfiguration.AngularPolicy);

        Assert.NotNull(policy);
        Assert.True(policy.IsOriginAllowed("http://localhost:4200"));
        Assert.True(policy.IsOriginAllowed("http://127.0.0.1:4200"));
        Assert.False(policy.IsOriginAllowed("https://evil.example.test"));
    }

    private static IConfiguration Configuration(string? origin = null)
    {
        var values = new Dictionary<string, string?>();
        if (origin is not null)
            values["Cors:AllowedOrigins:0"] = origin;

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
