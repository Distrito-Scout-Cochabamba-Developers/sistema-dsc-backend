using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using DSC.Api;
using DSC.Api.Controllers;
using DSC.Api.Conventions;
using DSC.Api.Middlewares;
using DSC.Application;
using DSC.Domain.Entities;
using DSC.Infrastructure;
using DSC.Infrastructure.Persistence;
using DSC.UnitTests.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace DSC.UnitTests.Api;

/// <summary>
/// #160: alta mínima de un Rover sin cuenta inseparable de su inscripción asistida.
/// Datos ficticios y PostgreSQL efímero; se prueba por HTTP, sin acceder a internos.
/// </summary>
[Collection("Training requirements PostgreSQL")]
public class SecretariatRoverEnrollmentHttpTests(RequirementsPostgres postgres) : IAsyncLifetime
{
    private static readonly SymmetricSecurityKey Key = new(new byte[32]);
    private ScoutDbContext db = null!;
    private WebApplication app = null!;
    private HttpClient client = null!;
    private TrainingModule module = null!;
    private TrainingModuleSessionStatus open = null!;
    private PersonStatus personStatus = null!;
    private UserStatus userStatus = null!;
    private Person secretary = null!;
    private long secretaryUserId;
    private readonly Logs logs = new();

    public async Task InitializeAsync()
    {
        db = await postgres.CreateDatabase();
        await db.Database.OpenConnectionAsync();
        await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
        open = new TrainingModuleSessionStatus { Code = "OPEN", Name = "Abierta" };
        personStatus = await db.PersonStatuses.SingleAsync(s => s.Code == "ACTIVE");
        userStatus = await db.UserStatuses.SingleAsync(s => s.Code == "ACTIVE");
        db.TrainingModuleSessionStatuses.Add(open);
        db.EnrollmentSources.Add(new EnrollmentSource { Code = "AUTHENTICATED_USER", Name = "Usuario autenticado" });
        await db.SaveChangesAsync();
        module = await db.TrainingModules.FirstAsync();
        secretary = new Person { IdCard = "9990001", FirstName = "Secretaria", FirstLastName = "Prueba", PersonStatusId = personStatus.Id };
        db.Persons.Add(secretary);
        await db.SaveChangesAsync();
        var user = new User { PersonId = secretary.Id, UserStatusId = userStatus.Id, Username = "secretaria-ficticia", Email = "secretaria@example.test", Password = "ficticia" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        secretaryUserId = user.Id;
        db.ChangeTracker.Clear();

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(logs);
        builder.Services.AddControllers(o => o.Conventions.Add(new GlobalRoutePrefixConvention(new RouteAttribute("api"))))
            .AddApplicationPart(typeof(CalendarController).Assembly);
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(db.Database.GetConnectionString()!);
        builder.Services.AddApi();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
        {
            o.MapInboundClaims = false;
            o.TokenValidationParameters = new() { ValidateIssuerSigningKey = true, IssuerSigningKey = Key, ValidateIssuer = false, ValidateAudience = false, RoleClaimType = "roles" };
        });
        builder.Services.AddAuthorization();
        app = builder.Build();
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        client = new HttpClient { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
    }

    [Fact]
    public async Task NewRoverIsCreatedAndEnrolledTogetherWithoutAccount()
    {
        var session = await CreateSessionAsync(capacity: 1);
        using var response = await Post(session.Id, new { ci = "7001001", extension = "cb", firstName = "Rover", lastName = "Ficticio", phone = "70000001" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var person = await db.Persons.Include(p => p.ContactMechanisms).SingleAsync(p => p.IdCard == "7001001");
        Assert.Equal("CB", person.IdCardExtension);
        Assert.Equal("70000001", Assert.Single(person.ContactMechanisms).ContactValue);
        Assert.False(await db.Users.AnyAsync(u => u.PersonId == person.Id));
        Assert.False(await db.ScoutLeaders.AnyAsync(l => l.PersonId == person.Id));
        Assert.False(await db.ScoutAffiliations.AnyAsync(a => a.PersonId == person.Id));
        var enrollment = await db.TrainingModuleEnrollments.SingleAsync(e => e.PersonId == person.Id);
        Assert.True(enrollment.RoverWithoutAccount);
        Assert.Equal(secretaryUserId, enrollment.RegisteredByUserId);
    }

    [Fact]
    public async Task NewRoverWithoutPhoneIsAccepted()
    {
        var session = await CreateSessionAsync(capacity: 1);
        using var response = await Post(session.Id, new { ci = "7002001", firstName = "Rover", lastName = "Ficticio" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var person = await db.Persons.Include(p => p.ContactMechanisms).SingleAsync(p => p.IdCard == "7002001");
        Assert.Empty(person.ContactMechanisms);
        Assert.Null(person.IdCardExtension);
    }

    [Fact]
    public async Task ExistingCiIsNeverReusedNorDuplicatedByCreation()
    {
        var session = await CreateSessionAsync(capacity: 1);
        db.Persons.Add(new Person { IdCard = "7003001", FirstName = "Existente", FirstLastName = "Ficticio", PersonStatusId = personStatus.Id });
        await db.SaveChangesAsync();
        using var response = await Post(session.Id, new { ci = "7003001", extension = "LP", firstName = "Rover", lastName = "Ficticio" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("person-exists", await response.Content.ReadAsStringAsync());
        Assert.Equal(1, await db.Persons.CountAsync(p => p.IdCard == "7003001"));
        Assert.Empty(await db.TrainingModuleEnrollments.ToListAsync());
    }

    [Fact]
    public async Task LostLastSeatLeavesNoProfileContactOrEnrollment()
    {
        var session = await CreateSessionAsync(capacity: 1);
        using var first = await Post(session.Id, new { ci = "7004001", firstName = "Rover", lastName = "Uno" });
        using var second = await Post(session.Id, new { ci = "7004002", firstName = "Rover", lastName = "Dos", phone = "70000004" });

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Contains("session-full", await second.Content.ReadAsStringAsync());
        Assert.False(await db.Persons.AnyAsync(p => p.IdCard == "7004002"));
        Assert.False(await db.PersonContactMechanisms.AnyAsync(c => c.ContactValue == "70000004"));
        Assert.Equal(1, await db.TrainingModuleEnrollments.CountAsync());
    }

    [Fact]
    public async Task ClosedRegistrationLeavesNoProfile()
    {
        var session = await CreateSessionAsync(capacity: 5, isRegistrationOpen: false);
        using var response = await Post(session.Id, new { ci = "7005001", firstName = "Rover", lastName = "Ficticio" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("enrollment-closed", await response.Content.ReadAsStringAsync());
        Assert.False(await db.Persons.AnyAsync(p => p.IdCard == "7005001"));
    }

    [Fact]
    public async Task MissingSessionLeavesNoProfile()
    {
        using var response = await Post(987654, new { ci = "7006001", firstName = "Rover", lastName = "Ficticio" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(await db.Persons.AnyAsync(p => p.IdCard == "7006001"));
    }

    [Theory]
    [InlineData("abc", "Rover", "Ficticio", null)]
    [InlineData("7007001", "", "Ficticio", null)]
    [InlineData("7007001", "Rover", "  ", null)]
    [InlineData("7007001", "Rover", "Ficticio", "no-es-un-celular")]
    public async Task InvalidDataIsRejectedWithoutWriting(string ci, string first, string last, string? phone)
    {
        var session = await CreateSessionAsync(capacity: 1);
        using var response = await Post(session.Id, new { ci, firstName = first, lastName = last, phone });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(1, await db.Persons.CountAsync());
        Assert.Empty(await db.TrainingModuleEnrollments.ToListAsync());
    }

    [Fact]
    public async Task OnlyDistrictSecretaryCanCreateRovers()
    {
        var session = await CreateSessionAsync(capacity: 1);
        using var response = await Post(session.Id, new { ci = "7008001", firstName = "Rover", lastName = "Ficticio" }, role: "SCOUT_LEADER");
        using var anonymous = await client.PostAsJsonAsync($"/api/v1/secretariat/sessions/{session.Id}/rover-enrollments", new { ci = "7008001", firstName = "Rover", lastName = "Ficticio" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.False(await db.Persons.AnyAsync(p => p.IdCard == "7008001"));
    }

    [Fact]
    public async Task SimultaneousCreationOfSameCiLeavesOnePersonAndOneEnrollment()
    {
        var session = await CreateSessionAsync(capacity: 5);
        var body = new { ci = "7009001", firstName = "Rover", lastName = "Ficticio" };
        var responses = await Task.WhenAll(Post(session.Id, body), Post(session.Id, body), Post(session.Id, body));
        try
        {
            Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
            Assert.All(responses.Where(r => r.StatusCode != HttpStatusCode.Created), r => Assert.Equal(HttpStatusCode.Conflict, r.StatusCode));
        }
        finally { foreach (var r in responses) r.Dispose(); }
        Assert.Equal(1, await db.Persons.CountAsync(p => p.IdCard == "7009001"));
        Assert.Equal(1, await db.TrainingModuleEnrollments.CountAsync());
    }

    [Fact]
    public async Task ResponsesAndLogsDoNotExposePersonalData()
    {
        var session = await CreateSessionAsync(capacity: 5);
        using var created = await Post(session.Id, new { ci = "7010001", firstName = "Rover", lastName = "Ficticio", phone = "70000010" });
        using var duplicate = await Post(session.Id, new { ci = "7010001", firstName = "Rover", lastName = "Ficticio", phone = "70000010" });
        var text = await created.Content.ReadAsStringAsync() + await duplicate.Content.ReadAsStringAsync();

        Assert.DoesNotContain("7010001", text);
        Assert.DoesNotContain("70000010", text);
        Assert.NotEmpty(logs.Messages);
        Assert.DoesNotContain(logs.Messages, l => l.Contains("7010001") || l.Contains("70000010"));
    }

    private async Task<HttpResponseMessage> Post(long sessionId, object body, string role = "DISTRICT_SECRETARY")
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/secretariat/sessions/{sessionId}/rover-enrollments") { Content = JsonContent.Create(body) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token(role));
        return await client.SendAsync(request);
    }

    private string Token(string role) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
        claims: [new Claim("sub", secretaryUserId.ToString()), new Claim("personId", secretary.Id.ToString()), new Claim("roles", role)],
        expires: DateTime.UtcNow.AddMinutes(5),
        signingCredentials: new SigningCredentials(Key, SecurityAlgorithms.HmacSha256)));

    private async Task<TrainingModuleSession> CreateSessionAsync(int capacity, bool isRegistrationOpen = true)
    {
        var session = new TrainingModuleSession
        {
            TrainingModuleId = module.Id,
            TrainingModuleSessionStatusId = open.Id,
            IsRegistrationOpen = isRegistrationOpen,
            Capacity = capacity,
            QrToken = Guid.NewGuid(),
            Schedules = [new TrainingModuleSchedule { PartNumber = 1, SessionDate = new DateOnly(2026, 12, 5), StartTime = new TimeOnly(8, 30), EndTime = new TimeOnly(12, 30) }],
        };
        db.TrainingModuleSessions.Add(session);
        await db.SaveChangesAsync();
        return session;
    }

    private sealed class Logs : ILoggerProvider, ILogger
    {
        public ConcurrentQueue<string> Messages { get; } = new();
        public ILogger CreateLogger(string categoryName) => this;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel level, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Messages.Enqueue($"{formatter(state, exception)} {exception}");
        public void Dispose() { }
    }

    public async Task DisposeAsync()
    {
        client?.Dispose();
        if (app is not null) await app.DisposeAsync();
        if (db is not null) await db.DisposeAsync();
    }
}
