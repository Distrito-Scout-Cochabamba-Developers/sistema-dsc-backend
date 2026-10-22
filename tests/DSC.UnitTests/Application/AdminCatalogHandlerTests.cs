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

    private static async Task SeedTypeAsync(ScoutDbContext db, string code = "AE")
    {
        db.ModuleTypes.Add(
            new ModuleType
            {
                Code = code,
                Name = "Adultos en el Escultismo",
                Description = "Formación de adultos",
            }
        );
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Create_ShouldPersistModule()
    {
        using var db = CreateDb();
        await SeedTypeAsync(db);

        var handler = new CreateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new ModuleTypeRepository(db)
        );

        var result = await handler.HandleAsync(
            new WriteTrainingModuleDto("mod-nuevo", "Módulo nuevo", "Desc", "AE")
        );

        Assert.Equal("mod-nuevo", result.Code);
        Assert.Equal("Módulo nuevo", result.Name);
        Assert.Equal("AE", result.ModuleTypeCode);
        Assert.Equal("Adultos en el Escultismo", result.ModuleTypeName);
    }

    [Fact]
    public async Task Create_DuplicateCode_ShouldConflict()
    {
        using var db = CreateDb();
        await SeedTypeAsync(db);
        db.TrainingModules.Add(
            new TrainingModule
            {
                Code = "mod-nuevo",
                Name = "Existente",
                ModuleTypeId = db.ModuleTypes.Single().Id,
            }
        );
        await db.SaveChangesAsync();

        var handler = new CreateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new ModuleTypeRepository(db)
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new WriteTrainingModuleDto("mod-nuevo", "Otro", null, "AE"))
        );
    }

    [Fact]
    public async Task Create_UnknownType_ShouldNotFound()
    {
        using var db = CreateDb();
        var handler = new CreateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new ModuleTypeRepository(db)
        );

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(new WriteTrainingModuleDto("mod-nuevo", "Nuevo", null, "ZZ"))
        );
    }

    [Fact]
    public async Task Create_EmptyCode_ShouldValidate()
    {
        using var db = CreateDb();
        var handler = new CreateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new ModuleTypeRepository(db)
        );

        await Assert.ThrowsAsync<RequestValidationException>(() =>
            handler.HandleAsync(new WriteTrainingModuleDto("  ", "Nuevo", null, "AE"))
        );
    }

    [Fact]
    public async Task Update_ShouldChangeNameAndType()
    {
        using var db = CreateDb();
        await SeedTypeAsync(db, "AE");
        db.ModuleTypes.Add(new ModuleType { Code = "PJ", Name = "Programa de Jóvenes" });
        await db.SaveChangesAsync();
        var aeId = db.ModuleTypes.Single(t => t.Code == "AE").Id;
        db.TrainingModules.Add(
            new TrainingModule
            {
                Code = "mod-edit",
                Name = "Viejo",
                Description = "A",
                ModuleTypeId = aeId,
            }
        );
        await db.SaveChangesAsync();

        var handler = new UpdateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new ModuleTypeRepository(db)
        );

        var result = await handler.HandleAsync(
            new UpdateTrainingModuleCommand(
                "mod-edit",
                new WriteTrainingModuleDto("ignored", "Nuevo nombre", "B", "PJ")
            )
        );

        Assert.Equal("mod-edit", result.Code);
        Assert.Equal("Nuevo nombre", result.Name);
        Assert.Equal("B", result.Description);
        Assert.Equal("PJ", result.ModuleTypeCode);
    }

    [Fact]
    public async Task Update_MissingModule_ShouldNotFound()
    {
        using var db = CreateDb();
        await SeedTypeAsync(db);
        var handler = new UpdateTrainingModuleCommandHandler(
            new TrainingModuleRepository(db),
            new ModuleTypeRepository(db)
        );

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(
                new UpdateTrainingModuleCommand(
                    "no-existe",
                    new WriteTrainingModuleDto("", "Nombre", null, "AE")
                )
            )
        );
    }

    [Fact]
    public async Task Delete_ShouldRemoveModule()
    {
        using var db = CreateDb();
        await SeedTypeAsync(db);
        db.TrainingModules.Add(
            new TrainingModule
            {
                Code = "mod-borrar",
                Name = "Para borrar",
                ModuleTypeId = db.ModuleTypes.Single().Id,
            }
        );
        await db.SaveChangesAsync();

        var handler = new DeleteTrainingModuleCommandHandler(new TrainingModuleRepository(db));
        await handler.HandleAsync(new DeleteTrainingModuleCommand("mod-borrar"));

        Assert.Null(await db.TrainingModules.SingleOrDefaultAsync(m => m.Code == "mod-borrar"));
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
        await SeedTypeAsync(db);
        db.TrainingModules.Add(
            new TrainingModule
            {
                Code = "mod-usado",
                Name = "Con asistencia",
                ModuleTypeId = db.ModuleTypes.Single().Id,
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
    public async Task ListTypes_ShouldReturnSeededTypes()
    {
        using var db = CreateDb();
        await SeedTypeAsync(db);
        var handler = new ListModuleTypesQueryHandler(new ModuleTypeRepository(db));
        var types = await handler.HandleAsync();
        Assert.Single(types);
        Assert.Equal("AE", types[0].Code);
    }
}
