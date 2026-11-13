using System.Text.Json;
using DSC.Api.Controllers;
using DSC.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace DSC.UnitTests.Api;

/// <summary>Comprueba contratos y confidencialidad de los diagnósticos sin conexiones de red.</summary>
public class HealthControllerTests
{
    private const string SyntheticError =
        "SyntheticConfigurationMarker;Password=SyntheticOnly;Host=invalid.test";
    private const string SafeError = "No se pudo completar el diagnóstico de la base de datos.";

    /// <summary>Limita los detalles de excepciones al entorno Development en ambas rutas.</summary>
    /// <param name="environment">Nombre del entorno del host.</param>
    /// <param name="diagnostic">Selecciona el diagnóstico de catálogo.</param>
    [Theory]
    [InlineData("Production", false)]
    [InlineData("Production", true)]
    [InlineData("Staging", false)]
    [InlineData("Staging", true)]
    [InlineData("Custom", false)]
    [InlineData("Custom", true)]
    [InlineData("Development", false)]
    [InlineData("Development", true)]
    public async Task ConnectionException_DisclosesDetailsOnlyInDevelopment(
        string environment,
        bool diagnostic
    )
    {
        await using var context = CreateContext(
            connectionError: new InvalidOperationException(SyntheticError)
        );
        var controller = CreateController(context, environment);
        using var cancellation = new CancellationTokenSource();

        var result = diagnostic
            ? await controller.DiagnosticCheck(cancellation.Token)
            : await controller.CheckHealth(cancellation.Token);

        var json = ReadJson(result, StatusCodes.Status503ServiceUnavailable);
        Assert.Equal(cancellation.Token, context.TestDatabase.LastCancellationToken);
        Assert.Equal(diagnostic ? "Exception" : "Degraded", json.GetProperty("Status").GetString());
        var error = diagnostic
            ? json.GetProperty("Error")
            : json.GetProperty("Database").GetProperty("Error");
        AssertSafeError(json, error, environment);
        Assert.Equal(diagnostic ? 2 : 3, json.EnumerateObject().Count());
        if (!diagnostic)
        {
            Assert.False(json.GetProperty("Database").GetProperty("CanConnect").GetBoolean());
            Assert.Equal(3, json.GetProperty("Database").EnumerateObject().Count());
            Assert.True(json.GetProperty("Timestamp").TryGetDateTime(out _));
        }
    }

    /// <summary>Resguarda también las excepciones posteriores a una conexión exitosa.</summary>
    /// <param name="environment">Nombre del entorno del host.</param>
    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    [InlineData("Custom")]
    [InlineData("Development")]
    public async Task CatalogQueryException_DisclosesDetailsOnlyInDevelopment(string environment)
    {
        await using var context = CreateContext(failCatalog: true);
        var controller = CreateController(context, environment);

        var result = await controller.DiagnosticCheck(CancellationToken.None);

        var json = ReadJson(result, StatusCodes.Status503ServiceUnavailable);
        Assert.True(context.TestDatabase.ConnectionChecked);
        Assert.Equal("Exception", json.GetProperty("Status").GetString());
        Assert.Equal(2, json.EnumerateObject().Count());
        AssertSafeError(json, json.GetProperty("Error"), environment);
    }

