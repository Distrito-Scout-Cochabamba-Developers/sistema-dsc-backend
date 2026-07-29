using DSC.Application.Commands;
using DSC.Application.DTOs;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Infrastructure.Persistence;
using DSC.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DSC.UnitTests.Application;

public class RegisterDigitalAttendanceHandlerTests
{
    private ScoutDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ScoutDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ScoutDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    [Fact]
    public async Task RegisterAttendance_WithNewAdult_ShouldCreatePartialAdultAndAttendance()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sampleModule = TrainingModule.Create("Módulo de Prueba", "TOKEN-123", DateTime.UtcNow);
        db.TrainingModules.Add(sampleModule);
        await db.SaveChangesAsync();

        var moduleRepo = new TrainingModuleRepository(db);
        var adultRepo = new ScoutAdultRepository(db);
        var attendanceRepo = new ModuleAttendanceRepository(db);
        var handler = new RegisterDigitalAttendanceCommandHandler(moduleRepo, adultRepo, attendanceRepo);

        var request = new RegisterDigitalAttendanceRequest(
            SessionToken: "TOKEN-123",
            CiNumber: "7654321",
            Extension: "CB",
            FirstName: "Juan",
            LastName: "Pérez",
            Phone: "77712345"
        );

        // Act
        var result = await handler.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Juan Pérez", result.ScoutAdultFullName);
        Assert.Equal("7654321-CB", result.CiNumber);
        Assert.True(result.IsPartialRegistration);

        var savedAdult = await db.ScoutAdults.FirstOrDefaultAsync(a => a.Ci.Number == "7654321");
        Assert.NotNull(savedAdult);
        Assert.True(savedAdult.IsPartialRegistration);

        var savedAttendance = await db.ModuleAttendances.FirstOrDefaultAsync(a => a.TrainingModuleId == sampleModule.Id);
        Assert.NotNull(savedAttendance);
    }

    [Fact]
    public async Task RegisterAttendance_WithDuplicateAttendance_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sampleModule = TrainingModule.Create("Módulo de Prueba", "TOKEN-DUPLICATE", DateTime.UtcNow);
        db.TrainingModules.Add(sampleModule);
        await db.SaveChangesAsync();

        var moduleRepo = new TrainingModuleRepository(db);
        var adultRepo = new ScoutAdultRepository(db);
        var attendanceRepo = new ModuleAttendanceRepository(db);
        var handler = new RegisterDigitalAttendanceCommandHandler(moduleRepo, adultRepo, attendanceRepo);

        var request = new RegisterDigitalAttendanceRequest(
            SessionToken: "TOKEN-DUPLICATE",
            CiNumber: "8888888",
            Extension: "LP",
            FirstName: "María",
            LastName: "Gómez",
            Phone: "66611122"
        );

        // Act 1: First registration succeeds
        await handler.HandleAsync(request, CancellationToken.None);

        // Act 2 & Assert: Second registration throws InvalidOperationException (409 Conflict)
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(request, CancellationToken.None));

        Assert.Contains("ya registró su asistencia", exception.Message);
    }

    [Fact]
    public async Task RegisterAttendance_WithNonExistentSessionToken_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var moduleRepo = new TrainingModuleRepository(db);
        var adultRepo = new ScoutAdultRepository(db);
        var attendanceRepo = new ModuleAttendanceRepository(db);
        var handler = new RegisterDigitalAttendanceCommandHandler(moduleRepo, adultRepo, attendanceRepo);

        var request = new RegisterDigitalAttendanceRequest(
            SessionToken: "TOKEN-INVALID",
            CiNumber: "7654321",
            Extension: "CB",
            FirstName: "Juan",
            LastName: "Pérez",
            Phone: "77712345"
        );

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAttendance_WithInvalidCi_ShouldThrowDomainException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sampleModule = TrainingModule.Create("Módulo de Prueba", "TOKEN-INVALID-CI", DateTime.UtcNow);
        db.TrainingModules.Add(sampleModule);
        await db.SaveChangesAsync();

        var moduleRepo = new TrainingModuleRepository(db);
        var adultRepo = new ScoutAdultRepository(db);
        var attendanceRepo = new ModuleAttendanceRepository(db);
        var handler = new RegisterDigitalAttendanceCommandHandler(moduleRepo, adultRepo, attendanceRepo);

        var request = new RegisterDigitalAttendanceRequest(
            SessionToken: "TOKEN-INVALID-CI",
            CiNumber: "7654321A", // Invalid alphabetic CI
            Extension: "CB",
            FirstName: "Juan",
            LastName: "Pérez",
            Phone: "77712345"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(request, CancellationToken.None));

        Assert.Contains("exclusivamente numérico", exception.Message);
    }
}
