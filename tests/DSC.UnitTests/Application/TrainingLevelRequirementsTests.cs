using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using DSC.Application.Commands;
using DSC.Application.DTOs;
using DSC.Application.Queries;
using DSC.Domain.Entities;
using DSC.Domain.Enums;
using DSC.Domain.Exceptions;
using DSC.Infrastructure.Persistence;
using DSC.Infrastructure.Persistence.Repositories;
using DSC.UnitTests.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DSC.UnitTests.Application;

[Collection("Training requirements PostgreSQL")]
public class TrainingLevelRequirementsTests(RequirementsPostgres postgres)
{
    [Fact]
    public async Task ConfigureInsigniaDeMadera_ReadsTwelveTotalTwoMinimumAndFourEligibleModules()
    {
        await using var db = await postgres.CreateDatabase();
        await new ConfigureTrainingLevelRequirementsCommandHandler(
            new TrainingLevelRepository(db)
        ).HandleAsync(
            ScoutTrainingCatalog.LevelCode,
            new(12, 2, ScoutTrainingCatalog.MandatoryCodes)
        );

        await using var reader = postgres.Open(db.Database.GetConnectionString()!);
        var result = await new GetTrainingLevelRequirementsQueryHandler(
            new TrainingLevelRepository(reader)
        ).HandleAsync(ScoutTrainingCatalog.LevelCode);

        Assert.Equal(12, result.Modules!.TotalModulesCount);
        Assert.Equal(2, result.Modules.MinMandatoryModulesCount);
        Assert.Equal(
            new[]
            {
                "CICLO-PROGRAMA",
                "IMPACTO-AMBIENTAL",
                "PRIMEROS-AUXILIOS",
                "RESPONSABILIDAD-LEGAL",
            },
            result.Modules.MandatoryModuleCodes
        );
    }

    public static IEnumerable<object?[]> InvalidConfigurations =>
        [
            [null],
            [new WriteTrainingLevelRequirementsDto(null, 2, ScoutTrainingCatalog.MandatoryCodes)],
            [new WriteTrainingLevelRequirementsDto(12, null, ScoutTrainingCatalog.MandatoryCodes)],
            [new WriteTrainingLevelRequirementsDto(12, 2, null)],
            [new WriteTrainingLevelRequirementsDto(12, 2, [null!])],
            [new WriteTrainingLevelRequirementsDto(12, 2, [" "])],
            [new WriteTrainingLevelRequirementsDto(12, 2, [new string('X', 31)])],
            [
                new WriteTrainingLevelRequirementsDto(
                    12,
                    2,
                    ["PRIMEROS-AUXILIOS", " PRIMEROS-AUXILIOS "]
                ),
            ],
            [
                new WriteTrainingLevelRequirementsDto(
                    12,
                    2,
                    ["PRIMEROS-AUXILIOS", "MODULO-INEXISTENTE"]
                ),
            ],
            [new WriteTrainingLevelRequirementsDto(12, 2, ["PRIMEROS-AUXILIOS", "ciclo-programa"])],
        ];

    [Fact]
    public async Task ReplaceSelection_TrimsCodesAndRepeatedWritesDoNotAccumulateModules()
    {
        await using var db = await postgres.CreateDatabase();
        var handler = new ConfigureTrainingLevelRequirementsCommandHandler(
            new TrainingLevelRepository(db)
        );
        await handler.HandleAsync(
            ScoutTrainingCatalog.LevelCode,
            new(12, 2, ScoutTrainingCatalog.MandatoryCodes)
        );
        // Cambio administrativo de configuración, no evaluación de aprobaciones individuales.
        var replacement = new WriteTrainingLevelRequirementsDto(
            12,
            2,
            [" PRIMEROS-AUXILIOS ", "CICLO-PROGRAMA"]
        );
        await handler.HandleAsync(ScoutTrainingCatalog.LevelCode, replacement);
        await handler.HandleAsync(ScoutTrainingCatalog.LevelCode, replacement);

        await using var reader = postgres.Open(db.Database.GetConnectionString()!);
        var result = await new GetTrainingLevelRequirementsQueryHandler(
            new TrainingLevelRepository(reader)
        ).HandleAsync(ScoutTrainingCatalog.LevelCode);
        Assert.Equal(
            new[] { "CICLO-PROGRAMA", "PRIMEROS-AUXILIOS" },
            result.Modules!.MandatoryModuleCodes
        );
        Assert.Equal(12, result.Modules.TotalModulesCount);
        Assert.Equal(2, result.Modules.MinMandatoryModulesCount);
    }

    [Fact]
    public async Task ClearModuleCriterion_ReturnsExplicitZeroConfigurationRatherThanUnconfiguredLevel()
    {
        await using var db = await postgres.CreateDatabase();
        var handler = new ConfigureTrainingLevelRequirementsCommandHandler(
            new TrainingLevelRepository(db)
        );
        await handler.HandleAsync(
            ScoutTrainingCatalog.LevelCode,
            new(12, 2, ScoutTrainingCatalog.MandatoryCodes)
        );
        await handler.HandleAsync(ScoutTrainingCatalog.LevelCode, new(0, 0, []));
        await using var reader = postgres.Open(db.Database.GetConnectionString()!);
        var modules = (
            await new GetTrainingLevelRequirementsQueryHandler(
                new TrainingLevelRepository(reader)
            ).HandleAsync(ScoutTrainingCatalog.LevelCode)
        ).Modules!;
        Assert.NotNull(modules);
        Assert.Equal(0, modules.TotalModulesCount);
        Assert.Equal(0, modules.MinMandatoryModulesCount);
        Assert.Empty(modules.MandatoryModuleCodes);
    }

