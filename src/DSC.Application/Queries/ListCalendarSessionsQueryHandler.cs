using DSC.Application.DTOs;
using DSC.Application.Interfaces;
using DSC.Domain.Repositories;

namespace DSC.Application.Queries;

public record class ListCalendarSessionsQuery(DateOnly? From = null, DateOnly? To = null);

public class ListCalendarSessionsQueryHandler(
    ITrainingModuleSessionRepository sessionRepository,
    ICurrentUser currentUser
)
{
    public async Task<IReadOnlyList<CalendarSessionDto>> HandleAsync(
        ListCalendarSessionsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var sessions = await sessionRepository.ListForCalendarAsync(
            query.From,
            query.To,
            cancellationToken
        );

        return sessions
            .Select(session => CalendarSessionMapping.ToDto(session, currentUser.PersonId))
            .OrderBy(session => session.StartsAt)
            .ToArray();
    }
}
