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

        // GetByCodeAsync includes the module's training lines.
        dbContext.TrainingLines.Add(new TrainingLine(1, "AE", "Adjuntos/Asesores de Formación"));
        // Ids deliberadamente distintos de los históricos (1, 2, 1): el handler debe resolver por código.
        dbContext.PersonStatuses.Add(new PersonStatus { Id = 7, Code = "ACTIVE", Name = "Activa" });
        dbContext.ContactMechanismTypes.Add(new ContactMechanismType { Id = 9, Code = "CELLPHONE", Name = "Celular" });
        dbContext.ContactPurposeTypes.Add(new ContactPurposeType { Id = 5, Code = "PERSONAL", Name = "Personal" });
        dbContext.SaveChanges();

        return dbContext;
    }

    [Fact]
    public async Task RegisterAttendance_WithNewLeader_ShouldCreatePersonAndAttendance()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sampleModule = new TrainingModule
        {
            Id = 123,
            Name = "Módulo de Prueba",
            ModuleLines = [new TrainingModuleLine { TrainingLineId = 1 }],
        };
        db.TrainingModules.Add(sampleModule);
        await db.SaveChangesAsync();

        var moduleRepo = new TrainingModuleRepository(db);
        var leaderRepo = new ScoutLeaderRepository(db);
        var attendanceRepo = new ModuleAttendanceRepository(db);
        var handler = new RegisterDigitalAttendanceCommandHandler(
            moduleRepo,
            leaderRepo,
            attendanceRepo,
            new UnitOfWork(db)
        );

        var request = new RegisterDigitalAttendanceDto(
            SessionToken: sampleModule.Id.ToString(),
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
        Assert.Equal(7, savedPerson.PersonStatusId);
        var phone = Assert.Single(savedPerson.ContactMechanisms);
        Assert.Equal(9, phone.ContactMechanismTypeId);
        Assert.Equal(5, phone.ContactPurposeTypeId);

        var savedAttendance = await db.ModuleAttendances.FirstOrDefaultAsync(a =>
            a.TrainingModuleId == sampleModule.Id
        );
        Assert.NotNull(savedAttendance);
    }

    [Fact]
    public async Task RegisterAttendance_WithExistingLeaderWithoutPhone_AddsPhoneWithCatalogIds()
    {
        using var db = CreateInMemoryDbContext();
        db.TrainingModules.Add(new TrainingModule { Id = 321, Name = "M", ModuleLines = [new TrainingModuleLine { TrainingLineId = 1 }] });
        db.Persons.Add(new Person { IdCard = "4040404", FirstName = "A", FirstLastName = "B", PersonStatusId = 7 });
        await db.SaveChangesAsync();
        var handler = new RegisterDigitalAttendanceCommandHandler(
            new TrainingModuleRepository(db), new ScoutLeaderRepository(db), new ModuleAttendanceRepository(db), new UnitOfWork(db));

        await handler.HandleAsync(new RegisterDigitalAttendanceDto("321", "4040404", "", "A", "B", "70000001"), CancellationToken.None);

        var mechanism = await db.PersonContactMechanisms.SingleAsync();
        Assert.Equal(9, mechanism.ContactMechanismTypeId);
        Assert.Equal(5, mechanism.ContactPurposeTypeId);
    }

    [Fact]
    public async Task RegisterAttendance_WithoutRequiredCatalog_FailsWithoutCreatingPersonOrAttendance()
    {
        using var db = CreateInMemoryDbContext();
        db.PersonStatuses.RemoveRange(db.PersonStatuses);
        db.TrainingModules.Add(new TrainingModule { Id = 322, Name = "M", ModuleLines = [new TrainingModuleLine { TrainingLineId = 1 }] });
        await db.SaveChangesAsync();
        var handler = new RegisterDigitalAttendanceCommandHandler(
            new TrainingModuleRepository(db), new ScoutLeaderRepository(db), new ModuleAttendanceRepository(db), new UnitOfWork(db));

        await Assert.ThrowsAsync<CatalogMissingException>(() =>
            handler.HandleAsync(new RegisterDigitalAttendanceDto("322", "4040405", "", "A", "B", "70000002"), CancellationToken.None));

        Assert.Empty(db.Persons);
        Assert.Empty(db.ModuleAttendances);
    }

    [Fact]
    public async Task RegisterAttendance_WithDuplicateAttendance_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sampleModule = new TrainingModule
        {
            Id = 456,
            Name = "Módulo de Prueba",
            ModuleLines = [new TrainingModuleLine { TrainingLineId = 1 }],
        };
        db.TrainingModules.Add(sampleModule);
        await db.SaveChangesAsync();

        var moduleRepo = new TrainingModuleRepository(db);
        var leaderRepo = new ScoutLeaderRepository(db);
        var attendanceRepo = new ModuleAttendanceRepository(db);
        var handler = new RegisterDigitalAttendanceCommandHandler(
            moduleRepo,
            leaderRepo,
            attendanceRepo,
            new UnitOfWork(db)
        );

        var request = new RegisterDigitalAttendanceDto(
            SessionToken: sampleModule.Id.ToString(),
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
            attendanceRepo,
            new UnitOfWork(db)
        );

        var request = new RegisterDigitalAttendanceDto(
            SessionToken: "999999",
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
            Id = 789,
            Name = "Módulo de Prueba",
            ModuleLines = [new TrainingModuleLine { TrainingLineId = 1 }],
        };
        db.TrainingModules.Add(sampleModule);
        await db.SaveChangesAsync();

        var moduleRepo = new TrainingModuleRepository(db);
        var leaderRepo = new ScoutLeaderRepository(db);
        var attendanceRepo = new ModuleAttendanceRepository(db);
        var handler = new RegisterDigitalAttendanceCommandHandler(
            moduleRepo,
            leaderRepo,
            attendanceRepo,
            new UnitOfWork(db)
        );

        var request = new RegisterDigitalAttendanceDto(
            SessionToken: sampleModule.Id.ToString(),
            CiNumber: "7654321A", // Invalid alphabetic Carnet de Identidad
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

    [Theory]
    [InlineData("", "Pérez")]
    [InlineData("Juan", "")]
    [InlineData("   ", "   ")]
    public async Task RegisterAttendance_WithMissingFullName_ShouldThrowDomainException(
        string firstName,
        string lastName
    )
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var sampleModule = new TrainingModule
        {
            Id = 101,
            Name = "Módulo de Prueba",
            ModuleLines = [new TrainingModuleLine { TrainingLineId = 1 }],
        };
        db.TrainingModules.Add(sampleModule);
        await db.SaveChangesAsync();

        var moduleRepo = new TrainingModuleRepository(db);
        var leaderRepo = new ScoutLeaderRepository(db);
        var attendanceRepo = new ModuleAttendanceRepository(db);
        var handler = new RegisterDigitalAttendanceCommandHandler(
            moduleRepo,
            leaderRepo,
            attendanceRepo,
            new UnitOfWork(db)
        );

        var request = new RegisterDigitalAttendanceDto(
            SessionToken: sampleModule.Id.ToString(),
            CiNumber: "7654321",
            Extension: "CB",
            FirstName: firstName,
            LastName: lastName,
            Phone: "77712345"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(request, CancellationToken.None)
        );

        Assert.Contains("nombre completo", exception.Message);
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

    [Fact]
    public async Task LookupLeaderByCi_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var leaderRepo = new ScoutLeaderRepository(db);
        var handler = new LookupLeaderByCiQueryHandler(leaderRepo);

        // Act
        var result = await handler.HandleAsync(
            new LookupLeaderByCiQuery("9999999", "LP"),
            CancellationToken.None
        );

        // Assert
        Assert.Null(result);
    }
}
