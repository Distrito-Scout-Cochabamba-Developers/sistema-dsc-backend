using DSC.Domain.Entities;
using DSC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace DSC.UnitTests.Infrastructure;

public class TrainingLineModelTests
{
    [Fact]
    public void Model_ShouldMapTrainingLineWithUniqueCode()
    {
        using var db = new ScoutDbContextFactory().CreateDbContext([]);
        var line = db.Model.FindEntityType(typeof(TrainingLine));
        Assert.NotNull(line);
        Assert.Equal("training_line", line.GetTableName());
        Assert.Contains(
            line.GetIndexes(),
            index =>
                index.IsUnique
                && index
                    .Properties.Select(property => property.Name)
                    .SequenceEqual([nameof(TrainingLine.Code)])
        );
    }

    [Fact]
    public void Model_ShouldRequireLevelLineWithoutCascadingItsDeletion()
    {
        using var db = new ScoutDbContextFactory().CreateDbContext([]);
        var level = db.Model.FindEntityType(typeof(TrainingLevel));
        Assert.NotNull(level);
        Assert.Equal("training_level", level.GetTableName());
        Assert.False(level.FindProperty(nameof(TrainingLevel.TrainingLineId))!.IsNullable);
        var relationship = Assert.Single(
            level.GetForeignKeys(),
            foreignKey =>
                foreignKey
                    .Properties.Select(property => property.Name)
                    .SequenceEqual([nameof(TrainingLevel.TrainingLineId)])
        );
        Assert.Equal(typeof(TrainingLine), relationship.PrincipalEntityType.ClrType);
        Assert.Equal(
            [nameof(TrainingLine.Id)],
            relationship.PrincipalKey.Properties.Select(p => p.Name)
        );
        Assert.Equal(nameof(TrainingLevel.TrainingLine), relationship.DependentToPrincipal?.Name);
        Assert.Equal(nameof(TrainingLine.TrainingLevels), relationship.PrincipalToDependent?.Name);
        Assert.True(relationship.IsRequired);
        Assert.False(relationship.IsUnique);
        Assert.Equal(DeleteBehavior.Restrict, relationship.DeleteBehavior);
    }

    [Fact]
    public void Model_ShouldMapMembershipKeyAndBothForeignKeyTargets()
    {
        using var db = new ScoutDbContextFactory().CreateDbContext([]);
        var membership = db.Model.FindEntityType(typeof(TrainingModuleLine));
        Assert.NotNull(membership);
        Assert.Equal("training_module_line", membership.GetTableName());
        Assert.Equal(
            new[] { "TrainingModuleId", "TrainingLineId" },
            membership.FindPrimaryKey()!.Properties.Select(property => property.Name)
        );
        Assert.Equal(2, membership.GetForeignKeys().Count());
        var moduleRelationship = Assert.Single(
            membership.GetForeignKeys(),
            foreignKey =>
                foreignKey
                    .Properties.Select(property => property.Name)
                    .SequenceEqual([nameof(TrainingModuleLine.TrainingModuleId)])
        );
        Assert.Equal(typeof(TrainingModule), moduleRelationship.PrincipalEntityType.ClrType);
        Assert.Equal(
            [nameof(TrainingModule.Id)],
            moduleRelationship.PrincipalKey.Properties.Select(p => p.Name)
        );
        Assert.True(moduleRelationship.IsRequired);
        Assert.Equal(DeleteBehavior.Cascade, moduleRelationship.DeleteBehavior);
        var lineRelationship = Assert.Single(
            membership.GetForeignKeys(),
            foreignKey =>
                foreignKey
                    .Properties.Select(property => property.Name)
                    .SequenceEqual([nameof(TrainingModuleLine.TrainingLineId)])
        );
        Assert.Equal(typeof(TrainingLine), lineRelationship.PrincipalEntityType.ClrType);
        Assert.Equal(
            [nameof(TrainingLine.Id)],
            lineRelationship.PrincipalKey.Properties.Select(p => p.Name)
        );
        Assert.True(lineRelationship.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, lineRelationship.DeleteBehavior);
        Assert.Null(db.Model.FindEntityType(typeof(TrainingModule))!.FindProperty("ModuleTypeId"));
    }

