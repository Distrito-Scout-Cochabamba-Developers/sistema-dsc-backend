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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DSC.UnitTests.Api;

[Collection("Training requirements PostgreSQL")]
public class CalendarCapacityHttpTests(RequirementsPostgres postgres) : IAsyncLifetime
{
    private const string Calendar = "/api/v1/calendar/sessions";
    private const string Create = "/api/v1/admin/modules/1/sessions";
    private static readonly SymmetricSecurityKey Key = new(new byte[32]);
    private ScoutDbContext db = null!;
    private WebApplication app = null!;
    private HttpClient client = null!;
    private Facility facility = null!;

    public async Task InitializeAsync()
    {
        db = await postgres.CreateDatabase();
        facility = new Facility
        {
            FacilityType = new FacilityType { Code = "TEST", Name = "Sede ficticia" },
            Name = "Sede ficticia",
            Capacity = 20,
        };
        db.Facilities.Add(facility);
        await db.SaveChangesAsync();

        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" }
        );
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
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
    public async Task Calendar_PublishesIndependentCapacitiesForTheSameFacility()
    {
        Authenticate("ADMIN");
        await CreateSession(3);
        await CreateSession(8);

        facility.Capacity = 50;
        await db.SaveChangesAsync();
        client.DefaultRequestHeaders.Authorization = null;

        var sessions = (await client.GetFromJsonAsync<CalendarSessionDto[]>(Calendar))!;
        Assert.Equal(new int?[] { 3, 8 }, sessions.Select(session => session.Capacity));
        Assert.Equal(new int?[] { 3, 8 }, sessions.Select(session => session.RemainingSeats));
        Assert.All(sessions, session => Assert.Equal("Sede ficticia", session.Location));
    }

    private object Schedule() =>
        new
        {
            sessionDate = "2026-11-28",
            startTime = "08:30:00",
            endTime = "12:30:00",
            facilityId = facility.Id,
        };

