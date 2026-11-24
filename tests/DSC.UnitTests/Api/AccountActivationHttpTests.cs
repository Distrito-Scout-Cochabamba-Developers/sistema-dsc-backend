using System.Net;
using System.Net.Http.Json;
using DSC.Api;
using DSC.Api.Controllers;
using DSC.Api.Conventions;
using DSC.Api.Middlewares;
using DSC.Application;
using DSC.Domain.Entities;
using DSC.Infrastructure;
using DSC.Infrastructure.Persistence;
using DSC.UnitTests.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DSC.UnitTests.Api;

[Collection("Training requirements PostgreSQL")]
public class AccountActivationHttpTests(RequirementsPostgres postgres) : IAsyncLifetime
{
    private const string Activation = "/api/v1/auth/activate";
    private const string Password = "Contraseña ficticia 162";
    private const string Username = "cuenta-ficticia-162";
    private ScoutDbContext db = null!;
    private WebApplication app = null!;
    private HttpClient client = null!;
    private long userId;
    private readonly ActivationClock clock = new();

    public async Task InitializeAsync()
    {
        db = await postgres.CreateDatabase(seed: false);
        await db.Database.OpenConnectionAsync();
        await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
        var pending = await db.UserStatuses.SingleAsync(x => x.Code == "PENDING_VERIFICATION");
        var user = new User
        {
            Username = Username,
            Email = "cuenta-162@example.test",
            UserStatus = pending,
            Person = new Person
            {
                IdCard = "99000162",
                FirstName = "Persona",
                FirstLastName = "Ficticia",
                PersonStatus = new PersonStatus { Code = "ACTIVE", Name = "Activo" },
            },
        };
        db.Add(user);
        await db.SaveChangesAsync();
        userId = user.Id;

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Configuration["Jwt:Key"] = "fictitious-ab162-test-only-key-123456789";
        builder.Services.AddControllers(options =>
            options.Conventions.Add(new GlobalRoutePrefixConvention(new RouteAttribute("api")))
        ).AddApplicationPart(typeof(AuthController).Assembly);
        builder.Services.AddApplication();
        builder.Services.AddSingleton<TimeProvider>(clock);
        builder.Services.AddInfrastructure(db.Database.GetConnectionString()!);
        builder.Services.AddApi();
        builder.Services.AddAuthorization();
        app = builder.Build();
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.MapControllers();
        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!
            .Addresses.Single();
        client = new HttpClient { BaseAddress = new Uri(address) };
    }

    [Fact]
    public async Task OpeningLinkDoesNotActivate_ExplicitPostEnablesLoginOnlyOnce()
    {
        var token = await IssueAsync();
        using var get = await client.GetAsync(Activation);
        Assert.True(get.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed);
        Assert.Equal(HttpStatusCode.Unauthorized, await LoginAsync(Password));

        using var activated = await RedeemAsync(token, Password);
        Assert.Equal(HttpStatusCode.NoContent, activated.StatusCode);
        Assert.True(activated.Headers.CacheControl?.NoStore);
        Assert.Equal(HttpStatusCode.OK, await LoginAsync(Password));

        using var replay = await RedeemAsync(token, Password + " otra");
        Assert.Equal(HttpStatusCode.BadRequest, replay.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, await LoginAsync(Password + " otra"));
        Assert.Equal(HttpStatusCode.OK, await LoginAsync(Password));
    }

    [Fact]
    public async Task ExpiredLinkCannotActivate_NewLinkCan()
    {
        var expired = await IssueAsync();
        clock.Advance(TimeSpan.FromHours(24));
        using var rejected = await RedeemAsync(expired, Password);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, await LoginAsync(Password));

        var replaced = await IssueAsync();
        var fresh = await IssueAsync();
        Assert.NotEqual(replaced, fresh);
        using var stale = await RedeemAsync(replaced, Password);
        Assert.Equal(HttpStatusCode.BadRequest, stale.StatusCode);
        using var accepted = await RedeemAsync(fresh, Password);
        Assert.Equal(HttpStatusCode.NoContent, accepted.StatusCode);
    }

    [Fact]
    public async Task ConcurrentPosts_OnlyTheWinningPasswordCanLogIn()
    {
        var token = await IssueAsync();
        var passwords = new[] { Password, "Otra contraseña ficticia" };
        var statuses = await Task.WhenAll(passwords.Select(async password =>
        {
            using var response = await RedeemAsync(token, password);
            return response.StatusCode;
        }));
        Assert.Single(statuses, status => status == HttpStatusCode.NoContent);
        Assert.Single(statuses, status => status == HttpStatusCode.BadRequest);
        for (var i = 0; i < statuses.Length; i++)
            Assert.Equal(
                statuses[i] == HttpStatusCode.NoContent ? HttpStatusCode.OK : HttpStatusCode.Unauthorized,
                await LoginAsync(passwords[i])
            );
    }

    [Fact]
    public async Task PasswordBeyondBCryptLimit_DoesNotConsumeLink()
    {
        var token = await IssueAsync();
        using var tooLong = await RedeemAsync(token, new string('é', 37));
        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
        using var allowed = await RedeemAsync(token, new string('é', 36));
        Assert.Equal(HttpStatusCode.NoContent, allowed.StatusCode);
    }

    private async Task<string> IssueAsync()
    {
        await using var scope = app.Services.CreateAsyncScope();
        var token = await scope.ServiceProvider.GetRequiredService<AccountActivation>().IssueAsync(userId);
        Assert.NotNull(token);
        return token;
    }

    private Task<HttpResponseMessage> RedeemAsync(string token, string password) =>
        client.PostAsJsonAsync(Activation, new { token, password });

    private async Task<HttpStatusCode> LoginAsync(string password)
    {
        using var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { identifier = Username, password });
        return response.StatusCode;
    }

    public async Task DisposeAsync()
    {
        client?.Dispose();
        if (app is not null)
            await app.DisposeAsync();
        if (db is not null)
            await db.DisposeAsync();
    }

    private sealed class ActivationClock : TimeProvider
    {
        private DateTimeOffset now = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => now;
        public void Advance(TimeSpan elapsed) => now += elapsed;
    }
}