    /// <summary>Conserva la respuesta de salud disponible con error nulo y token propagado.</summary>
    [Fact]
    public async Task CheckHealth_Connected_PreservesSuccessfulContract()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);
        using var cancellation = new CancellationTokenSource();

        var result = await controller.CheckHealth(cancellation.Token);

        var json = ReadJson(result, StatusCodes.Status200OK);
        Assert.Equal("Healthy", json.GetProperty("Status").GetString());
        Assert.Equal(3, json.EnumerateObject().Count());
        Assert.True(json.GetProperty("Timestamp").TryGetDateTime(out _));
        var database = json.GetProperty("Database");
        Assert.Equal(3, database.EnumerateObject().Count());
        Assert.True(database.GetProperty("CanConnect").GetBoolean());
        Assert.Equal("SyntheticProvider", database.GetProperty("Provider").GetString());
        Assert.Equal(JsonValueKind.Null, database.GetProperty("Error").ValueKind);
        Assert.Equal(cancellation.Token, context.TestDatabase.LastCancellationToken);
    }

    /// <summary>Conserva los cinco contadores del diagnóstico usando EF InMemory.</summary>
    [Fact]
    public async Task DiagnosticCheck_Connected_PreservesCatalogContract()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);

        var result = await controller.DiagnosticCheck(CancellationToken.None);

        var json = ReadJson(result, StatusCodes.Status200OK);
        Assert.Equal("Connected", json.GetProperty("Status").GetString());
        Assert.Equal("SyntheticProvider", json.GetProperty("Provider").GetString());
        Assert.Equal(3, json.EnumerateObject().Count());
        var summary = json.GetProperty("CatalogSummary");
        Assert.Equal(5, summary.EnumerateObject().Count());
        foreach (var name in new[] { "ModuleTypes", "Branches", "Roles", "ScoutGroups", "Persons" })
            Assert.Equal(0, summary.GetProperty(name).GetInt32());
    }

    /// <summary>Conserva la degradación sin inventar errores cuando CanConnect devuelve falso.</summary>
    /// <param name="diagnostic">Selecciona el diagnóstico de catálogo.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConnectionUnavailable_PreservesExistingContract(bool diagnostic)
    {
        await using var context = CreateContext(canConnect: false, failCatalog: true);
        var controller = CreateController(context);

        var result = diagnostic
            ? await controller.DiagnosticCheck(CancellationToken.None)
            : await controller.CheckHealth(CancellationToken.None);

        var json = ReadJson(result, StatusCodes.Status503ServiceUnavailable);
        Assert.Equal(diagnostic ? "Error" : "Degraded", json.GetProperty("Status").GetString());
        if (diagnostic)
        {
            Assert.Equal(2, json.EnumerateObject().Count());
            Assert.Equal(
                "No se puede conectar a la base de datos.",
                json.GetProperty("Message").GetString()
            );
        }
        else
        {
            Assert.Equal(3, json.EnumerateObject().Count());
            var database = json.GetProperty("Database");
            Assert.False(database.GetProperty("CanConnect").GetBoolean());
            Assert.Equal(JsonValueKind.Null, database.GetProperty("Error").ValueKind);
        }
        Assert.DoesNotContain("SyntheticConfigurationMarker", json.GetRawText());
    }

    /// <summary>Comprueba el cuerpo JSON completo, no sólo la propiedad Error.</summary>
    private static void AssertSafeError(JsonElement json, JsonElement error, string environment)
    {
        if (environment == Environments.Development)
        {
            Assert.Equal(SyntheticError, error.GetString());
            Assert.Contains("SyntheticConfigurationMarker", json.GetRawText());
            return;
        }

        Assert.Equal(SafeError, error.GetString());
        foreach (
            var marker in new[]
            {
                "SyntheticConfigurationMarker",
                "Password=",
                "SyntheticOnly",
                "invalid.test",
                "InvalidOperationException",
            }
        )
            Assert.DoesNotContain(marker, json.GetRawText());
    }

    /// <summary>Serializa la respuesta pública para comprobar el contrato sin servidor HTTP.</summary>
    private static JsonElement ReadJson(IActionResult result, int expectedStatus)
    {
        var response = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(expectedStatus, response.StatusCode);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(response.Value));
        return json.RootElement.Clone();
    }

    private static HealthController CreateController(
        ScoutDbContext context,
        string environment = "Production"
    ) => new(context, new TestEnvironment { EnvironmentName = environment });

    private static TestDbContext CreateContext(
        bool canConnect = true,
        Exception? connectionError = null,
        bool failCatalog = false
    ) =>
        new(
            new DbContextOptionsBuilder<ScoutDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            canConnect,
            connectionError,
            failCatalog
        );

    /// <summary>Contexto aislado que conserva consultas InMemory y permite fallos sintéticos.</summary>
    private sealed class TestDbContext : ScoutDbContext
    {
        private readonly bool failCatalog;

        public TestDbContext(
            DbContextOptions<ScoutDbContext> options,
            bool canConnect,
            Exception? connectionError,
            bool failCatalog
        )
            : base(options)
        {
            this.failCatalog = failCatalog;
            TestDatabase = new TestDatabaseFacade(this, canConnect, connectionError);
        }

        public TestDatabaseFacade TestDatabase { get; }
        public override DatabaseFacade Database => TestDatabase;

        public override DbSet<TEntity> Set<TEntity>()
        {
            if (failCatalog)
                throw new InvalidOperationException(SyntheticError);
            return base.Set<TEntity>();
        }
    }

    /// <summary>Simula conectividad sin usar sockets y registra la propagación del token.</summary>
    private sealed class TestDatabaseFacade(
        DbContext context,
        bool canConnect,
        Exception? connectionError
    ) : DatabaseFacade(context)
    {
        public override string? ProviderName => "SyntheticProvider";
        public CancellationToken LastCancellationToken { get; private set; }
        public bool ConnectionChecked { get; private set; }

        public override Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            ConnectionChecked = true;
            LastCancellationToken = cancellationToken;
            return connectionError is null
                ? Task.FromResult(canConnect)
                : Task.FromException<bool>(connectionError);
        }
    }

    /// <summary>Entorno del host controlado por cada prueba sin leer configuración real.</summary>
    private sealed class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "HealthControllerTests";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
