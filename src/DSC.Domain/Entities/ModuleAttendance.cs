using DSC.Domain.Exceptions;

namespace DSC.Domain.Entities;

public class ModuleAttendance
{
    public Guid Id { get; private set; }
    public Guid TrainingModuleId { get; private set; }
    public Guid ScoutAdultId { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    public TrainingModule TrainingModule { get; private set; } = null!;
    public ScoutAdult ScoutAdult { get; private set; } = null!;

    private ModuleAttendance() { }

    public static ModuleAttendance Create(Guid trainingModuleId, Guid scoutAdultId)
    {
        if (trainingModuleId == Guid.Empty)
            throw new DomainException("El ID del módulo no puede ser vacío.");

        if (scoutAdultId == Guid.Empty)
            throw new DomainException("El ID del dirigente no puede ser vacío.");

        return new ModuleAttendance
        {
            Id = Guid.NewGuid(),
            TrainingModuleId = trainingModuleId,
            ScoutAdultId = scoutAdultId,
            RegisteredAt = DateTime.UtcNow
        };
    }
}
