using DSC.UnitTests.Application;
using Microsoft.EntityFrameworkCore;

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
    public async Task MigrationRejectsDuplicatePersonCiWithoutExtension()
    {
        await using var db = await postgres.CreateDatabase(seed: false);
        await db.Database.ExecuteSqlRawAsync("INSERT INTO person (id_card, first_name, first_last_name, person_status_id) VALUES ('9100061','A','B',1)");
        var ex = await Assert.ThrowsAsync<Npgsql.PostgresException>(() =>
            db.Database.ExecuteSqlRawAsync("INSERT INTO person (id_card, first_name, first_last_name, person_status_id) VALUES ('9100061','C','D',1)"));
        Assert.Equal("ux_person_ci_extension", ex.ConstraintName);
    }
}
