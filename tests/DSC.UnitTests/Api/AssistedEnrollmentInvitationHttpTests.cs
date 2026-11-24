using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using DSC.Api;
using DSC.Api.Controllers;
using DSC.Api.Conventions;
using DSC.Api.Middlewares;
using DSC.Application;
using DSC.Application.Interfaces;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace DSC.UnitTests.Api;

/// <summary>
/// AB#161: correo de invitación tras la inscripción asistida de un dirigente nuevo.
/// Seams: HTTP de Secretaría, canje público y la frontera <see cref="IAccountInvitationMailer"/>
/// (sustituida por un transporte ficticio). PostgreSQL efímero y datos ficticios.
/// </summary>
[Collection("Training requirements PostgreSQL")]
public class AssistedEnrollmentInvitationHttpTests(RequirementsPostgres postgres) : IAsyncLifetime
{
    private const string Origin = "https://dsc.example.test";
    private const string Ci = "8101001";
    private const string Email = "ana.invitada@example.test";
    private static readonly SymmetricSecurityKey Key = new(new byte[32]);

    private ScoutDbContext db = null!;
    private WebApplication app = null!;
    private HttpClient client = null!;
    private Person secretary = null!;
    private long secretaryUserId;
    private TrainingModule module = null!;
    private TrainingModuleSessionStatus open = null!;
    private readonly FakeMailer mailer = new();
    private readonly Logs logs = new();
    private string? origin = Origin;
    private bool allowLoopbackHttp;

