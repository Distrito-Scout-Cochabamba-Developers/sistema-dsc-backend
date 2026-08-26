using DSC.Application.DTOs;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using DSC.Domain.ValueObjects;

namespace DSC.Application.Commands;

public class RegisterDigitalAttendanceCommandHandler(
    ITrainingModuleRepository moduleRepository,
    IScoutLeaderRepository leaderRepository,
    IModuleAttendanceRepository attendanceRepository
)
{
    public async Task<ModuleAttendanceResultDto> HandleAsync(
        RegisterDigitalAttendanceDto request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.SessionToken))
            throw new DomainException("El token de sesión no puede estar vacío.");

        // 1. Verify training module existence by code
        var module =
            await moduleRepository.GetByCodeAsync(request.SessionToken, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"No se encontró ningún módulo de capacitación activo con el código '{request.SessionToken}'."
            );

        // 2. Validate CI Value Object
        var ci = new CiNumber(request.CiNumber, request.Extension);

        // 3. Find or Create Person / Scout Leader
        var person = await leaderRepository.GetByCiNumberAsync(ci.Number, cancellationToken);
        if (person == null)
        {
            person = new Person
            {
                IdCard = ci.Number,
                IdCardExtension = string.IsNullOrWhiteSpace(ci.Extension) ? null : ci.Extension,
                FirstName = request.FirstName.Trim(),
                FirstLastName = request.LastName.Trim(),
                PersonStatusId = 1, // ACTIVE
                IsProfileComplete = false,
                ScoutLeader = new ScoutLeader { IsSpeakerEligible = false },
            };

            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                person.ContactMechanisms.Add(
                    new PersonContactMechanism
                    {
                        ContactMechanismTypeId = 2, // CELLPHONE
                        ContactPurposeTypeId = 1, // PERSONAL
                        ContactValue = request.Phone.Trim(),
                        IsPrimary = true,
                        ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow),
                    }
                );
            }

            await leaderRepository.AddAsync(person, cancellationToken);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                person.FirstName = request.FirstName.Trim();

            if (!string.IsNullOrWhiteSpace(request.LastName))
                person.FirstLastName = request.LastName.Trim();

            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                var phoneMechanism = person.ContactMechanisms.FirstOrDefault(cm =>
                    cm.ContactMechanismTypeId == 2
                );
                if (phoneMechanism != null)
                {
                    phoneMechanism.ContactValue = request.Phone.Trim();
                }
                else
                {
                    person.ContactMechanisms.Add(
                        new PersonContactMechanism
                        {
                            ContactMechanismTypeId = 2,
                            ContactPurposeTypeId = 1,
                            ContactValue = request.Phone.Trim(),
                            IsPrimary = true,
                            ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow),
                        }
                    );
                }
            }

            await leaderRepository.UpdateAsync(person, cancellationToken);
        }

        // 4. Prevent duplicate attendance per session
        var alreadyRegistered = await attendanceRepository.ExistsAsync(
            module.Id,
            person.Id,
            cancellationToken
        );
        if (alreadyRegistered)
            throw new InvalidOperationException(
                $"El dirigente con CI '{ci}' ya registró su asistencia para el módulo '{module.Name}'."
            );

        // 5. Create Attendance Record
        var attendance = ModuleAttendance.Create(module.Id, person.Id);
        await attendanceRepository.AddAsync(attendance, cancellationToken);

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
