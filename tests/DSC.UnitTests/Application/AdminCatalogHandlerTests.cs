using DSC.Application.Commands;
using DSC.Application.DTOs;
using DSC.Application.Queries;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Infrastructure.Persistence;
using DSC.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DSC.UnitTests.Application;

public class AdminCatalogHandlerTests
{
    private static ScoutDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ScoutDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var db = new ScoutDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    private static async Task SeedLineAsync(ScoutDbContext db, string code = "AE")
    {
        db.TrainingLines.Add(
            new TrainingLine
            {
                Code = code,
                Name = "Adjuntos/Asesores de Formación",
                Description = "Formación de adultos",
            }
        );
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Create_ShouldPersistModule()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);

        var handler = new CreateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingLineRepository(db)
        );

        var result = await handler.HandleAsync(
            new WriteTrainingModuleDto("mod-nuevo", "Módulo nuevo", "Desc", ["AE"])
        );

        Assert.Equal("mod-nuevo", result.Code);
        Assert.Equal("Módulo nuevo", result.Name);
        Assert.Equal("AE", Assert.Single(result.TrainingLines).Code);
        Assert.Equal("Adjuntos/Asesores de Formación", result.TrainingLines[0].Name);
    }

    [Fact]
    public async Task Create_DuplicateCode_ShouldConflict()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);
        db.TrainingModules.Add(
            new TrainingModule
            {
                Code = "mod-nuevo",
                Name = "Existente",
                ModuleLines =
                [
                    new TrainingModuleLine { TrainingLineId = db.TrainingLines.Single().Id },
                ],
            }
        );
        await db.SaveChangesAsync();

        var handler = new CreateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingLineRepository(db)
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new WriteTrainingModuleDto("mod-nuevo", "Otro", null, ["AE"]))
        );
    }

    [Fact]
    public async Task Create_UnknownLine_ShouldNotFound()
    {
        using var db = CreateDb();
        var handler = new CreateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingLineRepository(db)
        );

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(new WriteTrainingModuleDto("mod-nuevo", "Nuevo", null, ["ZZ"]))
        );
    }

    [Fact]
    public async Task Create_EmptyCode_ShouldValidate()
    {
        using var db = CreateDb();
        var handler = new CreateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingLineRepository(db)
        );

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            handler.HandleAsync(new WriteTrainingModuleDto("  ", "Nuevo", null, ["AE"]))
        );
    }

    [Fact]
    public async Task Update_ShouldChangeNameAndLines()
    {
        using var db = CreateDb();
        await SeedLineAsync(db, "AE");
        db.TrainingLines.Add(new TrainingLine { Code = "PJ", Name = "Programa de Jóvenes" });
        await db.SaveChangesAsync();
        var aeId = db.TrainingLines.Single(t => t.Code == "AE").Id;
        db.TrainingModules.Add(
            new TrainingModule
            {
                Code = "mod-edit",
                Name = "Viejo",
                Description = "A",
                ModuleLines = [new TrainingModuleLine { TrainingLineId = aeId }],
            }
        );
        await db.SaveChangesAsync();

        var handler = new UpdateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingLineRepository(db)
        );

        var result = await handler.HandleAsync(
            new UpdateTrainingModuleCommand(
                "mod-edit",
                new WriteTrainingModuleDto("ignored", "Nuevo nombre", "B", ["PJ"])
            )
        );

        Assert.Equal("mod-edit", result.Code);
        Assert.Equal("Nuevo nombre", result.Name);
        Assert.Equal("B", result.Description);
        Assert.Equal("PJ", Assert.Single(result.TrainingLines).Code);
        Assert.Equal(
            db.TrainingLines.Single(line => line.Code == "PJ").Id,
            Assert.Single(db.TrainingModuleLines).TrainingLineId
        );
    }

    [Fact]
    public async Task Update_MissingModule_ShouldNotFound()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);
        var handler = new UpdateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingLineRepository(db)
        );

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(
                new UpdateTrainingModuleCommand(
                    "no-existe",
                    new WriteTrainingModuleDto("", "Nombre", null, ["AE"])
                )
            )
        );
    }

    [Fact]
    public async Task Delete_ShouldRemoveModule()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);
        db.TrainingModules.Add(
            new TrainingModule
            {
                Code = "mod-borrar",
                Name = "Para borrar",
                ModuleLines =
                [
                    new TrainingModuleLine { TrainingLineId = db.TrainingLines.Single().Id },
                ],
            }
        );
        await db.SaveChangesAsync();

        var handler = new DeleteTrainingModuleCommandHandler(new TrainingModuleRepository(db));
        await handler.HandleAsync(new DeleteTrainingModuleCommand("mod-borrar"));

        Assert.Null(await db.TrainingModules.SingleOrDefaultAsync(m => m.Code == "mod-borrar"));
        Assert.Empty(db.TrainingModuleLines);
    }

    [Fact]
    public async Task Delete_MissingModule_ShouldNotFound()
    {
        using var db = CreateDb();
        var handler = new DeleteTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(new DeleteTrainingModuleCommand("no-existe"))
        );
    }

    [Fact]
    public async Task Delete_WithAttendance_ShouldConflict()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);
        db.TrainingModules.Add(
            new TrainingModule
            {
                Code = "mod-usado",
                Name = "Con asistencia",
                ModuleLines =
                [
                    new TrainingModuleLine { TrainingLineId = db.TrainingLines.Single().Id },
                ],
            }
        );
        await db.SaveChangesAsync();
        var moduleId = db.TrainingModules.Single().Id;
        db.ModuleAttendances.Add(ModuleAttendance.Create(moduleId, 1));
        await db.SaveChangesAsync();

        var handler = new DeleteTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new DeleteTrainingModuleCommand("mod-usado"))
        );
        Assert.NotNull(await db.TrainingModules.SingleOrDefaultAsync(m => m.Code == "mod-usado"));
    }

    [Fact]
    public async Task Create_ShouldPersistMultipleLines()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);
        db.TrainingLines.Add(new TrainingLine { Code = "PJ", Name = "Programa de Jóvenes" });
        await db.SaveChangesAsync();
        var handler = new CreateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingLineRepository(db)
        );

        var result = await handler.HandleAsync(
            new WriteTrainingModuleDto("compartido", "Módulo compartido", null, [" PJ ", "AE"])
        );

        Assert.Equal(new[] { "AE", "PJ" }, result.TrainingLines.Select(line => line.Code));
        db.ChangeTracker.Clear();
        var saved = await new TrainingModuleRepository(db).GetByCodeAsync("compartido", default);
        Assert.NotNull(saved);
        Assert.Equal(
            new[] { "AE", "PJ" },
            saved.ModuleLines.Select(link => link.TrainingLine.Code).OrderBy(code => code)
        );
        Assert.Equal(2, db.TrainingLines.Count());
    }

    [Fact]
    public async Task Create_InvalidLines_ShouldNotPersist()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);
        var handler = new CreateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingLineRepository(db)
        );

        string[]?[] invalidCodes =
        [
            null,
            [],
            [""],
            [null!],
            ["AE", " AE "],
            ["ABCDEFGHIJKLMNOPQRSTU"],
        ];
        foreach (var codes in invalidCodes)
            await Assert.ThrowsAsync<RequestValidationException>(() =>
                handler.HandleAsync(new WriteTrainingModuleDto("nuevo", "Nuevo", null, codes))
            );

        Assert.Empty(db.TrainingModules);
    }

    [Fact]
    public async Task Update_ShouldRetainExistingLineAndAddAnother()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);
        await SeedLineAsync(db, "PJ");
        var repository = new TrainingModuleRepository(db);
        var lines = new TrainingLineRepository(db);
        await new CreateTrainingModuleCommandHandler(repository, lines).HandleAsync(
            new WriteTrainingModuleDto("compartido", "Compartido", null, ["AE"])
        );
        db.ChangeTracker.Clear();

        var result = await new UpdateTrainingModuleCommandHandler(repository, lines).HandleAsync(
            new UpdateTrainingModuleCommand(
                "compartido",
                new WriteTrainingModuleDto("", "Compartido", null, ["AE", "PJ"])
            )
        );

        Assert.Equal(new[] { "AE", "PJ" }, result.TrainingLines.Select(line => line.Code));
        db.ChangeTracker.Clear();
        var saved = await repository.GetByCodeAsync("compartido", default);
        Assert.NotNull(saved);
        Assert.Equal(
            new[] { "AE", "PJ" },
            saved.ModuleLines.Select(link => link.TrainingLine.Code).OrderBy(code => code)
        );
    }

    [Fact]
    public async Task Update_UnknownLine_ShouldLeaveModuleUnchanged()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);
        var repository = new TrainingModuleRepository(db);
        var lines = new TrainingLineRepository(db);
        await new CreateTrainingModuleCommandHandler(repository, lines).HandleAsync(
            new WriteTrainingModuleDto("existente", "Original", "Descripción original", ["AE"])
        );
        var tracked = await repository.GetTrackedByCodeAsync("existente", default);
        Assert.NotNull(tracked);
        var originalLink = Assert.Single(tracked.ModuleLines);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new UpdateTrainingModuleCommandHandler(repository, lines).HandleAsync(
                new UpdateTrainingModuleCommand(
                    "existente",
                    new WriteTrainingModuleDto("", "Modificado", "Modificada", ["AE", "ZZ"])
                )
            )
        );

        Assert.Equal("Original", tracked.Name);
        Assert.Equal("Descripción original", tracked.Description);
        Assert.Same(originalLink, Assert.Single(tracked.ModuleLines));
        Assert.False(db.ChangeTracker.HasChanges());
        db.ChangeTracker.Clear();
        var saved = await repository.GetByCodeAsync("existente", default);
        Assert.NotNull(saved);
        Assert.Equal("Original", saved.Name);
        Assert.Equal("Descripción original", saved.Description);
        Assert.Equal("AE", Assert.Single(saved.ModuleLines).TrainingLine.Code);
    }

    [Fact]
    public async Task ListLines_ShouldReturnSeededLines()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);
        var handler = new ListTrainingLinesQueryHandler(new TrainingLineRepository(db));
        var lines = await handler.HandleAsync();
        Assert.Single(lines);
        Assert.Equal("AE", lines[0].Code);
    }
}
