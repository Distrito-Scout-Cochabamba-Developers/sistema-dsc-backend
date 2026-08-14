using DSC.Domain.Exceptions;

namespace DSC.Domain.Entities;

public class ModuleAttendance
{
    public Guid Id { get; private set; }
    public long TrainingModuleId { get; private set; }
    public long ScoutLeaderId { get; private set; }
    public DateTime RegisteredAt { get; private set; }

    public TrainingModule TrainingModule { get; private set; } = null!;
    public Person ScoutLeader { get; private set; } = null!;

    private ModuleAttendance() { }

    public static ModuleAttendance Create(long trainingModuleId, long scoutLeaderId)
    {
        if (trainingModuleId <= 0)
            throw new DomainException("El ID del módulo no puede ser menor o igual a cero.");

        if (scoutLeaderId <= 0)
            throw new DomainException("El ID del dirigente no puede ser menor o igual a cero.");

        return new ModuleAttendance
        {
            Id = Guid.NewGuid(),
            TrainingModuleId = trainingModuleId,
            ScoutLeaderId = scoutLeaderId,
            RegisteredAt = DateTime.UtcNow
        };
    }
}