    [Fact]
    public void MigrationSnapshot_ShouldMatchModel()
    {
        using var db = new ScoutDbContextFactory().CreateDbContext([]);
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public void MigrationSql_ShouldCopyExistingMembershipsBeforeDroppingSourceColumn()
    {
        using var db = new ScoutDbContextFactory().CreateDbContext([]);
        var sql = db.GetService<IMigrator>()
            .GenerateScript(
                "20260826031552_InitialMigration",
                "20261111050951_IntroduceTrainingLines"
            );
        Assert.Contains("RENAME TO training_line", sql);
        Assert.DoesNotContain("DROP TABLE module_type", sql);
        const string copy =
            "INSERT INTO training_module_line (training_module_id, training_line_id)\nSELECT id, module_type_id FROM training_module;";
        const string drop = "ALTER TABLE training_module DROP COLUMN module_type_id;";
        var normalizedSql = sql.Replace("\r\n", "\n");
        Assert.Contains(copy, normalizedSql);
        Assert.Contains(drop, normalizedSql);
        Assert.True(
            normalizedSql.IndexOf(copy, StringComparison.Ordinal)
                < normalizedSql.IndexOf(drop, StringComparison.Ordinal)
        );
    }

    [Fact]
    public void LegacyMigrationChain_ShouldKeepExistingIdsAndInitialSchema()
    {
        using var db = new ScoutDbContextFactory().CreateDbContext([]);
        var migrations = db.GetService<IMigrationsAssembly>();
        Assert.Equal(
            new[] { "20260826031552_InitialMigration", "20261111050951_IntroduceTrainingLines" },
            migrations.Migrations.Keys.OrderBy(id => id, StringComparer.Ordinal).Take(2)
        );
        var initial = migrations.CreateMigration(
            migrations.Migrations["20260826031552_InitialMigration"],
            db.Database.ProviderName!
        );
        var tables = initial.UpOperations.OfType<CreateTableOperation>().ToArray();
        Assert.Contains(tables, table => table.Name == "module_type");
        Assert.DoesNotContain(tables, table => table.Name == "training_line");
        Assert.DoesNotContain(tables, table => table.Name == "training_module_line");
        var module = Assert.Single(tables, table => table.Name == "training_module");
        Assert.Contains(
            module.Columns,
            column => column.Name == "module_type_id" && !column.IsNullable
        );
        var level = Assert.Single(tables, table => table.Name == "training_level");
        Assert.Contains(
            level.Columns,
            column => column.Name == "module_type_id" && column.IsNullable
        );
        Assert.Contains(
            level.ForeignKeys,
            foreignKey =>
                foreignKey.PrincipalTable == "module_type"
                && foreignKey.Columns.SequenceEqual(["module_type_id"])
        );
    }

    [Theory]
    [InlineData(
        "20260826031552_InitialMigration",
        "20261111050951_IntroduceTrainingLines",
        "ALTER INDEX \"IX_module_type_code\" RENAME TO \"IX_training_line_code\";",
        "ALTER INDEX \"IX_training_level_module_type_id\" RENAME TO \"IX_training_level_training_line_id\";"
    )]
    [InlineData(
        "20261111050951_IntroduceTrainingLines",
        "20260826031552_InitialMigration",
        "ALTER INDEX \"IX_training_line_code\" RENAME TO \"IX_module_type_code\";",
        "ALTER INDEX \"IX_training_level_training_line_id\" RENAME TO \"IX_training_level_module_type_id\";"
    )]
    public void MigrationSql_ShouldRenameIndexesToIndexNames(
        string from,
        string to,
        string lineIndexRename,
        string levelIndexRename
    )
    {
        using var db = new ScoutDbContextFactory().CreateDbContext([]);
        var sql = db.GetService<IMigrator>().GenerateScript(from, to);
        Assert.Contains(lineIndexRename, sql);
        Assert.Contains(levelIndexRename, sql);
    }

    [Fact]
    public void Persistence_ShouldUseRequiredEfCore10()
    {
        Assert.Equal(10, typeof(DbContext).Assembly.GetName().Version!.Major);
    }
}
