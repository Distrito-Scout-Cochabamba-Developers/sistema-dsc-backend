using DSC.Domain.ValueObjects;

namespace DSC.Domain.Entities;

/// <summary>
/// Calificación de una línea de formación scout (Insignia de Madera I-II, etc.).
/// </summary>
public class TrainingLevel
{
    public TrainingLevelId Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public TrainingLineId TrainingLineId { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navegación
    public TrainingLine TrainingLine { get; private set; } = null!;
    public ICollection<ScoutLeaderTrainingLevel> LeaderLevels { get; private set; } = [];
    public ICollection<TrainingLevelRequirement> Requirements { get; private set; } = [];

    private TrainingLevel() { }

    public TrainingLevel(
        TrainingLevelId id,
        string code,
        string name,
        TrainingLineId trainingLineId,
        bool isActive = true
    )
    {
        Id = id;
        Code = code;
        Name = name;
        TrainingLineId = trainingLineId;
        IsActive = isActive;
    }

    public TrainingLevel(
        string code,
        string name,
        TrainingLineId trainingLineId,
        bool isActive = true
    ) : this(default, code, name, trainingLineId, isActive) { }

    public TrainingLevel(
        TrainingLevelId id,
        string code,
        string name,
        TrainingLine trainingLine,
        bool isActive = true
    ) : this(id, code, name, trainingLine?.Id ?? default, isActive)
    {
        TrainingLine = trainingLine!;
    }

    public TrainingLevel(
        string code,
        string name,
        TrainingLine trainingLine,
        bool isActive = true
    ) : this(default, code, name, trainingLine, isActive) { }

    public void Update(string name, TrainingLineId trainingLineId, bool isActive = true)
    {
        Name = name;
        TrainingLineId = trainingLineId;
        IsActive = isActive;
    }
}
