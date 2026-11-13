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
using DSC.Infrastructure;
using DSC.Infrastructure.Persistence;
using DSC.UnitTests.Application;
using DSC.UnitTests.Domain;
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

namespace DSC.UnitTests.Api;

[Collection("Training requirements PostgreSQL")]
public class TrainingLevelRequirementsHttpTests(RequirementsPostgres postgres) : IAsyncLifetime
{
    private const string Read = "/api/v1/training-levels/IM-I-II/requirements";
    private const string Write = "/api/v1/admin/training-levels/IM-I-II/requirements";
    private static readonly SymmetricSecurityKey Key = new(
        Encoding.UTF8.GetBytes("test-only-key-not-a-production-secret-1234567890")
    );
    private static WriteTrainingLevelRequirementsDto Valid =>
        new(12, 2, ScoutTrainingCatalog.MandatoryIds);
    private ScoutDbContext db = null!;
    private WebApplication app = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        db = await postgres.CreateDatabase();
        // Contrato HTTP aislado: usa PostgreSQL real, no ejecuta Program ni carga .env.
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions { EnvironmentName = "Testing" }
        );
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder
            .Services.AddControllers(options =>
                options.Conventions.Add(new GlobalRoutePrefixConvention(new RouteAttribute("api")))
            )
            .AddApplicationPart(typeof(TrainingLevelsController).Assembly);
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(db.Database.GetConnectionString()!);
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
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        var address = app
            .Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.Single();
        client = new HttpClient { BaseAddress = new Uri(address) };
    }

    private void Authenticate(string role, bool expired = false)
    {
        var token = new JwtSecurityToken(
            "test",
            "test",
            [new Claim("roles", role)],
            expires: DateTime.UtcNow.AddMinutes(expired ? -5 : 5),
            signingCredentials: new SigningCredentials(Key, SecurityAlgorithms.HmacSha256)
        );
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            new JwtSecurityTokenHandler().WriteToken(token)
        );
    }

    [Theory]
    [InlineData("ADMIN")]
    [InlineData("DISTRICT_SECRETARY")]
    public async Task AuthorizedWriter_ConfiguresInsigniaDeMaderaAndLeaderCanRead(string role)
    {
        Authenticate(role);
        using var response = await client.PutAsJsonAsync(Write, Valid);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Authenticate("SCOUT_LEADER");
        var result = (await client.GetFromJsonAsync<TrainingLevelRequirementsDto>(Read))!;
        Assert.Equal("IM-I-II", result.TrainingLevelCode);
        Assert.Equal(12, result.Modules!.TotalModulesCount);
        Assert.Equal(2, result.Modules.MinMandatoryModulesCount);
        Assert.Equal(
            new long[] { 1, 2, 3, 4 },
            result.Modules!.MandatoryModules.Select(m => m.Id).Order()
        );
        Assert.Equal(
            new[]
            {
                "Ciclo de programa",
                "Impacto ambiental en actividades Scouts",
                "Primeros auxilios",
                "Responsabilidad legal del dirigente",
            },
            result.Modules.MandatoryModules.Select(m => m.Name).Order()
        );
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("EXPIRED", HttpStatusCode.Unauthorized)]
    [InlineData("SCOUT_LEADER", HttpStatusCode.Forbidden)]
    public async Task UnauthorizedWriter_CannotConfigureInsigniaDeMadera(
        string? role,
        HttpStatusCode expected
    )
    {
        if (role is not null)
            Authenticate(role == "EXPIRED" ? "ADMIN" : role, expired: role == "EXPIRED");
        using var response = await client.PutAsJsonAsync(Write, Valid);
        Assert.Equal(expected, response.StatusCode);
        if (expected == HttpStatusCode.Unauthorized)
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(Read)).StatusCode);
        Authenticate("SCOUT_LEADER");
        Assert.Null((await client.GetFromJsonAsync<TrainingLevelRequirementsDto>(Read))!.Modules);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"totalModulesCount\":12,\"minMandatoryModulesCount\":2}")]
    [InlineData(
        "{\"totalModulesCount\":12,\"minMandatoryModulesCount\":2,\"mandatoryModuleIds\":null}"
    )]
    [InlineData(
        "{\"totalModulesCount\":12,\"minMandatoryModulesCount\":2,\"mandatoryModuleIds\":[null]}"
    )]
    [InlineData(
        "{\"totalModulesCount\":12,\"minMandatoryModulesCount\":2,\"mandatoryModuleIds\":[3, 3]}"
    )]
    [InlineData(
        "{\"totalModulesCount\":12,\"minMandatoryModulesCount\":2,\"mandatoryModuleIds\":[3, 9999]}"
    )]
    [InlineData(
        "{\"totalModulesCount\":-1,\"minMandatoryModulesCount\":0,\"mandatoryModuleIds\":[]}"
    )]
    [InlineData(
        "{\"totalModulesCount\":12,\"minMandatoryModulesCount\":-1,\"mandatoryModuleIds\":[]}"
    )]
    [InlineData(
        "{\"totalModulesCount\":1,\"minMandatoryModulesCount\":2,\"mandatoryModuleIds\":[3, 4]}"
    )]
    [InlineData(
        "{\"totalModulesCount\":12,\"minMandatoryModulesCount\":2,\"mandatoryModuleIds\":[3]}"
    )]
    [InlineData(
        "{\"totalModulesCount\":12.5,\"minMandatoryModulesCount\":2,\"mandatoryModuleIds\":[]}"
    )]
    public async Task InvalidJsonOrConfiguration_ReturnsProblemDetailsAndPreservesRequirements(
        string json
    )
    {
        Authenticate("ADMIN");
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(Write, Valid)).StatusCode);
        using var response = await client.PutAsync(
            Write,
            new StringContent(json, Encoding.UTF8, "application/json")
        );
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(400, (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Status);
        var modules = (await client.GetFromJsonAsync<TrainingLevelRequirementsDto>(Read))!.Modules!;
        Assert.Equal(12, modules.TotalModulesCount);
        Assert.Equal(2, modules.MinMandatoryModulesCount);
        Assert.Equal(new long[] { 1, 2, 3, 4 }, modules.MandatoryModules.Select(m => m.Id).Order());
        Assert.Equal(
            new[]
            {
                "Ciclo de programa",
                "Impacto ambiental en actividades Scouts",
                "Primeros auxilios",
                "Responsabilidad legal del dirigente",
            },
            modules.MandatoryModules.Select(m => m.Name).Order()
        );
    }

    [Fact]
    public async Task UnknownLevel_IsNotConfusedWithUnconfiguredInsigniaDeMadera()
    {
        Authenticate("ADMIN");
        Assert.Null((await client.GetFromJsonAsync<TrainingLevelRequirementsDto>(Read))!.Modules);
        var level = Assert.Single(
            (await client.GetFromJsonAsync<TrainingLevelDto[]>("/api/v1/training-levels"))!
        );
        Assert.Equal("IM-I-II", level.Code);
        Assert.Equal("Insignia de Madera I-II", level.Name);
        Assert.Equal("PJ", level.TrainingLineCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (
                await client.GetAsync("/api/v1/training-levels/NIVEL-INEXISTENTE/requirements")
            ).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NotFound,
            (
                await client.PutAsJsonAsync(
                    "/api/v1/admin/training-levels/NIVEL-INEXISTENTE/requirements",
                    Valid
                )
            ).StatusCode
        );
    }

    [Fact]
    public async Task ReferencedFirstAidModule_CannotBeDeletedUntilRemovedFromRequirements()
    {
        Authenticate("ADMIN");
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync(Write, Valid)).StatusCode);
        using var blocked = await client.DeleteAsync("/api/v1/admin/modules/3");
        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);
        Assert.Equal(409, (await blocked.Content.ReadFromJsonAsync<ProblemDetails>())!.Status);

        var replacement = new WriteTrainingLevelRequirementsDto(12, 2, [1L, 2L, 4L]);
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PutAsJsonAsync(Write, replacement)).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.PutAsJsonAsync(Write, replacement)).StatusCode
        );
        Assert.Equal(
            new long[] { 1, 2, 4 },
            (await client.GetFromJsonAsync<TrainingLevelRequirementsDto>(Read))!
                .Modules!.MandatoryModules.Select(m => m.Id)
                .Order()
        );
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync("/api/v1/admin/modules/3")).StatusCode
        );
    }

    public async Task DisposeAsync()
    {
        client?.Dispose();
        try
        {
            if (app is not null)
            {
                await app.StopAsync();
                await app.DisposeAsync();
            }
        }
        finally
        {
            if (db is not null)
                await db.DisposeAsync();
        }
    }
}
