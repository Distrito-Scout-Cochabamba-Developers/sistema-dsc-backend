using DSC.Application.DTOs;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;
using DSC.Domain.ValueObjects;

namespace DSC.Application.Queries;

public record LookupSecretariatParticipantByCiQuery(string CiNumber, string? Extension);

public sealed class AmbiguousSecretariatCiException()
    : InvalidOperationException(
        "El CI tiene varias coincidencias. El complemento debe identificar a una sola persona."
    );

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
            throw new AmbiguousSecretariatCiException();

        var person = matches.SingleOrDefault();
        if (person is null)
            return null;

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
