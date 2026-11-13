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
            new TrainingLine(code, "Adjuntos/Asesores de Formación", "Formación de adultos")
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
            new WriteTrainingModuleDto("Módulo nuevo", "Desc", ["AE"])
        );

        Assert.True(result.Id > 0);
        Assert.Equal("Módulo nuevo", result.Name);
        Assert.Equal("Desc", result.Description);
        Assert.Equal("AE", Assert.Single(result.TrainingLines).Code);
        Assert.Equal("Adjuntos/Asesores de Formación", result.TrainingLines[0].Name);
    }

    [Fact]
    public async Task Create_DuplicateName_ShouldConflict()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);
        db.TrainingModules.Add(
            new TrainingModule
            {
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
            handler.HandleAsync(new WriteTrainingModuleDto(" Existente ", null, ["AE"]))
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
            handler.HandleAsync(new WriteTrainingModuleDto("Nuevo", null, ["ZZ"]))
        );
    }

    [Fact]
    public async Task Create_EmptyName_ShouldValidate()
    {
        using var db = CreateDb();
        var handler = new CreateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingLineRepository(db)
        );

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            handler.HandleAsync(new WriteTrainingModuleDto("  ", null, ["AE"]))
        );
    }

    [Fact]
    public async Task Update_ShouldChangeNameAndLines()
    {
        using var db = CreateDb();
        await SeedLineAsync(db, "AE");
        db.TrainingLines.Add(new TrainingLine("PJ", "Programa de Jóvenes"));
        await db.SaveChangesAsync();
        var aeId = db.TrainingLines.Single(t => t.Code == "AE").Id;
        var module = new TrainingModule
        {
            Name = "Viejo",
            Description = "A",
            ModuleLines = [new TrainingModuleLine { TrainingLineId = aeId }],
        };
        db.TrainingModules.Add(module);
        await db.SaveChangesAsync();

        var handler = new UpdateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingLineRepository(db)
        );

        var result = await handler.HandleAsync(
            new UpdateTrainingModuleCommand(
                module.Id,
                new WriteTrainingModuleDto("Nuevo nombre", "B", ["PJ"])
            )
        );

        Assert.Equal(module.Id, result.Id);
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
                    999,
                    new WriteTrainingModuleDto("Nombre", null, ["AE"])
                )
            )
        );
    }

    [Fact]
    public async Task Delete_ShouldRemoveModule()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);
        var module = new TrainingModule
        {
            Name = "Para borrar",
            ModuleLines =
            [
                new TrainingModuleLine { TrainingLineId = db.TrainingLines.Single().Id },
            ],
        };
        db.TrainingModules.Add(module);
        await db.SaveChangesAsync();

        var handler = new DeleteTrainingModuleCommandHandler(new TrainingModuleRepository(db));
        await handler.HandleAsync(new DeleteTrainingModuleCommand(module.Id));

        Assert.Null(await db.TrainingModules.SingleOrDefaultAsync(m => m.Id == module.Id));
        Assert.Empty(db.TrainingModuleLines);
    }

    [Fact]
    public async Task Delete_MissingModule_ShouldNotFound()
    {
        using var db = CreateDb();
        var handler = new DeleteTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(new DeleteTrainingModuleCommand(999))
        );
    }

    [Fact]
    public async Task Delete_WithAttendance_ShouldConflict()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);
        var module = new TrainingModule
        {
            Name = "Con asistencia",
            ModuleLines =
            [
                new TrainingModuleLine { TrainingLineId = db.TrainingLines.Single().Id },
            ],
        };
        db.TrainingModules.Add(module);
        await db.SaveChangesAsync();
        var moduleId = module.Id;
        db.ModuleAttendances.Add(ModuleAttendance.Create(moduleId, 1));
        await db.SaveChangesAsync();

        var handler = new DeleteTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new DeleteTrainingModuleCommand(moduleId))
        );
        Assert.NotNull(await db.TrainingModules.SingleOrDefaultAsync(m => m.Id == moduleId));
    }

    [Fact]
    public async Task Create_ShouldPersistMultipleLines()
    {
        using var db = CreateDb();
        await SeedLineAsync(db);
        db.TrainingLines.Add(new TrainingLine("PJ", "Programa de Jóvenes"));
        await db.SaveChangesAsync();
        var handler = new CreateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new TrainingLineRepository(db)
        );

        var result = await handler.HandleAsync(
            new WriteTrainingModuleDto("Módulo compartido", null, [" PJ ", "AE"])
        );

        Assert.Equal(new[] { "AE", "PJ" }, result.TrainingLines.Select(line => line.Code));
        db.ChangeTracker.Clear();
        var saved = await new TrainingModuleRepository(db).GetByIdAsync(result.Id, default);
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
                handler.HandleAsync(new WriteTrainingModuleDto("Nuevo", null, codes))
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
        var created = await new CreateTrainingModuleCommandHandler(repository, lines).HandleAsync(
            new WriteTrainingModuleDto("Compartido", null, ["AE"])
        );
        db.ChangeTracker.Clear();

        var result = await new UpdateTrainingModuleCommandHandler(repository, lines).HandleAsync(
            new UpdateTrainingModuleCommand(
                created.Id,
                new WriteTrainingModuleDto("Compartido", null, ["AE", "PJ"])
            )
        );

        Assert.Equal(new[] { "AE", "PJ" }, result.TrainingLines.Select(line => line.Code));
        db.ChangeTracker.Clear();
        var saved = await repository.GetByIdAsync(created.Id, default);
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
        var created = await new CreateTrainingModuleCommandHandler(repository, lines).HandleAsync(
            new WriteTrainingModuleDto("Original", "Descripción original", ["AE"])
        );
        var tracked = await repository.GetTrackedByIdAsync(created.Id, default);
        Assert.NotNull(tracked);
        var originalLink = Assert.Single(tracked.ModuleLines);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new UpdateTrainingModuleCommandHandler(repository, lines).HandleAsync(
                new UpdateTrainingModuleCommand(
                    created.Id,
                    new WriteTrainingModuleDto("Modificado", "Modificada", ["AE", "ZZ"])
                )
            )
        );

        Assert.Equal("Original", tracked.Name);
        Assert.Equal("Descripción original", tracked.Description);
        Assert.Same(originalLink, Assert.Single(tracked.ModuleLines));
        Assert.False(db.ChangeTracker.HasChanges());
        db.ChangeTracker.Clear();
        var saved = await repository.GetByIdAsync(created.Id, default);
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
