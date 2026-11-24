using System.ComponentModel.DataAnnotations;
using DSC.Application.Interfaces;
using DSC.Application.Queries;
using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using DSC.Domain.ValueObjects;

namespace DSC.Application.Commands;

/// <summary>Alta mínima inseparable de la inscripción asistida.</summary>
public record NewSecretariatLeader(string Ci, string? Extension, string FirstName, string LastName, string Phone, string Email);

/// <summary>Coordina identidad y cupo bajo la misma transacción.</summary>
public class SecretariatEnrollment(IScoutLeaderRepository people, IUnitOfWork unitOfWork, EnrollParticipantInSessionCommandHandler enrollment, IAccountInvitationRepository invitations)
{
    private static CiNumber Parse(string ci, string? extension)
    {
        try
        {
            var parsed = new CiNumber(ci, extension ?? "");
            if (parsed.Number.Any(c => c is < '0' or > '9')) throw new DomainException("CI inválido.");
            return parsed;
        }
        catch (DomainException) { throw new DomainException("CI o complemento inválido."); }
    }

    public Task<TrainingModuleEnrollment> CreateAsync(long sessionId, long operatorId, NewSecretariatLeader input, CancellationToken cancellationToken)
    {
        var ci = Parse(input.Ci, input.Extension);
        if (string.IsNullOrWhiteSpace(input.FirstName) || input.FirstName.Trim().Length > 120 ||
            string.IsNullOrWhiteSpace(input.LastName) || input.LastName.Trim().Length > 120 ||
            string.IsNullOrWhiteSpace(input.Phone) || input.Phone.Trim().Length > 255 ||
            string.IsNullOrWhiteSpace(input.Email) || input.Email.Trim().Length > 150 || !new EmailAddressAttribute().IsValid(input.Email.Trim()))
            throw new DomainException("Nombres, apellidos, celular y correo válido son obligatorios.");
        return unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            await people.LockCiAsync(ci.Number, token);
            // Una búsqueda sin resultados para un complemento no autoriza crear otro CI idéntico.
            if ((await people.FindByCiAsync(ci.Number, null, token)).Count != 0)
                throw new AmbiguousSecretariatCiException();
            var person = await people.CreateSecretariatLeaderAsync(ci.Number, string.IsNullOrEmpty(ci.Extension) ? null : ci.Extension, input.FirstName.Trim(), input.LastName.Trim(), input.Phone.Trim(), input.Email.Trim(), token);
            var result = await enrollment.HandleAsync(new(sessionId, person.Id, "SECRETARIAT", "Inscripción asistida por Secretaría", operatorId, false), token);
            // Cuenta pendiente e invitación PENDING se confirman o revierten con el perfil y la inscripción.
            await invitations.CreatePendingAsync(person.Id, result.Id, input.Email, token);
            return result;
        }, cancellationToken);
    }

    /// <summary>
    /// Inscribe a una persona existente. Con CI se reverifica la identidad bajo bloqueo;
    /// sin CI se conserva el contrato de #157 (solo identificador de persona).
    /// </summary>
    public Task<TrainingModuleEnrollment> ExistingAsync(long sessionId, long personId, long operatorId, string? ciValue, string? extension, bool rover, CancellationToken cancellationToken)
    {
        var ci = string.IsNullOrWhiteSpace(ciValue) ? null : Parse(ciValue, extension);
        return unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            if (ci is not null)
            {
                await people.LockCiAsync(ci.Number, token);
                var matches = await people.FindByCiAsync(ci.Number, string.IsNullOrEmpty(ci.Extension) ? null : ci.Extension, token);
                if (matches.Count != 1 || matches[0].Id != personId)
                    throw new AmbiguousSecretariatCiException();
            }
            var person = await people.GetByIdAsync(personId, token);
            if (person is null || (!rover && person.ScoutLeader is null)) throw new KeyNotFoundException();
            return await enrollment.HandleAsync(new(sessionId, personId, "SECRETARIAT", "Inscripción asistida por Secretaría", operatorId, rover), token);
        }, cancellationToken);
    }
}
