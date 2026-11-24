using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using DSC.Api.Controllers;
using DSC.Api.Conventions;
using DSC.Api.Middlewares;
using DSC.Application;
using DSC.Application.DTOs;
using DSC.Application.Queries;
using DSC.Domain.Entities;
using DSC.Infrastructure;
using DSC.Infrastructure.Persistence;
using DSC.Infrastructure.Persistence.Repositories;
using DSC.UnitTests.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace DSC.UnitTests.Api;

public class SecretariatParticipantLookupHttpTests : IAsyncLifetime
{
    private const string Path = "/api/v1/secretariat/participants/lookup";
    private static readonly SymmetricSecurityKey Key = new(
        Encoding.UTF8.GetBytes("test-only-key-not-a-production-secret-1234567890")
    );
    private WebApplication app = null!;
    private HttpClient client = null!;
    private readonly LookupTestLogs logs = new();

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" }
        );
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(logs);
        builder
            .Services.AddControllers(options =>
                options.Conventions.Add(new GlobalRoutePrefixConvention(new RouteAttribute("api")))
            )
            .AddApplicationPart(typeof(SecretariatParticipantsController).Assembly);
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure();
        builder.Services.RemoveAll<DbContextOptions<ScoutDbContext>>();
        builder.Services.RemoveAll<ScoutDbContext>();
        var databaseName = Guid.NewGuid().ToString();
        builder.Services.AddDbContext<ScoutDbContext>(options =>
            options.UseInMemoryDatabase(databaseName)
        );
        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = Key,
                    ValidateIssuer = true,
                    ValidIssuer = "test",
                    ValidateAudience = true,
                    ValidAudience = "test",
                    ValidateLifetime = true,
                    RoleClaimType = "roles",
                    ClockSkew = TimeSpan.Zero,
                };
            });
        builder.Services.AddAuthorization();
        app = builder.Build();
        app.UseMiddleware<SecretariatLookupPrivacyMiddleware>();
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ScoutDbContext>();
            db.Persons.AddRange(
                new Person
                {
                    Id = 1,
                    IdCard = "5556667",
                    IdCardExtension = "cb",
                    FirstName = "Carlos",
                    MiddleName = "Andrés",
                    FirstLastName = "Montaño",
                    SecondLastName = "Pérez",
                    PersonStatusId = 1,
                    IsProfileComplete = true,
                    User = new User
                    {
                        Username = "carlos",
                        Email = "carlos@example.com",
                        Password = "hash",
                    },
                    ScoutLeader = new ScoutLeader(),
                    ContactMechanisms =
                    [
                        new PersonContactMechanism
                        {
                            ContactMechanismTypeId = 1,
                            ContactValue = "carlos@example.com",
                            IsPrimary = true,
                        },
                        new PersonContactMechanism
                        {
                            ContactMechanismTypeId = 2,
                            ContactValue = "71234567",
                        },
                        new PersonContactMechanism
                        {
                            ContactMechanismTypeId = 2,
                            ContactValue = "70000000",
                            IsPrimary = true,
                            ValidUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                        },
                    ],
                },
                new Person
                {
                    Id = 2,
                    IdCard = "5556667",
                    IdCardExtension = "LP",
                    FirstName = "María",
                    FirstLastName = "Paz",
                    PersonStatusId = 1,
                },
                new Person
                {
                    Id = 3,
                    IdCard = "7778889",
                    FirstName = "Rover",
                    FirstLastName = "Scout",
                    PersonStatusId = 1,
                },
                new Person
                {
                    Id = 4,
                    IdCard = "6667778",
                    IdCardExtension = "cb",
                    FirstName = "Prueba",
                    FirstLastName = "Uno",
                    PersonStatusId = 1,
                },
                new Person
                {
                    Id = 5,
                    IdCard = "6667778",
                    IdCardExtension = "CB",
                    FirstName = "Prueba",
                    FirstLastName = "Dos",
                    PersonStatusId = 1,
                }
            );
            await db.SaveChangesAsync();
        }

        await app.StartAsync();
        var address = app
            .Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.Single();
        client = new HttpClient { BaseAddress = new Uri(address) };
    }

    private void Authenticate(string role)
    {
        var token = new JwtSecurityToken(
            "test",
            "test",
            [new Claim("roles", role)],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(Key, SecurityAlgorithms.HmacSha256)
        );
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            new JwtSecurityTokenHandler().WriteToken(token)
        );
    }

    [Fact]
    public async Task Lookup_RequiresSecretaryRole()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await LookupAsync("5556667")).StatusCode);
        Authenticate("SCOUT_LEADER");
        Assert.Equal(HttpStatusCode.Forbidden, (await LookupAsync("5556667")).StatusCode);
        Authenticate("ADMIN");
        Assert.Equal(HttpStatusCode.Forbidden, (await LookupAsync("5556667")).StatusCode);
    }

    [Fact]
    public async Task Lookup_AmbiguousCiRequiresExtension_AndReturnsOnlyExactMatch()
    {
        Authenticate("DISTRICT_SECRETARY");
        var ambiguous = await LookupAsync("5556667");
        Assert.Equal(HttpStatusCode.Conflict, ambiguous.StatusCode);
        Assert.Contains("urn:dsc:secretariat:extension-required", await ambiguous.Content.ReadAsStringAsync());
        var response = await LookupAsync("5556667", "cb");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
        var person = (await response.Content.ReadFromJsonAsync<SecretariatParticipantDto>())!;
        Assert.Equal(1, person.PersonId);
        Assert.Equal("Carlos Andrés", person.FirstName);
        Assert.Equal("Montaño Pérez", person.LastName);
        Assert.Equal("cb", person.Extension);
        Assert.Equal("71234567", person.Phone);
        Assert.True(person.HasAccount);
        Assert.True(person.IsScoutLeader);
        Assert.True(person.IsProfileComplete);
        // El CI existe bajo otros complementos: se pide el complemento en lugar de un "no encontrado" ciego.
        var otherComplement = await LookupAsync("5556667", "SC");
        Assert.Equal(HttpStatusCode.Conflict, otherComplement.StatusCode);
        Assert.Contains("urn:dsc:secretariat:extension-required", await otherComplement.Content.ReadAsStringAsync());
        Assert.DoesNotContain("personId", await otherComplement.Content.ReadAsStringAsync());
        Assert.DoesNotContain(
            logs.Messages,
            line => line.Contains("5556667") || line.Contains("71234567")
        );
    }

    [Fact]
    public async Task Lookup_DoesNotReturnPersonWhenExtensionIsAlsoAmbiguous()
    {
        Authenticate("DISTRICT_SECRETARY");
        var response = await LookupAsync("6667778", "CB");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.DoesNotContain("personId", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Lookup_RoverWithoutAccount_DoesNotCreateOrModifyRecords()
    {
        Authenticate("DISTRICT_SECRETARY");
        var response = await LookupAsync("7778889");
        response.EnsureSuccessStatusCode();
        var person = (await response.Content.ReadFromJsonAsync<SecretariatParticipantDto>())!;
        Assert.Equal(3, person.PersonId);
        Assert.False(person.HasAccount);
        Assert.False(person.IsScoutLeader);
        Assert.Equal(string.Empty, person.Phone);
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ScoutDbContext>();
        Assert.Equal(5, await db.Persons.CountAsync());
        Assert.Empty(await db.TrainingModuleEnrollments.ToListAsync());
    }

    [Theory]
    [InlineData(null, null, HttpStatusCode.BadRequest)]
    [InlineData("test-ci-sentinel", null, HttpStatusCode.BadRequest)]
    [InlineData("5556667", "test-extension-sentinel", HttpStatusCode.BadRequest)]
    [InlineData("9999999", null, HttpStatusCode.NotFound)]
    public async Task Lookup_InvalidOrMissingCi_ReturnsExpectedStatus(
        string? ci,
        string? extension,
        HttpStatusCode expected
    )
    {
        Authenticate("DISTRICT_SECRETARY");
        var response = await LookupAsync(ci, extension);
        Assert.Equal(expected, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("sentinel", body);
        Assert.DoesNotContain(logs.Messages, line => line.Contains("sentinel"));
        if (expected == HttpStatusCode.NotFound)
            Assert.Contains("urn:dsc:secretariat:participant-not-found", body);
    }

    [Fact]
    public async Task Lookup_DoesNotAcceptGet()
    {
        Authenticate("DISTRICT_SECRETARY");
        var response = await client.GetAsync(Path);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
    }

    private async Task<HttpResponseMessage> LookupAsync(string? ci, string? extension = null)
    {
        var response = await client.PostAsJsonAsync(Path, new { ci, extension });
        Assert.True(response.Headers.CacheControl?.NoStore);
        return response;
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private sealed class LookupTestLogs : ILoggerProvider, ILogger
    {
        public ConcurrentQueue<string> Messages { get; } = new();

        public ILogger CreateLogger(string categoryName) => this;

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel level,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        ) => Messages.Enqueue($"{formatter(state, exception)} {exception}");

        public void Dispose() { }
    }
}

[Collection("Training requirements PostgreSQL")]
public class SecretariatParticipantLookupPostgresTests(RequirementsPostgres postgres)
{
    [Fact]
    public async Task Lookup_UsesMappedRelationshipsAndCaseInsensitiveExtension()
    {
        await using var db = await postgres.CreateDatabase(seed: false);
        await db.Database.OpenConnectionAsync();
        await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
        db.Persons.AddRange(
            new Person
            {
                IdCard = "5556667",
                IdCardExtension = "cb",
                FirstName = "Carlos",
                FirstLastName = "Montaño",
                PersonStatusId = 1,
                ScoutLeader = new ScoutLeader(),
            },
            new Person
            {
                IdCard = "5556667",
                IdCardExtension = "LP",
                FirstName = "María",
                FirstLastName = "Paz",
                PersonStatusId = 1,
            }
        );
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var handler = new DSC.Application.Queries.LookupSecretariatParticipantByCiQueryHandler(
            new ScoutLeaderRepository(db)
        );
        await Assert.ThrowsAsync<CiExtensionRequiredException>(() =>
            handler.HandleAsync(new("5556667", null), CancellationToken.None)
        );
        var result = await handler.HandleAsync(new("5556667", "CB"), CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal("Carlos", result.FirstName);
        Assert.True(result.IsScoutLeader);
        Assert.False(result.HasAccount);
        Assert.Empty(db.ChangeTracker.Entries());
    }
}
