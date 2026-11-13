namespace DSC.Domain.Entities;

/// <summary>Módulo obligatorio elegible para un criterio de nivel.</summary>
public class TrainingLevelMandatoryModule
{
    public int TrainingLevelRequirementId { get; private set; }
    public TrainingLevelRequirement TrainingLevelRequirement { get; private set; } = null!;
    public long TrainingModuleId { get; private set; }
    public TrainingModule TrainingModule { get; private set; } = null!;

    private TrainingLevelMandatoryModule() { }

    internal TrainingLevelMandatoryModule(TrainingModule module)
    {
        TrainingModuleId = module.Id;
        TrainingModule = module;
    }
}
