using DSC.Application.DTOs;
using DSC.Domain.Repositories;
using DSC.Domain.ValueObjects;

namespace DSC.Application.Queries;

public record class LookupAdultByCiQuery(string CiNumber, string Extension = "");

public class LookupAdultByCiQueryHandler(IScoutAdultRepository adultRepository)
{
    public async Task<ScoutAdultDto?> HandleAsync(LookupAdultByCiQuery query, CancellationToken cancellationToken)
    {
        var ci = new CiNumber(query.CiNumber, query.Extension);
        var adult = await adultRepository.GetByCiNumberAsync(ci.Number, cancellationToken);

        if (adult == null)
            return null;

        return new ScoutAdultDto(
            adult.Id,
            adult.FirstName,
            adult.LastName,
            adult.Ci.Number,
            adult.Ci.Extension,
            adult.Phone,
            adult.IsPartialRegistration
        );
    }
}
