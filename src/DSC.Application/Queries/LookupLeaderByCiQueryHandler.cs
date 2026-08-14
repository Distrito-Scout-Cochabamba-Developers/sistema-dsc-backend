using DSC.Application.DTOs;
using DSC.Domain.Repositories;
using DSC.Domain.ValueObjects;

namespace DSC.Application.Queries;

public record class LookupLeaderByCiQuery(string CiNumber, string Extension = "");

public class LookupLeaderByCiQueryHandler(IScoutLeaderRepository leaderRepository)
{
    public async Task<ScoutLeaderDto?> HandleAsync(LookupLeaderByCiQuery query, CancellationToken cancellationToken)
    {
        var ci = new CiNumber(query.CiNumber, query.Extension);
        var person = await leaderRepository.GetByCiNumberAsync(ci.Number, cancellationToken);

        if (person == null)
            return null;

        var phone = person.ContactMechanisms
            .FirstOrDefault(cm => cm.ContactMechanismTypeId == 2 || cm.IsPrimary)?.ContactValue
            ?? string.Empty;

        return new ScoutLeaderDto(
            person.Id,
            person.FirstName,
            $"{person.FirstLastName} {person.SecondLastName}".Trim(),
            person.IdCard,
            person.IdCardExtension ?? string.Empty,
            phone,
            person.IsProfileComplete
        );
    }
}
