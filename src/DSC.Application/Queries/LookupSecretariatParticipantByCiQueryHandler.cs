using DSC.Application.DTOs;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using DSC.Domain.ValueObjects;

namespace DSC.Application.Queries;

public record LookupSecretariatParticipantByCiQuery(string CiNumber, string? Extension);

public class AmbiguousSecretariatCiException(
    string message = "El CI tiene varias coincidencias. El complemento debe identificar a una sola persona."
) : InvalidOperationException(message);

/// <summary>El CI existe con varios complementos o solo con otros: la API pide el complemento que lo desambigüe.</summary>
public sealed class CiExtensionRequiredException()
    : AmbiguousSecretariatCiException("El CI tiene coincidencias con otros complementos. Indica el complemento que identifique a la persona.");

public class LookupSecretariatParticipantByCiQueryHandler(IScoutLeaderRepository leaderRepository)
{
    public async Task<SecretariatParticipantDto?> HandleAsync(
        LookupSecretariatParticipantByCiQuery query,
        CancellationToken cancellationToken
    )
    {
        CiNumber ci;
        try
        {
            ci = new CiNumber(query.CiNumber, query.Extension ?? string.Empty);
        }
        catch (DomainException)
        {
            // This value object's errors echo user input; never propagate those here.
            throw new DomainException("CI o complemento inválido.");
        }
        var matches = await leaderRepository.FindByCiAsync(
            ci.Number,
            string.IsNullOrEmpty(ci.Extension) ? null : ci.Extension,
            cancellationToken
        );

        if (matches.Count > 1)
            throw new CiExtensionRequiredException();

        var person = matches.SingleOrDefault();
        if (person is null)
        {
            // Con complemento sin coincidencia, pero con el CI existente bajo otro: se pide el complemento correcto.
            if (!string.IsNullOrEmpty(ci.Extension) && (await leaderRepository.ExtensionsOfCiAsync(ci.Number, cancellationToken)).Count > 0)
                throw new CiExtensionRequiredException();
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var phone =
            person
                .ContactMechanisms.Where(contact =>
                    contact.ContactMechanismTypeId == 2
                    && contact.ValidFrom <= today
                    && (contact.ValidUntil is null || contact.ValidUntil >= today)
                )
                .OrderByDescending(contact => contact.IsPrimary)
                .ThenByDescending(contact => contact.ValidFrom)
                .ThenByDescending(contact => contact.Id)
                .Select(contact => contact.ContactValue)
                .FirstOrDefault()
            ?? string.Empty;

        return new SecretariatParticipantDto(
            person.Id,
            string.Join(
                " ",
                new[] { person.FirstName, person.MiddleName }.Where(name =>
                    !string.IsNullOrWhiteSpace(name)
                )
            ),
            string.Join(
                " ",
                new[] { person.FirstLastName, person.SecondLastName }.Where(name =>
                    !string.IsNullOrWhiteSpace(name)
                )
            ),
            person.IdCard,
            person.IdCardExtension ?? string.Empty,
            phone,
            person.User is not null,
            person.ScoutLeader is not null,
            person.IsProfileComplete
        );
    }
}