    [Fact]
    public async Task OnlyAdminCanExpandAnExistingConvocatoria()
    {
        Authenticate("ADMIN");
        var session = await CreateSession(3);
        var path = $"/api/v1/admin/sessions/{session.Id}/capacity";

        Authenticate("DISTRICT_SECRETARY");
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await client.PutAsJsonAsync(path, new { capacity = 8 })).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await client.PutAsJsonAsync(path, new { capacity = (int?)null })).StatusCode
        );
        Assert.Equal(
            3,
            (await client.GetFromJsonAsync<CalendarSessionDto[]>(Calendar))!.Single().Capacity
        );

        Authenticate("ADMIN");
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.PutAsJsonAsync(path, new { capacity = 8 })).StatusCode
        );
        Assert.Equal(
            8,
            (await client.GetFromJsonAsync<CalendarSessionDto[]>(Calendar))!.Single().Capacity
        );
        Assert.Equal(
            HttpStatusCode.Conflict,
            (await client.PutAsJsonAsync(path, new { capacity = 7 })).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.PutAsJsonAsync(path, new { })).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.PutAsJsonAsync(path, new { capacity = (int?)null })).StatusCode
        );
        Assert.Null(
            (await client.GetFromJsonAsync<CalendarSessionDto[]>(Calendar))!.Single().RemainingSeats
        );
    }

    [Fact]
    public async Task SecretariatCanCreateButCannotRaiseTheDefaultOrExplicitlyRemoveTheLimit()
    {
        Authenticate("DISTRICT_SECRETARY");
        var schedules = new[] { Schedule() };
        Assert.Equal(
            HttpStatusCode.Created,
            (await client.PostAsJsonAsync(Create, new { schedules })).StatusCode
        );
        await CreateSession(7);
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (await client.PostAsJsonAsync(Create, new { schedules, capacity = 21 })).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.Forbidden,
            (
                await client.PostAsJsonAsync(Create, new { schedules, capacity = (int?)null })
            ).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.PostAsJsonAsync(Create, new { schedules, capacity = -1 })).StatusCode
        );
        facility.Capacity = 50;
        await db.SaveChangesAsync();
        Assert.Equal(
            new int?[] { 20, 7 },
            (await client.GetFromJsonAsync<CalendarSessionDto[]>(Calendar))!.Select(session =>
                session.Capacity
            )
        );
    }

    [Fact]
    public async Task DeploymentSqlPreservesHistoricalLimitsFromTheFirstBlockWithAFacility()
    {
        const string previous = "20261113165822_RemoveTrainingModuleCodeAndAddNameUniqueIndex";
        await using var historical = await postgres.CreateDatabase(migrate: false, seed: false);
        var migrator = historical.GetService<IMigrator>();
        await migrator.MigrateAsync(previous);
        await historical.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO training_module (id, name) VALUES (1, 'Módulo ficticio');
            INSERT INTO training_module_session_status (id, code, name) VALUES (1, 'OPEN', 'Abierta');
            INSERT INTO facility_type (id, code, name) VALUES (1, 'TEST', 'Ficticia');
            INSERT INTO facility (id, facility_type_id, name, capacity)
            VALUES (1, 1, 'Sede A', 12), (2, 1, 'Sede B', 40), (3, 1, 'Sede C', NULL);
            INSERT INTO training_module_session (id, training_module_id, training_module_session_status_id)
            VALUES (1, 1, 1), (2, 1, 1), (3, 1, 1), (4, 1, 1);
            INSERT INTO training_module_schedule
                (training_module_session_id, part_number, session_date, start_time, end_time, facility_id)
            VALUES (1, 3, '2026-11-26', '08:00', '09:00', 2),
                   (1, 2, '2026-11-27', '08:00', '09:00', 1),
                   (1, 1, '2026-11-28', '08:00', '09:00', NULL),
                   (2, 1, '2026-11-27', '08:00', '09:00', 3),
                   (2, 2, '2026-11-28', '08:00', '09:00', 2),
                   (3, 1, '2026-11-28', '08:00', '09:00', NULL);
            """
        );

        var sql = migrator.GenerateScript(
            previous,
            options: MigrationsSqlGenerationOptions.Idempotent
        );
        await historical.Database.ExecuteSqlRawAsync(sql);
        await historical.Database.ExecuteSqlRawAsync("UPDATE facility SET capacity = 99");
        await historical.Database.ExecuteSqlRawAsync(sql);
        Assert.Equal(
            new int?[] { 12, null, null, null },
            await historical
                .TrainingModuleSessions.OrderBy(session => session.Id)
                .Select(session => session.Capacity)
                .ToArrayAsync()
        );
        Assert.Empty(await historical.Database.GetPendingMigrationsAsync());
    }

    [Fact]
    public async Task CompleteDeploymentSqlInitializesAnEmptyDatabase()
    {
        await using var empty = await postgres.CreateDatabase(migrate: false, seed: false);
        var sql = empty
            .GetService<IMigrator>()
            .GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);
        await empty.Database.ExecuteSqlRawAsync(sql);
        Assert.Empty(await empty.Database.GetPendingMigrationsAsync());
        Assert.Empty(
            await empty.TrainingModuleSessions.Select(session => session.Capacity).ToArrayAsync()
        );
    }

    private async Task<CalendarSessionDto> CreateSession(int capacity)
    {
        using var response = await client.PostAsJsonAsync(
            Create,
            new { schedules = new[] { Schedule() }, capacity }
        );
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CalendarSessionDto>())!;
    }

    private void Authenticate(string role)
    {
        var token = new JwtSecurityToken(
            claims: [new Claim("roles", role)],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(Key, SecurityAlgorithms.HmacSha256)
        );
        client.DefaultRequestHeaders.Authorization = new(
            "Bearer",
            new JwtSecurityTokenHandler().WriteToken(token)
        );
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
