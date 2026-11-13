using DSC.Application.Commands;
using DSC.Application.Queries;
using DSC.Domain.Entities;
using DSC.Infrastructure.Persistence;
using DSC.Infrastructure.Persistence.Repositories;
using DSC.UnitTests.Application;
using DSC.UnitTests.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace DSC.UnitTests.Infrastructure;

[Collection("Training requirements PostgreSQL")]
public class TrainingLevelRequirementModelTests(RequirementsPostgres postgres)
{
    [Fact]
    public void NewDatabase_StoresLevelsWithoutTrainingStages()
    {
        using var db = new ScoutDbContextFactory().CreateDbContext([]);
        Assert.DoesNotContain("training_stage", db.Database.GenerateCreateScript());
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task ConcurrentSelectionEdits_RejectStaleWriteInsteadOfMergingMandatoryModules()
    {
        await using var first = await postgres.CreateDatabase();
        var repository = new TrainingLevelRepository(first);
        await new ConfigureTrainingLevelRequirementsCommandHandler(repository).HandleAsync(
            ScoutTrainingCatalog.LevelCode,
            new(12, 2, ScoutTrainingCatalog.MandatoryIds)
        );
        first.ChangeTracker.Clear();
        await using var second = postgres.Open(first.Database.GetConnectionString()!);
        var other = new TrainingLevelRepository(second);
        var current = (
            await repository.GetTrackedByCodeAsync(ScoutTrainingCatalog.LevelCode, default)
        )!;
        var stale = (await other.GetTrackedByCodeAsync(ScoutTrainingCatalog.LevelCode, default))!;

        current
            .Requirements.Single()
            .ConfigureCurricularModules(
                12,
                2,
                await repository.GetTrackedModulesByIdsAsync(
                    [3L, 4L],
                    default
                )
            );
        stale
            .Requirements.Single()
            .ConfigureCurricularModules(
                12,
                2,
                await other.GetTrackedModulesByIdsAsync(
                    [1L, 2L],
                    default
                )
            );
        await repository.SaveChangesAsync(default);
        await Assert.ThrowsAsync<InvalidOperationException>(() => other.SaveChangesAsync(default));

        await using var reader = postgres.Open(first.Database.GetConnectionString()!);
        var result = await new GetTrainingLevelRequirementsQueryHandler(
            new TrainingLevelRepository(reader)
        ).HandleAsync(ScoutTrainingCatalog.LevelCode);
        Assert.Equal(
            new long[] { 3, 4 },
            result.Modules!.MandatoryModules.Select(m => m.Id).Order()
        );
    }

    [Theory]
    [InlineData("duplicate-configuration", "23505")]
    [InlineData("duplicate-selection", "23505")]
    [InlineData("unknown-module", "23503")]
    [InlineData("referenced-module", "23503")]
    [InlineData("negative-total", "23514")]
    [InlineData("negative-minimum", "23514")]
    [InlineData("minimum-above-total", "23514")]
    public async Task PostgreSql_RejectsInvalidConfigurationAtStorageBoundary(
        string scenario,
        string sqlState
    )
    {
        await using var db = await postgres.CreateDatabase();
        await new ConfigureTrainingLevelRequirementsCommandHandler(
            new TrainingLevelRepository(db)
        ).HandleAsync(
            ScoutTrainingCatalog.LevelCode,
            new(12, 2, ScoutTrainingCatalog.MandatoryIds)
        );
        var sql = scenario switch
        {
            "duplicate-configuration" =>
                "INSERT INTO training_level_requirement (training_level_id, requirement_type, total_modules_count, min_mandatory_modules_count, version) VALUES (1, 'MODULES', 12, 2, gen_random_uuid())",
            "duplicate-selection" =>
                "INSERT INTO training_level_mandatory_module SELECT id, 3 FROM training_level_requirement",
            "unknown-module" =>
                "INSERT INTO training_level_mandatory_module SELECT id, 999 FROM training_level_requirement",
            "referenced-module" => "DELETE FROM training_module WHERE id = 3",
            "negative-total" => "UPDATE training_level_requirement SET total_modules_count = -1",
            "negative-minimum" =>
                "UPDATE training_level_requirement SET min_mandatory_modules_count = -1",
            "minimum-above-total" =>
                "UPDATE training_level_requirement SET min_mandatory_modules_count = 13",
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var error = await Assert.ThrowsAsync<PostgresException>(() =>
            db.Database.ExecuteSqlRawAsync(sql)
        );
        Assert.Equal(sqlState, error.SqlState);
        var result = await new GetTrainingLevelRequirementsQueryHandler(
            new TrainingLevelRepository(db)
        ).HandleAsync(ScoutTrainingCatalog.LevelCode);
        Assert.Equal(12, result.Modules!.TotalModulesCount);
        Assert.Equal(2, result.Modules.MinMandatoryModulesCount);
        Assert.Equal(4, result.Modules.MandatoryModules.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Migrations_CreateOrUpgradeWithoutLosingInsigniaDeMaderaConfiguration(
        bool upgrade
    )
    {
        await using var db = await postgres.CreateDatabase(migrate: false, seed: false);
        var migrator = db.GetService<IMigrator>();
        if (upgrade)
        {
            await migrator.MigrateAsync("20261113054215_AddTrainingLevelRequirements");
            await RequirementsPostgres.Seed(db);
            await new ConfigureTrainingLevelRequirementsCommandHandler(
                new TrainingLevelRepository(db)
            ).HandleAsync(
                ScoutTrainingCatalog.LevelCode,
                new(12, 2, ScoutTrainingCatalog.MandatoryIds)
            );
            await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO training_stage (id, code, name) VALUES (1, 'LEGACY', 'Clasificación retirada'); UPDATE training_level SET training_stage_id = 1"
            );
        }
        await migrator.MigrateAsync();
        if (!upgrade)
        {
            await RequirementsPostgres.Seed(db);
            await new ConfigureTrainingLevelRequirementsCommandHandler(
                new TrainingLevelRepository(db)
            ).HandleAsync(
                ScoutTrainingCatalog.LevelCode,
                new(12, 2, ScoutTrainingCatalog.MandatoryIds)
            );
        }
        db.ChangeTracker.Clear();
        var repository = new TrainingLevelRepository(db);
        var level = Assert.Single(await repository.ListAsync(default));
        Assert.Equal("Insignia de Madera I-II", level.Name);
        Assert.Equal("PJ", level.TrainingLine.Code);
        var result = await new GetTrainingLevelRequirementsQueryHandler(repository).HandleAsync(
            ScoutTrainingCatalog.LevelCode
        );
        Assert.Equal(12, result.Modules!.TotalModulesCount);
        Assert.Equal(2, result.Modules.MinMandatoryModulesCount);
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
        Assert.Null(
            await db
                .Database.SqlQueryRaw<string?>(
                    "SELECT to_regclass('public.training_stage')::text AS \"Value\""
                )
                .SingleAsync()
        );
        Assert.Equal(
            0,
            await db
                .Database.SqlQueryRaw<int>(
                    "SELECT count(*)::int AS \"Value\" FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'training_level' AND column_name = 'training_stage_id'"
                )
                .SingleAsync()
        );
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
    }

    [Fact]
    public async Task Reconfiguration_PreservesOtherCriteriaAndLeaderHistory()
    {
        await using var db = await postgres.CreateDatabase();
        // Datos históricos completos: las FK reales deben aceptar la fixture.
        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO person_status (id, code, name) VALUES (1, 'ACTIVE', 'Activo');
            INSERT INTO person (id, id_card, first_name, first_last_name, person_status_id)
                VALUES (1, 'FIXTURE-DSC', 'Dirigente', 'De prueba', 1);
            INSERT INTO scout_leader (person_id) VALUES (1);
            INSERT INTO leader_training_status (id, code, name) VALUES (1, 'APPROVED', 'Aprobado');
            INSERT INTO scout_leader_training_level (person_id, training_level_id, status_id, status_date, notes)
                VALUES (1, 1, 1, '2026-01-01T00:00:00Z', 'Historial conservado');
            INSERT INTO training_level_requirement (training_level_id, requirement_type, total_modules_count, min_mandatory_modules_count, version)
                VALUES (1, 'INTERVIEW', 0, 0, '00000000-0000-0000-0000-000000000001');
            """
        );
        var attendance = new ModuleAttendanceRepository(db);
        await attendance.AddAsync(ModuleAttendance.Create(3, 1), default);
        var before = await History(db);
        var handler = new ConfigureTrainingLevelRequirementsCommandHandler(
            new TrainingLevelRepository(db)
        );
        await handler.HandleAsync(
            ScoutTrainingCatalog.LevelCode,
            new(12, 2, ScoutTrainingCatalog.MandatoryIds)
        );
        await handler.HandleAsync(
            ScoutTrainingCatalog.LevelCode,
            new(12, 2, [3L, 4L])
        );
        await using var reader = postgres.Open(db.Database.GetConnectionString()!);
        Assert.Equal(before, await History(reader));
        Assert.True(await new ModuleAttendanceRepository(reader).ExistsAsync(3, 1, default));
    }

    private static Task<string> History(ScoutDbContext db) =>
        db
            .Database.SqlQueryRaw<string>(
                """
                SELECT json_build_object(
                    'levels', (SELECT json_agg(row_to_json(history)) FROM scout_leader_training_level history),
                    'otherCriteria', (SELECT json_agg(row_to_json(requirement)) FROM training_level_requirement requirement WHERE requirement_type <> 'MODULES')
                )::text AS "Value"
                """
            )
            .SingleAsync();
}
