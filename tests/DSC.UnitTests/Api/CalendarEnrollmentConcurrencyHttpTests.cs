using System.IdentityModel.Tokens.Jwt;
using System.Collections.Concurrent;
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
using DSC.Application.DTOs;
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
/// Competencia real por el último cupo con conexiones independientes sobre PostgreSQL efímero.
/// El contrato HTTP de autoservicio (201/409 y DTO) no cambia.
/// </summary>
[Collection("Training requirements PostgreSQL")]
public class CalendarEnrollmentConcurrencyHttpTests(RequirementsPostgres postgres) : IAsyncLifetime
{
    private const string Calendar = "/api/v1/calendar/sessions";
    private static readonly SymmetricSecurityKey Key = new(new byte[32]);

    private ScoutDbContext db = null!;
    private WebApplication app = null!;
    private HttpClient client = null!;
    private TrainingModule module = null!;
    private TrainingModuleSessionStatus open = null!;
    private TrainingModuleSessionStatus cancelled = null!;
    private TrainingModuleSessionStatus closed = null!;
    private PersonStatus personStatus = null!;
    private EnrollmentSource source = null!;
    private UserStatus userStatus = null!;
    private readonly Dictionary<long, long> userIds = [];
    private readonly CapturedLogs logs = new();

    public async Task InitializeAsync()
    {
        db = await postgres.CreateDatabase();
        await db.Database.OpenConnectionAsync();
        await ((NpgsqlConnection)db.Database.GetDbConnection()).ReloadTypesAsync();

        open = new TrainingModuleSessionStatus { Code = "OPEN", Name = "Abierta" };
        cancelled = new TrainingModuleSessionStatus { Code = "CANCELLED", Name = "Cancelada" };
        closed = new TrainingModuleSessionStatus { Code = "CLOSED", Name = "Cerrada" };
        personStatus = new PersonStatus { Code = "ACTIVE", Name = "Activa" };
        userStatus = await db.UserStatuses.SingleAsync(status => status.Code == "ACTIVE");
        source = new EnrollmentSource { Code = "AUTHENTICATED_USER", Name = "Usuario autenticado" };
        db.TrainingModuleSessionStatuses.AddRange(open, cancelled, closed);
        db.PersonStatuses.Add(personStatus);
        db.EnrollmentSources.Add(source);
        await db.SaveChangesAsync();
        module = await db.TrainingModules.FirstAsync();
        db.ChangeTracker.Clear();

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
            .AddApplicationPart(typeof(CalendarController).Assembly);
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(db.Database.GetConnectionString()!);
        builder.Services.AddApi();
        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = Key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RoleClaimType = "roles",
                };
            });
        builder.Services.AddAuthorization();
        app = builder.Build();
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();

        client = new HttpClient
        {
            BaseAddress = new Uri(
                app.Services.GetRequiredService<IServer>()
                    .Features.Get<IServerAddressesFeature>()!
                    .Addresses.Single()
            ),
        };
    }

    [Fact]
    public async Task AssistedConfirmationDoesNotExposePersonalDataInResponseOrLogs()
    {
        const string ci = "9876543";
        const string phone = "76543210";
        var session = await CreateSessionAsync(capacity: 1);
        var leader = await CreatePersonAsync(ci, "Dirigente");
        var secretary = await CreatePersonAsync("9876544", "Secretaria");
        db.ScoutLeaders.Add(new ScoutLeader { PersonId = leader.Id });
        db.Add(new PersonContactMechanism { PersonId = leader.Id, ContactMechanismType = new() { Code = "TEST_PHONE", Name = "Teléfono ficticio" }, ContactPurposeType = new() { Code = "TEST_PERSONAL", Name = "Personal ficticio" }, ContactValue = phone });
        await db.SaveChangesAsync();
        var url = $"/api/v1/secretariat/participants/{leader.Id}/sessions/{session.Id}/enrollments";
        Assert.DoesNotContain(ci, url);
        Assert.DoesNotContain(phone, url);
        async Task<HttpResponseMessage> Confirm()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(new { }) };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token(secretary, "DISTRICT_SECRETARY"));
            return await client.SendAsync(request);
        }
        using var success = await Confirm();
        Assert.Equal(HttpStatusCode.Created, success.StatusCode);
        var successBody = await success.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(successBody);
        Assert.Equal(new[] { "enrollmentId" }, json.RootElement.EnumerateObject().Select(p => p.Name).ToArray());
        using var duplicate = await Confirm();
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        var conflictBody = await duplicate.Content.ReadAsStringAsync();
        Assert.DoesNotContain(ci, successBody + conflictBody);
        Assert.DoesNotContain(phone, successBody + conflictBody);
        Assert.NotEmpty(logs.Messages);
        Assert.DoesNotContain(logs.Messages, line => line.Contains(ci) || line.Contains(phone));
    }

    private sealed class CapturedLogs : ILoggerProvider, ILogger
    {
        public ConcurrentQueue<string> Messages { get; } = new();
        public ILogger CreateLogger(string categoryName) => this;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel level, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Messages.Enqueue($"{formatter(state, exception)} {exception}");
        public void Dispose() { }
    }

    [Fact]
    public async Task AssistedEnrollmentRequiresSecretaryAndPersistsJwtOperator()
    {
        var session = await CreateSessionAsync(capacity: 1);
        var leader = await CreatePersonAsync("9100021", "Dirigente");
        var secretary = await CreatePersonAsync("9100022", "Secretaria");
        db.ScoutLeaders.Add(new ScoutLeader { PersonId = leader.Id });
        await db.SaveChangesAsync();
        var url = $"/api/v1/secretariat/participants/{leader.Id}/sessions/{session.Id}/enrollments";
        using var anonymous = await client.PostAsync(url, null);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        using var forbiddenRequest = new HttpRequestMessage(HttpMethod.Post, url);
        forbiddenRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token(leader));
        using var forbidden = await client.SendAsync(forbiddenRequest);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(new { registeredByUserId = userIds[leader.Id] }) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token(secretary, "DISTRICT_SECRETARY"));
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var enrollment = await db.TrainingModuleEnrollments.Include(e => e.EnrollmentSource).SingleAsync(e => e.TrainingModuleSessionId == session.Id);
        Assert.Equal(userIds[secretary.Id], enrollment.RegisteredByUserId);
        Assert.Equal("SECRETARIAT", enrollment.EnrollmentSource.Code);
    }

    [Fact]
    public async Task SecretariatAndSelfServiceCompeteForLastSeat()
    {
        var session = await CreateSessionAsync(capacity: 1);
        var first = await CreatePersonAsync("9100011", "Ana");
        var second = await CreatePersonAsync("9100012", "Beto");
        var secretary = await CreatePersonAsync("9100013", "Secretaria");
        db.ScoutLeaders.Add(new ScoutLeader { PersonId = second.Id });
        await db.SaveChangesAsync();
        using var assisted = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/secretariat/participants/{second.Id}/sessions/{session.Id}/enrollments");
        assisted.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token(secretary, "DISTRICT_SECRETARY"));
        var responses = await Task.WhenAll(client.SendAsync(EnrollRequest(session.Id, first)), client.SendAsync(assisted));
        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        Assert.Equal(1, await db.TrainingModuleEnrollments.CountAsync(e => e.TrainingModuleSessionId == session.Id));
        foreach (var response in responses) response.Dispose();
    }

    [Fact]
    public async Task TwoSelfServiceRequestsForTheLastSeatLeaveOneEnrollment()
    {
        var session = await CreateSessionAsync(capacity: 1);
        var first = await CreatePersonAsync("9100001", "Ana");
        var second = await CreatePersonAsync("9100002", "Beto");

        var responses = await Task.WhenAll(
            client.SendAsync(EnrollRequest(session.Id, first)),
            client.SendAsync(EnrollRequest(session.Id, second))
        );

        try
        {
            Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Created));
            Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Conflict));
            Assert.Equal(
                1,
                await db.TrainingModuleEnrollments.CountAsync(enrollment =>
                    enrollment.TrainingModuleSessionId == session.Id
                )
            );
        }
        finally
        {
            foreach (var response in responses)
                response.Dispose();
        }
    }

    [Fact]
    public async Task DuplicateDoesNotConsumeASeat()
    {
        var session = await CreateSessionAsync(capacity: 2);
        var first = await CreatePersonAsync("9200001", "Carla");
        var second = await CreatePersonAsync("9200002", "Diego");

        var duplicates = await Task.WhenAll(
            client.SendAsync(EnrollRequest(session.Id, first)),
            client.SendAsync(EnrollRequest(session.Id, first))
        );
        try
        {
            Assert.Equal(1, duplicates.Count(response => response.StatusCode == HttpStatusCode.Created));
            Assert.Equal(1, duplicates.Count(response => response.StatusCode == HttpStatusCode.Conflict));
        }
        finally
        {
            foreach (var response in duplicates)
                response.Dispose();
        }

        using var secondResponse = await client.SendAsync(EnrollRequest(session.Id, second));
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.Equal(
            2,
            await db.TrainingModuleEnrollments.CountAsync(enrollment =>
                enrollment.TrainingModuleSessionId == session.Id
            )
        );
    }

    [Fact]
    public async Task ClosedRegistrationDoesNotConsumeASeat()
    {
        var session = await CreateSessionAsync(capacity: 1, isRegistrationOpen: false);
        var person = await CreatePersonAsync("9300001", "Elena");

        using var response = await client.SendAsync(EnrollRequest(session.Id, person));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Empty(
            await db.TrainingModuleEnrollments
                .Where(enrollment => enrollment.TrainingModuleSessionId == session.Id)
                .ToListAsync()
        );
    }

    [Fact]
    public async Task CancelledSessionDoesNotConsumeASeat()
    {
        var session = await CreateSessionAsync(capacity: 1, status: cancelled);
        var person = await CreatePersonAsync("9400001", "Fabián");

        using var response = await client.SendAsync(EnrollRequest(session.Id, person));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Empty(
            await db.TrainingModuleEnrollments
                .Where(enrollment => enrollment.TrainingModuleSessionId == session.Id)
                .ToListAsync()
        );
    }

    [Fact]
    public async Task SessionCapacityWinsOverTheFacilityCapacity()
    {
        var facility = new Facility
        {
            FacilityType = new FacilityType { Code = "TEST", Name = "Sede ficticia" },
            Name = "Sede ficticia",
            Capacity = 10,
        };
        db.Facilities.Add(facility);
        await db.SaveChangesAsync();
        var session = await CreateSessionAsync(capacity: 1, facilityId: facility.Id);
        var first = await CreatePersonAsync("9500001", "Gabriela");
        var second = await CreatePersonAsync("9500002", "Hugo");

        using var firstResponse = await client.SendAsync(EnrollRequest(session.Id, first));
        using var secondResponse = await client.SendAsync(EnrollRequest(session.Id, second));

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        Assert.Equal(
            1,
            await db.TrainingModuleEnrollments.CountAsync(enrollment =>
                enrollment.TrainingModuleSessionId == session.Id
            )
        );
    }

    private async Task<TrainingModuleSession> CreateSessionAsync(
        int capacity,
        bool isRegistrationOpen = true,
        TrainingModuleSessionStatus? status = null,
        long? facilityId = null
    )
    {
        var session = new TrainingModuleSession
        {
            TrainingModuleId = module.Id,
            TrainingModuleSessionStatusId = (status ?? open).Id,
            IsRegistrationOpen = isRegistrationOpen,
            Capacity = capacity,
            QrToken = Guid.NewGuid(),
            Schedules =
            [
                new TrainingModuleSchedule
                {
                    PartNumber = 1,
                    SessionDate = new DateOnly(2026, 12, 5),
                    StartTime = new TimeOnly(8, 30),
                    EndTime = new TimeOnly(12, 30),
                    FacilityId = facilityId,
                },
            ],
        };
        db.TrainingModuleSessions.Add(session);
        await db.SaveChangesAsync();
        return session;
    }

    private async Task<Person> CreatePersonAsync(string idCard, string firstName)
    {
        var person = new Person
        {
            IdCard = idCard,
            FirstName = firstName,
            FirstLastName = "Prueba",
            PersonStatusId = personStatus.Id,
        };
        db.Persons.Add(person);
        await db.SaveChangesAsync();
        var user = new User
        {
            PersonId = person.Id,
            UserStatusId = userStatus.Id,
            Username = idCard,
            Email = $"{idCard}@example.test",
            Password = "ficticia",
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        userIds[person.Id] = user.Id;
        return person;
    }

    private HttpRequestMessage EnrollRequest(long sessionId, Person person)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{Calendar}/{sessionId}/enrollments"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            Token(person)
        );
        return request;
    }

    private string Token(Person person, string role = "SCOUT_LEADER")
    {
        var token = new JwtSecurityToken(
            claims:
            [
                new Claim("sub", userIds[person.Id].ToString()),
                new Claim("personId", person.Id.ToString()),
                new Claim("roles", role),
            ],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(Key, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task DisposeAsync()
    {
        client?.Dispose();
        if (app is not null)
            await app.DisposeAsync();
        if (db is not null)
            await db.DisposeAsync();
    }
}
