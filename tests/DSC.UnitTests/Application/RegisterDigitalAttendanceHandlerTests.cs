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
    public async Task RegisterAttendance_WithNewLeader_ShouldCreatePersonAndAttendance()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sampleModule = new TrainingModule
        {
            Code = "TOKEN-123",
            Name = "Módulo de Prueba",
            ModuleTypeId = 1,
        };
        db.TrainingModules.Add(sampleModule);
        await db.SaveChangesAsync();

        var moduleRepo = new TrainingModuleRepository(db);
        var leaderRepo = new ScoutLeaderRepository(db);
        var attendanceRepo = new ModuleAttendanceRepository(db);
        var handler = new RegisterDigitalAttendanceCommandHandler(
            moduleRepo,
            leaderRepo,
            attendanceRepo
        );

        var request = new RegisterDigitalAttendanceDto(
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
        Assert.Equal("Juan Pérez", result.ScoutLeaderFullName);
        Assert.Equal("7654321-CB", result.CiNumber);
        Assert.True(result.IsPartialRegistration);

        var savedPerson = await db
            .Persons.Include(p => p.ContactMechanisms)
            .FirstOrDefaultAsync(a => a.IdCard == "7654321");
        Assert.NotNull(savedPerson);
        Assert.False(savedPerson.IsProfileComplete);
        Assert.Contains(savedPerson.ContactMechanisms, cm => cm.ContactValue == "77712345");

        var savedAttendance = await db.ModuleAttendances.FirstOrDefaultAsync(a =>
            a.TrainingModuleId == sampleModule.Id
        );
        Assert.NotNull(savedAttendance);
    }

    [Fact]
    public async Task RegisterAttendance_WithDuplicateAttendance_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sampleModule = new TrainingModule
        {
            Code = "TOKEN-DUPLICATE",
            Name = "Módulo de Prueba",
            ModuleTypeId = 1,
        };
        db.TrainingModules.Add(sampleModule);
        await db.SaveChangesAsync();

        var moduleRepo = new TrainingModuleRepository(db);
        var leaderRepo = new ScoutLeaderRepository(db);
        var attendanceRepo = new ModuleAttendanceRepository(db);
        var handler = new RegisterDigitalAttendanceCommandHandler(
            moduleRepo,
            leaderRepo,
            attendanceRepo
        );

        var request = new RegisterDigitalAttendanceDto(
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
            handler.HandleAsync(request, CancellationToken.None)
        );

        Assert.Contains("ya registró su asistencia", exception.Message);
    }

    [Fact]
    public async Task RegisterAttendance_WithNonExistentSessionToken_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var moduleRepo = new TrainingModuleRepository(db);
        var leaderRepo = new ScoutLeaderRepository(db);
        var attendanceRepo = new ModuleAttendanceRepository(db);
        var handler = new RegisterDigitalAttendanceCommandHandler(
            moduleRepo,
            leaderRepo,
            attendanceRepo
        );

        var request = new RegisterDigitalAttendanceDto(
            SessionToken: "TOKEN-INVALID",
            CiNumber: "7654321",
            Extension: "CB",
            FirstName: "Juan",
            LastName: "Pérez",
            Phone: "77712345"
        );

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.HandleAsync(request, CancellationToken.None)
        );
    }

    [Fact]
    public async Task RegisterAttendance_WithInvalidCi_ShouldThrowDomainException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sampleModule = new TrainingModule
        {
            Code = "TOKEN-INVALID-CI",
            Name = "Módulo de Prueba",
            ModuleTypeId = 1,
        };
        db.TrainingModules.Add(sampleModule);
        await db.SaveChangesAsync();

        var moduleRepo = new TrainingModuleRepository(db);
        var leaderRepo = new ScoutLeaderRepository(db);
        var attendanceRepo = new ModuleAttendanceRepository(db);
        var handler = new RegisterDigitalAttendanceCommandHandler(
            moduleRepo,
            leaderRepo,
            attendanceRepo
        );

        var request = new RegisterDigitalAttendanceDto(
            SessionToken: "TOKEN-INVALID-CI",
            CiNumber: "7654321A", // Invalid alphabetic CI
            Extension: "CB",
            FirstName: "Juan",
            LastName: "Pérez",
            Phone: "77712345"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(request, CancellationToken.None)
        );

        Assert.Contains("exclusivamente numérico", exception.Message);
    }

    [Fact]
    public async Task LookupLeaderByCi_WhenExists_ShouldReturnDto()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var person = new Person
        {
            IdCard = "5556667",
            IdCardExtension = "CB",
            FirstName = "Carlos",
            FirstLastName = "Montaño",
            PersonStatusId = 1,
            IsProfileComplete = true,
        };
        person.ContactMechanisms.Add(
            new PersonContactMechanism
            {
                ContactMechanismTypeId = 2,
                ContactPurposeTypeId = 1,
                ContactValue = "71234567",
                IsPrimary = true,
                ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            }
        );
        db.Persons.Add(person);
        await db.SaveChangesAsync();

        var leaderRepo = new ScoutLeaderRepository(db);
        var handler = new LookupLeaderByCiQueryHandler(leaderRepo);

        // Act
        var result = await handler.HandleAsync(
            new LookupLeaderByCiQuery("5556667", "CB"),
            CancellationToken.None
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Carlos", result.FirstName);
        Assert.Equal("Montaño", result.LastName);
        Assert.Equal("71234567", result.Phone);
        Assert.True(result.IsProfileComplete);
    }
}
