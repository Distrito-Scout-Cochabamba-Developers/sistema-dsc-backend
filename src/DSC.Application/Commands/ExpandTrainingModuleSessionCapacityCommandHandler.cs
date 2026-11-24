using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public class ExpandTrainingModuleSessionCapacityCommandHandler(
    ITrainingModuleSessionRepository repository
)
{
    public Task HandleAsync(
        long sessionId,
        int? capacity,
        CancellationToken cancellationToken = default
    )
    {
        if (capacity is < 0)
            throw new RequestValidationException("El cupo no puede ser negativo.");

        return repository.ExpandCapacityAsync(sessionId, capacity, cancellationToken);
    }
}
