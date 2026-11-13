using DSC.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace DSC.UnitTests.Infrastructure;

/// <summary>Comprueba el contrato obligatorio de configuración JWT sin utilizar claves reales.</summary>
public class JwtSettingsTests
{
    /// <summary>Rechaza el arranque cuando falta cualquier configuración de autenticación.</summary>
    /// <param name="missingKey">Clave obligatoria que se omite en el escenario.</param>
    [Theory]
    [InlineData("Jwt:Secret")]
    [InlineData("Jwt:Issuer")]
    [InlineData("Jwt:Audience")]
    public void Load_RequiresEveryAuthenticationSetting(string missingKey)
    {
        var values = ValidValues();
        values.Remove(missingKey);

        Assert.Throws<InvalidOperationException>(() => JwtSettings.Load(Configuration(values)));
    }

    /// <summary>Comprueba que los errores de configuración no reproducen valores sensibles.</summary>
    [Fact]
    public void Load_RejectsShortSecretsWithoutRevealingTheirValue()
    {
        var values = ValidValues();
        values["Jwt:Secret"] = "sensitive-short-value";

        var error = Assert.Throws<InvalidOperationException>(() =>
            JwtSettings.Load(Configuration(values))
        );

        Assert.DoesNotContain(values["Jwt:Secret"]!, error.Message);
    }

    /// <summary>Evita que un despliegue conserve accidentalmente el nombre antiguo del secreto.</summary>
    [Fact]
    public void Load_DoesNotAcceptTheLegacyKeyInsteadOfSecret()
    {
        var values = ValidValues();
        values["Jwt:Key"] = values["Jwt:Secret"];
        values.Remove("Jwt:Secret");

        Assert.Throws<InvalidOperationException>(() => JwtSettings.Load(Configuration(values)));
    }

    /// <summary>Verifica la configuración compartida por emisión y validación de tokens.</summary>
    [Fact]
    public void Load_UsesConfiguredIssuerAudienceAndExpiration()
    {
        var settings = JwtSettings.Load(Configuration(ValidValues()));

        Assert.Equal("test-issuer", settings.Issuer);
        Assert.Equal("test-audience", settings.Audience);
        Assert.Equal(45, settings.ExpiresMinutes);
        Assert.True(settings.CreateSigningKey().KeySize >= 256);
    }

    private static Dictionary<string, string?> ValidValues() =>
        new()
        {
            ["Jwt:Secret"] = Convert.ToBase64String(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(48)
            ),
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:ExpiresMinutes"] = "45",
        };

    private static IConfiguration Configuration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
