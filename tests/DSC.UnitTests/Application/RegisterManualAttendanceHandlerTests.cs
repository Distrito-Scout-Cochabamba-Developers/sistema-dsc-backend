using DSC.Application.Commands;
using DSC.Application.DTOs;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Infrastructure.Persistence;
using DSC.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DSC.UnitTests.Application;

public class RegisterManualAttendanceHandlerTests
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
  public async Task HandleAsync_WithValidData_ShouldCreatePersonAndAttendance()
  {
    using var db = CreateInMemoryDbContext();

    var sampleModule = new TrainingModule { Code = "MOD-001", Name = "Módulo de Prueba", ModuleTypeId = 1 };
    db.TrainingModules.Add(sampleModule);
    await db.SaveChangesAsync();

    var moduleRepo = new TrainingModuleRepository(db);
    var leaderRepo = new ScoutLeaderRepository(db);
    var attendanceRepo = new ModuleAttendanceRepository(db);

    var handler = new RegisterManualAttendanceCommandHandler(moduleRepo, leaderRepo, attendanceRepo);

    var request = new RegisterManualAttendanceDto(
        TrainingModuleId: sampleModule.Id,
        CiNumber: "1234567",
        Extension: "LP",
        FirstName: "María",
        LastName: "López",
        Phone: "77788899"
    );

    var result = await handler.HandleAsync(request, CancellationToken.None);

    Assert.NotNull(result);
    Assert.Equal("María López", result.ScoutLeaderFullName);
    Assert.Equal("1234567-LP", result.CiNumber);
    Assert.True(result.IsPartialRegistration);

    var savedPerson = await db.Persons.FirstOrDefaultAsync(p => p.IdCard == "1234567");
    Assert.NotNull(savedPerson);

    var savedAttendance = await db.ModuleAttendances.FirstOrDefaultAsync(a => a.TrainingModuleId == sampleModule.Id);
    Assert.NotNull(savedAttendance);
  }

  [Fact]
  public async Task HandleAsync_WithInvalidModuleId_ShouldThrowKeyNotFoundException()
  {
    using var db = CreateInMemoryDbContext();
    var handler = new RegisterManualAttendanceCommandHandler(
        new TrainingModuleRepository(db),
        new ScoutLeaderRepository(db),
        new ModuleAttendanceRepository(db));

    var request = new RegisterManualAttendanceDto(
        TrainingModuleId: 999, // ID que no existe
        CiNumber: "1234567",
        Extension: "LP",
        FirstName: "María",
        LastName: "López",
        Phone: "77788899"
    );

    var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
        handler.HandleAsync(request, CancellationToken.None));

    Assert.Contains("No se encontró ningún módulo activo", exception.Message);
  }

  [Fact]
  public async Task HandleAsync_WithInvalidCi_ShouldThrowDomainException()
  {
    using var db = CreateInMemoryDbContext();

    var sampleModule = new TrainingModule { Code = "MOD-001", Name = "Módulo de Prueba", ModuleTypeId = 1 };
    db.TrainingModules.Add(sampleModule);
    await db.SaveChangesAsync();

    var handler = new RegisterManualAttendanceCommandHandler(
        new TrainingModuleRepository(db),
        new ScoutLeaderRepository(db),
        new ModuleAttendanceRepository(db));

    var request = new RegisterManualAttendanceDto(
        TrainingModuleId: sampleModule.Id,
        CiNumber: "1234567A", // CI Inválido (letras)
        Extension: "LP",
        FirstName: "María",
        LastName: "López",
        Phone: "77788899"
    );

    var exception = await Assert.ThrowsAsync<DomainException>(() =>
        handler.HandleAsync(request, CancellationToken.None));

    Assert.Contains("exclusivamente numérico", exception.Message);
  }
}