    [Theory]
    [MemberData(nameof(InvalidConfigurations))]
    public async Task InvalidConfiguration_DoesNotReplaceExistingInsigniaDeMaderaRequirements(
        WriteTrainingLevelRequirementsDto request
    )
    {
        await using var db = await postgres.CreateDatabase();
        var handler = new ConfigureTrainingLevelRequirementsCommandHandler(
            new TrainingLevelRepository(db)
        );
        await handler.HandleAsync(
            ScoutTrainingCatalog.LevelCode,
            new(12, 2, ScoutTrainingCatalog.MandatoryCodes)
        );

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            handler.HandleAsync(ScoutTrainingCatalog.LevelCode, request)
        );

        await using var reader = postgres.Open(db.Database.GetConnectionString()!);
        var modules = (
            await new GetTrainingLevelRequirementsQueryHandler(
                new TrainingLevelRepository(reader)
            ).HandleAsync(ScoutTrainingCatalog.LevelCode)
        ).Modules!;
        Assert.Equal(12, modules.TotalModulesCount);
        Assert.Equal(2, modules.MinMandatoryModulesCount);
        Assert.Equal(
            new[]
            {
                "CICLO-PROGRAMA",
                "IMPACTO-AMBIENTAL",
                "PRIMEROS-AUXILIOS",
                "RESPONSABILIDAD-LEGAL",
            },
            modules.MandatoryModuleCodes
        );
    }
}

[CollectionDefinition("Training requirements PostgreSQL")]
public class RequirementsPostgresCollection : ICollectionFixture<RequirementsPostgres>;

// Un clúster por colección; una base independiente por caso. Nunca carga .env ni Program.
public sealed class RequirementsPostgres : IDisposable
{
    private readonly string directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Temp",
        "opencode",
        "dsc-requirements-" + Guid.NewGuid().ToString("N")
    );
    private readonly string bin =
        Environment.GetEnvironmentVariable("DSC_TEST_PG_BIN")
        ?? @"C:\Program Files\PostgreSQL\17\bin";
    private readonly string connection;
    private bool started;

    public RequirementsPostgres()
    {
        Directory.CreateDirectory(directory);
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        Run(
            "initdb",
            "-D",
            Path.Combine(directory, "data"),
            "-U",
            "postgres",
            "--auth=trust",
            "--encoding=UTF8",
            "--no-locale"
        );
        Run(
            "pg_ctl",
            "-D",
            Path.Combine(directory, "data"),
            "-l",
            Path.Combine(directory, "postgres.log"),
            "-o",
            $"-h 127.0.0.1 -p {port}",
            "-w",
            "start"
        );
        started = true;
        connection =
            $"Host=127.0.0.1;Port={port};Username=postgres;Database=postgres;Pooling=false;Timeout=10";
    }

    private void Run(string name, params string[] arguments)
    {
        var info = new ProcessStartInfo(Path.Combine(bin, name + ".exe"))
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
            info.ArgumentList.Add(argument);
        using var process =
            Process.Start(info) ?? throw new InvalidOperationException($"Cannot start {name}");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(60_000))
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"{name} timed out. Artifacts: {directory}");
        }
        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"{name} failed: {output.GetAwaiter().GetResult()} {error.GetAwaiter().GetResult()}"
            );
    }

    internal ScoutDbContext Open(string connectionString) =>
        new(
            new DbContextOptionsBuilder<ScoutDbContext>()
                .UseNpgsql(connectionString, options => options.MapEnum<GenderType>("gender_type"))
                .Options
        );

    internal async Task<ScoutDbContext> CreateDatabase(bool migrate = true, bool seed = true)
    {
        var name = "requirements_" + Guid.NewGuid().ToString("N");
        await using (var admin = new NpgsqlConnection(connection))
        {
            await admin.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE {name}", admin);
            await command.ExecuteNonQueryAsync();
        }
        var db = Open(
            new NpgsqlConnectionStringBuilder(connection) { Database = name }.ConnectionString
        );
        try
        {
            if (migrate)
                await db.Database.MigrateAsync();
            if (seed)
                await Seed(db);
            return db;
        }
        catch
        {
            await db.DisposeAsync();
            throw;
        }
    }

    internal static async Task Seed(ScoutDbContext db)
    {
        var line = new TrainingLine
        {
            Id = 1,
            Code = "PJ",
            Name = "Programa de Jóvenes",
        };
        db.TrainingLevels.Add(
            new TrainingLevel
            {
                Id = 1,
                Code = ScoutTrainingCatalog.LevelCode,
                Name = "Insignia de Madera I-II",
                TrainingLine = line,
            }
        );
        var modules = ScoutTrainingCatalog
            .Mandatory()
            .Concat(ScoutTrainingCatalog.Voluntary())
            .ToArray();
        db.TrainingModules.AddRange(modules);
        db.TrainingModuleLines.AddRange(
            modules.Select(module => new TrainingModuleLine
            {
                TrainingModule = module,
                TrainingLine = line,
            })
        );
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        if (started)
        {
            Run("pg_ctl", "-D", Path.Combine(directory, "data"), "-m", "fast", "-w", "stop");
            started = false;
        }
        Console.WriteLine($"PostgreSQL stopped. Local test artifacts: {directory}");
    }
}
