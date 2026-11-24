using DSC.UnitTests.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace DSC.UnitTests.Infrastructure;

/// <summary>Una instalación nueva debe contar con los catálogos que requieren ambos canales de alta.</summary>
[Collection("Training requirements PostgreSQL")]
public class SecretariatCatalogsMigrationTests(RequirementsPostgres postgres)
{
    [Fact]
    public async Task FreshInstallHasCatalogsWithIdentifiersUsedByDigitalAttendance()
    {
        await using var db = await postgres.CreateDatabase(seed: false);
        Assert.Equal(1, (await db.PersonStatuses.SingleAsync(x => x.Code == "ACTIVE")).Id);
        Assert.Equal(1, (await db.ContactPurposeTypes.SingleAsync(x => x.Code == "PERSONAL")).Id);
        Assert.Equal(2, (await db.ContactMechanismTypes.SingleAsync(x => x.Code == "CELLPHONE")).Id);
        Assert.True(await db.ContactMechanismTypes.AnyAsync(x => x.Code == "EMAIL"));
    }

    [Fact]
    public async Task BothAltaRepositoriesFailWithCatalogMissingWhenACatalogCodeIsAbsent()
    {
        await using var db = await postgres.CreateDatabase(seed: false);
        await db.Database.ExecuteSqlRawAsync("DELETE FROM contact_purpose_type WHERE code = 'PERSONAL'");
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        await Assert.ThrowsAsync<DSC.Domain.Exceptions.CatalogMissingException>(() =>
            new DSC.Infrastructure.Persistence.Repositories.RoverRegistrationRepository(db)
                .CreateAsync("9100062", null, "Ana", "Ficticia", "70000000", CancellationToken.None));
        await Assert.ThrowsAsync<DSC.Domain.Exceptions.CatalogMissingException>(() =>
            new DSC.Infrastructure.Persistence.Repositories.ScoutLeaderRepository(db)
                .CreateSecretariatLeaderAsync("9100063", null, "Ana", "Ficticia", "70000000", "ana@example.test", CancellationToken.None));
    }

    [Fact]
    public async Task MigrationRejectsDuplicatePersonCiWithoutExtension()
    {
        await using var db = await postgres.CreateDatabase(seed: false);
        await db.Database.ExecuteSqlRawAsync("INSERT INTO person (id_card, first_name, first_last_name, person_status_id) VALUES ('9100061','A','B',1)");
        var ex = await Assert.ThrowsAsync<Npgsql.PostgresException>(() =>
            db.Database.ExecuteSqlRawAsync("INSERT INTO person (id_card, first_name, first_last_name, person_status_id) VALUES ('9100061','C','D',1)"));
        Assert.Equal("ux_person_ci_extension", ex.ConstraintName);
    }

    private const string Previous = "20261124173738_AddRoverEnrollmentModality";
    private const string Target = "20261124180000_SecretariatCatalogs";

    private static async Task MigrateToPreviousAsync(DbContext db)
    {
        await db.GetService<IMigrator>().MigrateAsync(Previous);
        await db.Database.ExecuteSqlRawAsync("INSERT INTO person_status (code, name) VALUES ('TMP', 'Tmp') ON CONFLICT DO NOTHING");
    }

    [Fact]
    public async Task MigrationFailsWithClearMessageWhenPersonCiIsDuplicatedAndDoesNotPrintTheCi()
    {
        await using var db = await postgres.CreateDatabase(migrate: false, seed: false);
        await MigrateToPreviousAsync(db);
        await db.Database.ExecuteSqlRawAsync("INSERT INTO person (id_card, first_name, first_last_name, person_status_id) SELECT '9100077', n, 'B', id FROM person_status, (VALUES ('A'), ('C')) v(n) WHERE code = 'TMP'");

        var ex = await Assert.ThrowsAsync<PostgresException>(() => db.GetService<IMigrator>().MigrateAsync(Target));

        Assert.Contains("CI duplicados", ex.MessageText);
        Assert.DoesNotContain("9100077", ex.MessageText + ex.Detail + ex.Hint);
        Assert.False(await db.Database.SqlQueryRaw<int>("SELECT 1 AS \"Value\" FROM pg_indexes WHERE indexname = 'ux_person_ci_extension'").AnyAsync());
    }

    [Theory]
    [InlineData("CREATE INDEX ux_person_ci_extension ON person (id_card)")]
    [InlineData("CREATE UNIQUE INDEX ux_person_ci_extension ON person (id_card)")]
    public async Task MigrationFailsWhenAHomonymousIndexIsNotTheExpectedUniqueIndex(string createIndex)
    {
        await using var db = await postgres.CreateDatabase(migrate: false, seed: false);
        await MigrateToPreviousAsync(db);
        await db.Database.ExecuteSqlRawAsync(createIndex);

        var ex = await Assert.ThrowsAsync<PostgresException>(() => db.GetService<IMigrator>().MigrateAsync(Target));

        Assert.Contains("ux_person_ci_extension", ex.MessageText);
    }
}
