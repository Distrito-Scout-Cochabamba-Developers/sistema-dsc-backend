using DSC.Application.Commands;
using DSC.Application.Queries;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Infrastructure.Persistence;
using DSC.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DSC.UnitTests.Application;

public class TrainingModuleCatalogHandlerTests
{
    private const short ModuleTypeId = 1;

    private static ScoutDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ScoutDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ScoutDbContext(options);
        dbContext.Database.EnsureCreated();
        dbContext.ModuleTypes.Add(
            new ModuleType
            {
                Id = ModuleTypeId,
                Code = "PJ",
                Name = "Programa de Jóvenes",
            }
        );
        dbContext.SaveChanges();
        // Simula una petición nueva: nada queda en el change tracker
        dbContext.ChangeTracker.Clear();
        return dbContext;
    }

    private static async Task<TrainingModule> SeedModuleAsync(
        ScoutDbContext db,
        string code,
        string name,
        bool isActive = true
    )
    {
        var module = TrainingModule.Create(code, name, null, ModuleTypeId);
        module.IsActive = isActive;
        db.TrainingModules.Add(module);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return module;
    }

    [Fact]
    public async Task Create_WithValidCommand_ShouldPersistActiveModule()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var handler = new CreateTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        // Act
        var result = await handler.HandleAsync(
            new CreateTrainingModuleCommand(" MOD-01 ", " Liderazgo ", "Desc", ModuleTypeId),
            CancellationToken.None
        );

        // Assert
        Assert.True(result.Id > 0);
        Assert.Equal("MOD-01", result.Code);
        Assert.Equal("Liderazgo", result.Name);
        Assert.Equal("PJ", result.ModuleTypeCode);
        Assert.True(result.IsActive);

        var stored = await db.TrainingModules.AsNoTracking().SingleAsync();
        Assert.True(stored.IsActive);
    }

    [Fact]
    public async Task Create_WithDuplicateActiveName_ShouldThrowConflict()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        await SeedModuleAsync(db, "MOD-01", "Liderazgo");
        var handler = new CreateTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(
                new CreateTrainingModuleCommand("MOD-02", "  LIDERAZGO ", null, ModuleTypeId),
                CancellationToken.None
            )
        );
        Assert.Equal(1, await db.TrainingModules.CountAsync());
    }

    [Fact]
    public async Task Create_WithNameOfInactiveModule_ShouldBeAllowed()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        await SeedModuleAsync(db, "MOD-01", "Liderazgo", isActive: false);
        var handler = new CreateTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        // Act
        var result = await handler.HandleAsync(
            new CreateTrainingModuleCommand("MOD-02", "Liderazgo", null, ModuleTypeId),
            CancellationToken.None
        );

        // Assert
        Assert.Equal("MOD-02", result.Code);
        Assert.Equal(2, await db.TrainingModules.CountAsync());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Create_WithDuplicateCode_ShouldThrowConflictEvenIfInactive(bool isActive)
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        await SeedModuleAsync(db, "MOD-01", "Liderazgo", isActive);
        var handler = new CreateTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(
                new CreateTrainingModuleCommand(" mod-01 ", "Otro nombre", null, ModuleTypeId),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Create_WithUnknownModuleType_ShouldThrowDomainException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var handler = new CreateTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(
                new CreateTrainingModuleCommand("MOD-01", "Liderazgo", null, 99),
                CancellationToken.None
            )
        );
        Assert.Equal(0, await db.TrainingModules.CountAsync());
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var handler = new CreateTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(
                new CreateTrainingModuleCommand("MOD-01", " ", null, ModuleTypeId),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Update_WithValidCommand_ShouldPersistChanges()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var existing = await SeedModuleAsync(db, "MOD-01", "Liderazgo");
        var handler = new UpdateTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        // Act
        var result = await handler.HandleAsync(
            new UpdateTrainingModuleCommand(
                existing.Id,
                "MOD-01B",
                "Liderazgo II",
                "Nueva descripción",
                ModuleTypeId,
                IsActive: false
            ),
            CancellationToken.None
        );

        // Assert
        Assert.Equal("MOD-01B", result.Code);
        Assert.False(result.IsActive);

        var stored = await db.TrainingModules.AsNoTracking().SingleAsync();
        Assert.Equal("Liderazgo II", stored.Name);
        Assert.Equal("Nueva descripción", stored.Description);
        Assert.False(stored.IsActive);
    }

    [Fact]
    public async Task Update_KeepingOwnNameAndCode_ShouldNotBeDuplicate()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var existing = await SeedModuleAsync(db, "MOD-01", "Liderazgo");
        var handler = new UpdateTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        // Act
        var result = await handler.HandleAsync(
            new UpdateTrainingModuleCommand(
                existing.Id,
                "MOD-01",
                "Liderazgo",
                "Solo cambia la descripción",
                ModuleTypeId,
                IsActive: true
            ),
            CancellationToken.None
        );

        // Assert
        Assert.Equal("Solo cambia la descripción", result.Description);
    }

    [Fact]
    public async Task Update_WithNameOfAnotherActiveModule_ShouldThrowConflict()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        await SeedModuleAsync(db, "MOD-01", "Liderazgo");
        var target = await SeedModuleAsync(db, "MOD-02", "Gestión");
        var handler = new UpdateTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(
                new UpdateTrainingModuleCommand(
                    target.Id,
                    "MOD-02",
                    "liderazgo",
                    null,
                    ModuleTypeId,
                    IsActive: true
                ),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Update_DeactivatingWithNameOfAnotherActiveModule_ShouldBeAllowed()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        await SeedModuleAsync(db, "MOD-01", "Liderazgo");
        var target = await SeedModuleAsync(db, "MOD-02", "Gestión");
        var handler = new UpdateTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        // Act
        var result = await handler.HandleAsync(
            new UpdateTrainingModuleCommand(
                target.Id,
                "MOD-02",
                "Liderazgo",
                null,
                ModuleTypeId,
                IsActive: false
            ),
            CancellationToken.None
        );

        // Assert
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task Update_WithCodeOfAnotherModule_ShouldThrowConflict()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        await SeedModuleAsync(db, "MOD-01", "Liderazgo", isActive: false);
        var target = await SeedModuleAsync(db, "MOD-02", "Gestión");
        var handler = new UpdateTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(
                new UpdateTrainingModuleCommand(
                    target.Id,
                    "MOD-01",
                    "Gestión",
                    null,
                    ModuleTypeId,
                    IsActive: true
                ),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Update_WithUnknownId_ShouldThrowKeyNotFound()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var handler = new UpdateTrainingModuleCommandHandler(new TrainingModuleRepository(db));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(
                new UpdateTrainingModuleCommand(
                    404,
                    "MOD-01",
                    "Liderazgo",
                    null,
                    ModuleTypeId,
                    IsActive: true
                ),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task List_ShouldReturnOrderedByNameAndFilterActive()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        await SeedModuleAsync(db, "MOD-B", "Zeta");
        await SeedModuleAsync(db, "MOD-A", "Alfa");
        await SeedModuleAsync(db, "MOD-C", "Medio", isActive: false);
        var handler = new ListTrainingModulesQueryHandler(new TrainingModuleRepository(db));

        // Act
        var all = await handler.HandleAsync(new ListTrainingModulesQuery(), CancellationToken.None);
        var active = await handler.HandleAsync(
            new ListTrainingModulesQuery(OnlyActive: true),
            CancellationToken.None
        );

        // Assert
        Assert.Equal(["Alfa", "Medio", "Zeta"], all.Select(m => m.Name));
        Assert.Equal(["Alfa", "Zeta"], active.Select(m => m.Name));
        Assert.All(active, m => Assert.True(m.IsActive));
        Assert.Empty(db.ChangeTracker.Entries());
    }
}
