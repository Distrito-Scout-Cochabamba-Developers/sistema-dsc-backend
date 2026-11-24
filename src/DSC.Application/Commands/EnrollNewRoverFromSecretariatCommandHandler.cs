using System.Text.RegularExpressions;
using DSC.Application.Interfaces;
using DSC.Application.Queries;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using DSC.Domain.ValueObjects;

namespace DSC.Application.Commands;

public record EnrollNewRoverFromSecretariatCommand(
    long SessionId,
    string Ci,
    string? Extension,
    string FirstName,
    string LastName,
    string? Phone,
    long OperatorUserId
);

/// <summary>
/// Rover nuevo (#160): alta mínima + inscripción en una sola transacción. Si la
/// inscripción falla (cupo, ventana, duplicado, convocatoria inexistente) no queda
/// perfil, contacto ni inscripción. No crea cuenta, dirigente, afiliación ni rol.
/// </summary>
public partial class EnrollNewRoverFromSecretariatCommandHandler(
    IRoverRegistrationRepository rovers,
    EnrollParticipantInSessionCommandHandler enrollment,
    IUnitOfWork unitOfWork
)
{
    [GeneratedRegex(@"^\+?\d{7,15}$")]
    private static partial Regex PhonePattern();

    public Task<TrainingModuleEnrollment> HandleAsync(
        EnrollNewRoverFromSecretariatCommand command,
        CancellationToken cancellationToken = default
    )
    {
        // Los mensajes de validación no incluyen el CI ni el celular: no deben llegar a respuestas ni logs.
        var firstName = command.FirstName?.Trim() ?? "";
        var lastName = command.LastName?.Trim() ?? "";
        var phone = string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim();
        if (firstName.Length is 0 or > 120 || lastName.Length is 0 or > 120)
            throw new RequestValidationException("Nombres y apellidos son obligatorios.");
        if (phone is not null && !PhonePattern().IsMatch(phone))
            throw new RequestValidationException("El celular no es válido.");
        CiNumber ci;
        try { ci = new CiNumber(command.Ci ?? "", command.Extension ?? ""); }
        catch (DomainException) { throw new RequestValidationException("CI o complemento inválido."); }

        return unitOfWork.ExecuteInTransactionAsync(
            async token =>
            {
                await rovers.LockCiAsync(ci.Number, token);
                switch (CiExtensionRule.Classify(
                    await rovers.ExtensionsOfCiAsync(ci.Number, token), string.IsNullOrEmpty(ci.Extension) ? null : ci.Extension))
                {
                    case CiPresence.ExactMatch: throw new RoverPersonExistsException();
                    case CiPresence.OtherComplement: throw new CiExtensionRequiredException();
                }
                var person = await rovers.CreateAsync(
                    ci.Number,
                    string.IsNullOrEmpty(ci.Extension) ? null : ci.Extension,
                    firstName, lastName, phone, token);
                return await enrollment.HandleAsync(
                    new(command.SessionId, person.Id, "SECRETARIAT", "Inscripción asistida por Secretaría", command.OperatorUserId, true),
                    token);
            },
            cancellationToken);
    }
}
