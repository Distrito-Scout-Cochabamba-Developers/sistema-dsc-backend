using DSC.Application.DTOs;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using DSC.Domain.ValueObjects;

namespace DSC.Application.Commands;

public class RegisterDigitalAttendanceCommandHandler(
    ITrainingModuleRepository moduleRepository,
    IScoutAdultRepository adultRepository,
    IModuleAttendanceRepository attendanceRepository)
{
    public async Task<ModuleAttendanceResultDto> HandleAsync(
        RegisterDigitalAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.SessionToken))
            throw new DomainException("El token de sesión no puede estar vacío.");

        // 1. Verify training module session existence
        var module = await moduleRepository.GetBySessionTokenAsync(request.SessionToken, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró ningún módulo de capacitación activo con el token de sesión '{request.SessionToken}'.");

        // 2. Validate CI Value Object
        var ci = new CiNumber(request.CiNumber, request.Extension);

        // 3. Find or Create Scout Adult Leader
        var adult = await adultRepository.GetByCiNumberAsync(ci.Number, cancellationToken);
        if (adult == null)
        {
            adult = ScoutAdult.Create(
                request.FirstName,
                request.LastName,
                ci,
                request.Phone,
                isPartialRegistration: true
            );
            await adultRepository.AddAsync(adult, cancellationToken);
        }
        else
        {
            adult.UpdateProfile(request.FirstName, request.LastName, request.Phone);
            await adultRepository.UpdateAsync(adult, cancellationToken);
        }

        // 4. Prevent duplicate attendance per session
        var alreadyRegistered = await attendanceRepository.ExistsAsync(module.Id, adult.Id, cancellationToken);
        if (alreadyRegistered)
            throw new InvalidOperationException($"El dirigente con CI '{ci}' ya registró su asistencia para el módulo '{module.Title}'.");

        // 5. Create Attendance Record
        var attendance = ModuleAttendance.Create(module.Id, adult.Id);
        await attendanceRepository.AddAsync(attendance, cancellationToken);

        return new ModuleAttendanceResultDto(
            attendance.Id,
            module.Id,
            module.Title,
            adult.Id,
            $"{adult.FirstName} {adult.LastName}".Trim(),
            adult.Ci.FullValue,
            attendance.RegisteredAt,
            adult.IsPartialRegistration
        );
    }
}
