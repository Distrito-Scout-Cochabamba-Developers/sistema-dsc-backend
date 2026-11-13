using DSC.Domain.Exceptions;
using DSC.Domain.ValueObjects;

namespace DSC.Domain.Entities;

/// <summary>Configuración estándar de un criterio de módulos para un nivel.</summary>
public class TrainingLevelRequirement
{
    public const string ModulesType = "MODULES";
    private readonly List<TrainingLevelMandatoryModule> _mandatoryModules = [];

    public int Id { get; private set; }
    public TrainingLevelId TrainingLevelId { get; private set; }
    public string RequirementType { get; private set; } = ModulesType;
    public int TotalModulesCount { get; private set; }
    public int MinMandatoryModulesCount { get; private set; }
    public Guid Version { get; private set; }
    public TrainingLevel TrainingLevel { get; private set; } = null!;
    public IReadOnlyCollection<TrainingLevelMandatoryModule> MandatoryModules =>
        _mandatoryModules.AsReadOnly();

    private TrainingLevelRequirement() { }

    public static TrainingLevelRequirement Create(
        TrainingLevelId trainingLevelId,
        int totalModulesCount,
        int minMandatoryModulesCount,
        IReadOnlyCollection<TrainingModule> modules
    )
    {
        if (trainingLevelId.Value <= 0)
            throw new DomainException("El nivel de formación es obligatorio.");

        var requirement = new TrainingLevelRequirement { TrainingLevelId = trainingLevelId };
        requirement.ConfigureCurricularModules(
            totalModulesCount,
            minMandatoryModulesCount,
            modules
        );
        return requirement;
    }

    // ponytail: solo criterio de módulos; añadir otras categorías cuando tengan reglas definidas.
    public void ConfigureCurricularModules(
        int totalModulesCount,
        int minMandatoryModulesCount,
        IReadOnlyCollection<TrainingModule> modules
    )
    {
        if (RequirementType != ModulesType)
            throw new DomainException("El criterio no corresponde a módulos.");
        if (modules is null || modules.Any(module => module is null || module.Id <= 0))
            throw new DomainException("La selección debe contener módulos existentes.");

        var ids = modules.Select(module => module.Id).ToHashSet();
        if (ids.Count != modules.Count)
            throw new DomainException("Los módulos obligatorios no pueden repetirse.");
        if (totalModulesCount < 0 || minMandatoryModulesCount < 0)
            throw new DomainException("Las cantidades de módulos no pueden ser negativas.");
        if (minMandatoryModulesCount > totalModulesCount || minMandatoryModulesCount > ids.Count)
            throw new DomainException(
                "El mínimo obligatorio no puede superar el total ni la selección elegible."
            );

        // Validar antes de mutar; conservar enlaces retenidos evita duplicar claves rastreadas por EF.
        _mandatoryModules.RemoveAll(link => !ids.Contains(link.TrainingModuleId));
        var retained = _mandatoryModules.Select(link => link.TrainingModuleId).ToHashSet();
        _mandatoryModules.AddRange(
            modules
                .Where(module => !retained.Contains(module.Id))
                .Select(module => new TrainingLevelMandatoryModule(module))
        );
        TotalModulesCount = totalModulesCount;
        MinMandatoryModulesCount = minMandatoryModulesCount;
        Version = Guid.NewGuid();
    }
}
