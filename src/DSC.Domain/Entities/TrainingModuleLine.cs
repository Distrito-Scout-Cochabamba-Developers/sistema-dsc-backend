using DSC.Domain.ValueObjects;

namespace DSC.Domain.Entities;

/// <summary>
/// Pertenencia de un módulo a una línea de formación.
/// </summary>
public class TrainingModuleLine
{
    public long TrainingModuleId { get; set; }
    public TrainingLineId TrainingLineId { get; set; }

    public TrainingModule TrainingModule { get; set; } = null!;
    public TrainingLine TrainingLine { get; set; } = null!;
}