    public async Task InitializeAsync()
    {
        db = await postgres.CreateDatabase();
        await db.Database.OpenConnectionAsync();
        await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();
        open = new TrainingModuleSessionStatus { Code = "OPEN", Name = "Abierta" };
        db.TrainingModuleSessionStatuses.Add(open);
        db.EnrollmentSources.Add(new EnrollmentSource { Code = "AUTHENTICATED_USER", Name = "Usuario autenticado" });
        await db.SaveChangesAsync();
        module = await db.TrainingModules.FirstAsync();
        var personStatus = await db.PersonStatuses.SingleAsync(s => s.Code == "ACTIVE");
        var userStatus = await db.UserStatuses.SingleAsync(s => s.Code == "ACTIVE");
        secretary = new Person { IdCard = "9990101", FirstName = "Secretaria", FirstLastName = "Prueba", PersonStatusId = personStatus.Id };
        db.Persons.Add(secretary);
        await db.SaveChangesAsync();
        var user = new User { PersonId = secretary.Id, UserStatusId = userStatus.Id, Username = "secretaria-ficticia", Email = "secretaria@example.test", Password = "ficticia" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        secretaryUserId = user.Id;
        db.ChangeTracker.Clear();
        await StartAppAsync();
    }

    private async Task StartAppAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(logs);
        if (origin is not null)
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["Activation:FrontendOrigin"] = origin, ["Activation:AllowLoopbackHttp"] = allowLoopbackHttp.ToString() });
        builder.Services.AddControllers(o => o.Conventions.Add(new GlobalRoutePrefixConvention(new RouteAttribute("api"))))
            .AddApplicationPart(typeof(CalendarController).Assembly);
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(db.Database.GetConnectionString()!);
        builder.Services.AddScoped<IAccountInvitationMailer>(_ => mailer);
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
    public async Task ConfirmedNewLeaderGetsPendingAccountAndOneInvitationWithUsernameAndLink()
    {
        var session = await CreateSessionAsync(capacity: 1);
        using var response = await Enroll(session.Id, Ci);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var user = await db.Users.Include(u => u.UserStatus).SingleAsync(u => u.Email == Email);
        Assert.Equal("PENDING_VERIFICATION", user.UserStatus.Code);
        Assert.Equal("", user.Password);
        var message = Assert.Single(mailer.Sent);
        Assert.Equal(Email, message.To);
        Assert.Equal(user.Username, message.Username);
        Assert.StartsWith($"{Origin}/auth/activar#token=", message.ActivationUrl);
        Assert.Equal(43, message.ActivationUrl[$"{Origin}/auth/activar#token=".Length..].Length);
        Assert.Equal("SENT", (await db.Set<AccountInvitation>().SingleAsync()).Status);
        Assert.Contains("\"invitation\":\"SENT\"", body);
        Assert.DoesNotContain("token=", body);
        Assert.DoesNotContain(Email, body);
    }

    [Fact]
    public async Task SentLinkActivatesTheAccountAndNeverCarriesPersonalData()
    {
        var session = await CreateSessionAsync(capacity: 1);
        using var response = await Enroll(session.Id, Ci);
        var message = Assert.Single(mailer.Sent);
        var token = message.ActivationUrl[(message.ActivationUrl.IndexOf("token=") + 6)..];

        Assert.DoesNotContain(Ci, message.ActivationUrl);
        Assert.DoesNotContain("ana", message.ActivationUrl, StringComparison.OrdinalIgnoreCase);
        using var redeem = await client.PostAsJsonAsync("/api/v1/auth/activate", new { token, password = "clave-ficticia-segura-1" });
        Assert.Equal(HttpStatusCode.NoContent, redeem.StatusCode);
    }

    [Fact]
    public async Task FailedDeliveryLeavesEnrollmentAndPersistentPendingInvitation()
    {
        var session = await CreateSessionAsync(capacity: 1);
        mailer.FailWith = new InvalidOperationException($"SMTP caído para {Email} en {Ci}");
        using var response = await Enroll(session.Id, Ci);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Contains("\"invitation\":\"PENDING\"", await response.Content.ReadAsStringAsync());
        Assert.Equal(1, await db.TrainingModuleEnrollments.CountAsync(e => e.Person.IdCard == Ci));
        var invitation = await db.Set<AccountInvitation>().SingleAsync();
        Assert.Equal("PENDING", invitation.Status);
        Assert.Equal(1, invitation.Attempts);
        Assert.Equal("InvalidOperationException", invitation.LastFailureCode);
        Assert.NotEmpty(logs.Messages);
        Assert.DoesNotContain(logs.Messages, l => l.Contains(Email) || l.Contains(Ci) || l.Contains("token="));
    }

    [Fact]
    public async Task SecretaryManualRetryDeliversWithoutTouchingEnrollmentAndInvalidatesPreviousLink()
    {
        var session = await CreateSessionAsync(capacity: 1);
        mailer.FailWith = new InvalidOperationException("fallo");
        using var created = await Enroll(session.Id, Ci);
        var enrollmentId = EnrollmentId(await created.Content.ReadAsStringAsync());
        var before = await db.TrainingModuleEnrollments.AsNoTracking().SingleAsync(e => e.Id == enrollmentId);
        mailer.FailWith = null;

        using var retry = await Retry(enrollmentId);

        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.Single(mailer.Sent);
        Assert.Equal("SENT", (await db.Set<AccountInvitation>().AsNoTracking().SingleAsync()).Status);
        var after = await db.TrainingModuleEnrollments.AsNoTracking().SingleAsync(e => e.Id == enrollmentId);
        Assert.Equal(before.UpdatedAt, after.UpdatedAt);
        Assert.Equal(1, await db.TrainingModuleEnrollments.CountAsync());
    }

    [Fact]
    public async Task RetryAfterDeliveryReplacesNothingAndSendsNothing()
    {
        var session = await CreateSessionAsync(capacity: 1);
        using var created = await Enroll(session.Id, Ci);
        var enrollmentId = EnrollmentId(await created.Content.ReadAsStringAsync());

        using var retry = await Retry(enrollmentId);

        Assert.Equal(HttpStatusCode.Conflict, retry.StatusCode);
        Assert.Contains("already-sent", await retry.Content.ReadAsStringAsync());
        Assert.Single(mailer.Sent);
    }

    [Fact]
    public async Task SimultaneousRetriesSendOnlyOnce()
    {
        var session = await CreateSessionAsync(capacity: 1);
        mailer.FailWith = new InvalidOperationException("fallo");
        using var created = await Enroll(session.Id, Ci);
        var enrollmentId = EnrollmentId(await created.Content.ReadAsStringAsync());
        mailer.FailWith = null;
        mailer.Delay = TimeSpan.FromMilliseconds(300);

        var responses = await Task.WhenAll(Retry(enrollmentId), Retry(enrollmentId), Retry(enrollmentId));
        try
        {
            Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.OK));
            Assert.All(responses.Where(r => r.StatusCode != HttpStatusCode.OK), r => Assert.Equal(HttpStatusCode.Conflict, r.StatusCode));
        }
        finally { foreach (var r in responses) r.Dispose(); }
        Assert.Single(mailer.Sent);
    }

    [Fact]
    public async Task RetryThatFailsAgainKeepsPendingAndCountsAttempts()
    {
        var session = await CreateSessionAsync(capacity: 1);
        mailer.FailWith = new TimeoutException("fallo");
        using var created = await Enroll(session.Id, Ci);
        var enrollmentId = EnrollmentId(await created.Content.ReadAsStringAsync());

        using var retry = await Retry(enrollmentId);

        Assert.Equal(HttpStatusCode.BadGateway, retry.StatusCode);
        Assert.Contains("delivery-failed", await retry.Content.ReadAsStringAsync());
        var invitation = await db.Set<AccountInvitation>().AsNoTracking().SingleAsync();
        Assert.Equal("PENDING", invitation.Status);
        Assert.Equal(2, invitation.Attempts);
        Assert.Equal("TimeoutException", invitation.LastFailureCode);
    }

    [Fact]
    public async Task PendingInvitationSurvivesRestartAndIsListedWithoutPersonalData()
    {
        var session = await CreateSessionAsync(capacity: 1);
        mailer.FailWith = new InvalidOperationException("fallo");
        using var created = await Enroll(session.Id, Ci);
        var enrollmentId = EnrollmentId(await created.Content.ReadAsStringAsync());
        await app.DisposeAsync();
        client.Dispose();
        await StartAppAsync();
        mailer.FailWith = null;

        using var list = await Send(HttpMethod.Get, "/api/v1/secretariat/invitations/pending");
        var text = await list.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        Assert.Contains($"\"enrollmentId\":{enrollmentId}", text);
        Assert.DoesNotContain(Email, text);
        Assert.DoesNotContain(Ci, text);
        using var retry = await Retry(enrollmentId);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.Single(mailer.Sent);
    }

    [Fact]
    public async Task LostSeatLeavesNoAccountInvitationOrMail()
    {
        var session = await CreateSessionAsync(capacity: 1);
        using var first = await Enroll(session.Id, Ci);
        using var second = await Enroll(session.Id, "8101002", "otra@example.test");

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.False(await db.Users.AnyAsync(u => u.Email == "otra@example.test"));
        Assert.Equal(1, await db.Set<AccountInvitation>().CountAsync());
        Assert.Single(mailer.Sent);
    }

    [Fact]
    public async Task EmailAlreadyUsedByAnotherAccountRejectsWholeOperation()
    {
        var session = await CreateSessionAsync(capacity: 2);
        using var response = await Enroll(session.Id, Ci, "secretaria@example.test");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("email-in-use", await response.Content.ReadAsStringAsync());
        Assert.False(await db.Persons.AnyAsync(p => p.IdCard == Ci));
        Assert.Empty(await db.TrainingModuleEnrollments.ToListAsync());
        Assert.Empty(mailer.Sent);
    }

    [Theory]
    [InlineData(true, "http://localhost:4200", "SENT")]
    [InlineData(true, "http://dsc.example.test", "PENDING")]
    [InlineData(false, "http://localhost:4200", "PENDING")]
    public async Task HttpLoopbackOriginIsAcceptedOnlyWhenExplicitlyAllowed(bool allow, string configuredOrigin, string expected)
    {
        await app.DisposeAsync();
        client.Dispose();
        origin = configuredOrigin;
        allowLoopbackHttp = allow;
        await StartAppAsync();
        var session = await CreateSessionAsync(capacity: 1);
        using var response = await Enroll(session.Id, Ci);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Contains($"\"invitation\":\"{expected}\"", await response.Content.ReadAsStringAsync());
        Assert.Equal(expected == "SENT" ? 1 : 0, mailer.Sent.Count);
    }

    [Fact]
    public async Task MissingOrInsecureOriginKeepsPendingAndSendsNothing()
    {
        await app.DisposeAsync();
        client.Dispose();
        origin = "http://dsc.example.test";
        await StartAppAsync();
        var session = await CreateSessionAsync(capacity: 1);
        using var response = await Enroll(session.Id, Ci);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Contains("\"invitation\":\"PENDING\"", await response.Content.ReadAsStringAsync());
        Assert.Empty(mailer.Sent);
        Assert.Equal("InvalidActivationOrigin", (await db.Set<AccountInvitation>().AsNoTracking().SingleAsync()).LastFailureCode);
    }

    [Fact]
    public async Task OnlyDistrictSecretaryCanRetryOrListInvitations()
    {
        var session = await CreateSessionAsync(capacity: 1);
        mailer.FailWith = new InvalidOperationException("fallo");
        using var created = await Enroll(session.Id, Ci);
        var enrollmentId = EnrollmentId(await created.Content.ReadAsStringAsync());
        mailer.FailWith = null;

        using var forbidden = await Retry(enrollmentId, role: "SCOUT_LEADER");
        using var anonymous = await client.PostAsync($"/api/v1/secretariat/invitations/{enrollmentId}/retry", null);
        using var forbiddenList = await Send(HttpMethod.Get, "/api/v1/secretariat/invitations/pending", role: "SCOUT_LEADER");

        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenList.StatusCode);
        Assert.Empty(mailer.Sent);
    }

    [Fact]
    public async Task InvalidNewLeaderDataReturnsFixedMessageWithoutEchoingInput()
    {
        var session = await CreateSessionAsync(capacity: 1);
        const string badEmail = "dato-sensible-invalido";
        using var response = await Enroll(session.Id, Ci, badEmail);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Los datos de la persona no son válidos.", JsonDocument.Parse(body).RootElement.GetProperty("detail").GetString());
        Assert.DoesNotContain(badEmail, body);
        Assert.DoesNotContain(Ci, body);
    }

    [Theory]
    [InlineData("LP")]
    [InlineData(null)]
    public async Task NewLeaderWhoseCiExistsWithAnotherComplementAsksForTheComplement(string? extension)
    {
        var session = await CreateSessionAsync(capacity: 1);
        db.Persons.Add(new Person { IdCard = Ci, IdCardExtension = "CB", FirstName = "Existente", FirstLastName = "Ficticio", PersonStatusId = (await db.PersonStatuses.SingleAsync(s => s.Code == "ACTIVE")).Id });
        await db.SaveChangesAsync();
        using var response = await Send(HttpMethod.Post, $"/api/v1/secretariat/participants/sessions/{session.Id}/enrollments",
            new { ci = Ci, extension, firstName = "Ana", lastName = "Prueba", phone = "70000000", email = Email });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("urn:dsc:secretariat:extension-required", body);
        Assert.DoesNotContain(Ci, body);
        Assert.Equal(1, await db.Persons.CountAsync(p => p.IdCard == Ci));
        Assert.Empty(await db.TrainingModuleEnrollments.ToListAsync());
        Assert.Empty(mailer.Sent);
    }

    [Fact]
    public async Task NewLeaderWhoseCiExistsWithSameComplementIsAnIdentityChangeNotAComplementRequest()
    {
        var session = await CreateSessionAsync(capacity: 1);
        db.Persons.Add(new Person { IdCard = Ci, FirstName = "Existente", FirstLastName = "Ficticio", PersonStatusId = (await db.PersonStatuses.SingleAsync(s => s.Code == "ACTIVE")).Id });
        await db.SaveChangesAsync();
        using var response = await Enroll(session.Id, Ci);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("urn:dsc:secretariat:identity-changed", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ConfirmingExistingPersonWhoseCiSharesSeveralComplementsAsksForTheComplement()
    {
        var session = await CreateSessionAsync(capacity: 1);
        var status = await db.PersonStatuses.SingleAsync(s => s.Code == "ACTIVE");
        var first = new Person { IdCard = Ci, IdCardExtension = "CB", FirstName = "Uno", FirstLastName = "Ficticio", PersonStatusId = status.Id, ScoutLeader = new ScoutLeader() };
        db.Persons.AddRange(first, new Person { IdCard = Ci, IdCardExtension = "LP", FirstName = "Dos", FirstLastName = "Ficticio", PersonStatusId = status.Id, ScoutLeader = new ScoutLeader() });
        await db.SaveChangesAsync();
        using var response = await Send(HttpMethod.Post, $"/api/v1/secretariat/participants/{first.Id}/sessions/{session.Id}/enrollments", new { ci = Ci });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("urn:dsc:secretariat:extension-required", await response.Content.ReadAsStringAsync());
        Assert.Empty(await db.TrainingModuleEnrollments.ToListAsync());
    }

    [Fact]
    public async Task RetryOfUnknownEnrollmentIsNotFound()
    {
        using var retry = await Retry(987654);
        Assert.Equal(HttpStatusCode.NotFound, retry.StatusCode);
    }

    [Fact]
    public async Task RetryAfterAccountWasActivatedClosesInvitationWithoutSending()
    {
        var session = await CreateSessionAsync(capacity: 1);
        mailer.FailWith = new InvalidOperationException("fallo");
        using var created = await Enroll(session.Id, Ci);
        var enrollmentId = EnrollmentId(await created.Content.ReadAsStringAsync());
        mailer.FailWith = null;
        var active = await db.UserStatuses.SingleAsync(s => s.Code == "ACTIVE");
        await db.Users.Where(u => u.Email == Email).ExecuteUpdateAsync(s => s.SetProperty(u => u.UserStatusId, active.Id).SetProperty(u => u.Password, "hash"));

        using var retry = await Retry(enrollmentId);

        Assert.Equal(HttpStatusCode.Conflict, retry.StatusCode);
        Assert.Contains("not-eligible", await retry.Content.ReadAsStringAsync());
        Assert.Empty(mailer.Sent);
        Assert.Equal("CLOSED", (await db.Set<AccountInvitation>().AsNoTracking().SingleAsync()).Status);
    }

    private Task<HttpResponseMessage> Enroll(long sessionId, string ci, string email = Email) =>
        Send(HttpMethod.Post, $"/api/v1/secretariat/participants/sessions/{sessionId}/enrollments",
            new { ci, firstName = "Ana", lastName = "Prueba", phone = "70000000", email });

    private Task<HttpResponseMessage> Retry(long enrollmentId, string role = "DISTRICT_SECRETARY") =>
        Send(HttpMethod.Post, $"/api/v1/secretariat/invitations/{enrollmentId}/retry", role: role);

    private async Task<HttpResponseMessage> Send(HttpMethod method, string url, object? body = null, string role = "DISTRICT_SECRETARY")
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null) request.Content = JsonContent.Create(body);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token(role));
        return await client.SendAsync(request);
    }

    private static long EnrollmentId(string json) => JsonDocument.Parse(json).RootElement.GetProperty("enrollmentId").GetInt64();

    private string Token(string role) => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
        claims: [new Claim("sub", secretaryUserId.ToString()), new Claim("personId", secretary.Id.ToString()), new Claim("roles", role)],
        expires: DateTime.UtcNow.AddMinutes(5),
        signingCredentials: new SigningCredentials(Key, SecurityAlgorithms.HmacSha256)));

    private async Task<TrainingModuleSession> CreateSessionAsync(int capacity)
    {
        var session = new TrainingModuleSession
        {
            TrainingModuleId = module.Id,
            TrainingModuleSessionStatusId = open.Id,
            IsRegistrationOpen = true,
            Capacity = capacity,
            QrToken = Guid.NewGuid(),
            Schedules = [new TrainingModuleSchedule { PartNumber = 1, SessionDate = new DateOnly(2026, 12, 5), StartTime = new TimeOnly(8, 30), EndTime = new TimeOnly(12, 30) }],
        };
        db.TrainingModuleSessions.Add(session);
        await db.SaveChangesAsync();
        return session;
    }

    private sealed class FakeMailer : IAccountInvitationMailer
    {
        private readonly ConcurrentQueue<AccountInvitationMessage> sent = new();
        public IReadOnlyCollection<AccountInvitationMessage> Sent => sent;
        public Exception? FailWith { get; set; }
        public TimeSpan Delay { get; set; }

        public async Task SendAsync(AccountInvitationMessage message, CancellationToken cancellationToken)
        {
            if (Delay > TimeSpan.Zero) await Task.Delay(Delay, cancellationToken);
            if (FailWith is not null) throw FailWith;
            sent.Enqueue(message);
        }
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
