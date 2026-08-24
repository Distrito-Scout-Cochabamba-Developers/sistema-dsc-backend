namespace DSC.Application.Commands;

using DSC.Application.DTOs;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using DSC.Domain.ValueObjects;

public class RegisterManualAttendanceCommandHandler(
    ITrainingModuleRepository moduleRepository,
    IScoutLeaderRepository leaderRepository,
    IModuleAttendanceRepository attendanceRepository)
{
  public async Task<ModuleAttendanceResultDto> HandleAsync(
      RegisterManualAttendanceDto request,
      CancellationToken cancellationToken)
  {
    // 1. Buscar el módulo por ID (Diferencia principal con el registro digital)
    var module = await moduleRepository.GetByIdAsync(request.TrainingModuleId, cancellationToken)
        ?? throw new KeyNotFoundException($"No se encontró ningún módulo activo con el ID '{request.TrainingModuleId}'.");

    // 2. Validar el CI usando el Value Object del Dominio
    var ci = new CiNumber(request.CiNumber, request.Extension);

    // 3. Buscar si la persona ya existe, si no, crear un perfil parcial
    var person = await leaderRepository.GetByCiNumberAsync(ci.Number, cancellationToken);

    if (person == null)
    {
      person = new Person
      {
        IdCard = ci.Number,
        IdCardExtension = string.IsNullOrWhiteSpace(ci.Extension) ? null : ci.Extension,
        FirstName = request.FirstName.Trim(),
        FirstLastName = request.LastName.Trim(),
        PersonStatusId = 1, // Activo
        IsProfileComplete = false,
        ScoutLeader = new ScoutLeader { IsSpeakerEligible = false }
      };

      if (!string.IsNullOrWhiteSpace(request.Phone))
      {
        person.ContactMechanisms.Add(new PersonContactMechanism
        {
          ContactMechanismTypeId = 2, // Celular
          ContactPurposeTypeId = 1,   // Personal
          ContactValue = request.Phone.Trim(),
          IsPrimary = true,
          ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow)
        });
      }
      await leaderRepository.AddAsync(person, cancellationToken);
    }

    // 4. Evitar que la secretaría registre dos veces a la misma persona
    var alreadyRegistered = await attendanceRepository.ExistsAsync(module.Id, person.Id, cancellationToken);
    if (alreadyRegistered)
      throw new InvalidOperationException($"El dirigente con CI '{ci}' ya registró su asistencia.");

    // 5. Crear el registro de asistencia
    var attendance = ModuleAttendance.Create(module.Id, person.Id);
    await attendanceRepository.AddAsync(attendance, cancellationToken);

    // Devolver el DTO de resultado que ya existe
    return new ModuleAttendanceResultDto(
        attendance.Id,
        module.Id,
        module.Name,
        person.Id,
        person.FullName,
        ci.FullValue,
        attendance.RegisteredAt,
        !person.IsProfileComplete
    );
  }
}
