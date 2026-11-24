using DSC.Application.Interfaces;
using DSC.Domain.Entities;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

/// <summary>
/// Datos de la operación única de inscripción. El canal decide el origen y el
/// operador; nunca se toma el operador del cuerpo de la solicitud.
/// </summary>
public record class EnrollParticipantInSessionCommand(
    long SessionId,
    long PersonId,
    string EnrollmentSourceCode,
    string EnrollmentSourceName,
    long? RegisteredByUserId,
    bool RoverWithoutAccount
);

/// <summary>
/// Operación compartida por autoservicio y Secretaría (#157). Valida ventana,
/// duplicado y cupo en una sola transacción y no usa un contador adicional:
/// el cupo es el de la convocatoria (#155). Para #158/#160, el alta de la
/// persona y esta inscripción deben ejecutarse dentro del mismo
/// <see cref="IUnitOfWork"/>; la operación se une a la transacción ambiente.
/// </summary>
public class EnrollParticipantInSessionCommandHandler(
    ITrainingModuleSessionRepository sessionRepository,
    IUnitOfWork unitOfWork
)
{
    public Task<TrainingModuleEnrollment> HandleAsync(
        EnrollParticipantInSessionCommand command,
        CancellationToken cancellationToken = default
    ) =>
        unitOfWork.ExecuteInTransactionAsync(
            token =>
                sessionRepository.EnrollAsync(
                    new SessionEnrollmentRequest(
                        command.SessionId,
                        command.PersonId,
                        command.EnrollmentSourceCode,
                        command.EnrollmentSourceName,
                        command.RegisteredByUserId,
                        command.RoverWithoutAccount
                    ),
                    token
                ),
            cancellationToken
        );
}
