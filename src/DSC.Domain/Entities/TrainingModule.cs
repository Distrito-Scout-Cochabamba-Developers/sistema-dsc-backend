using DSC.Domain.Exceptions;

namespace DSC.Domain.Entities;

/// <summary>
/// Catálogo general de módulos de formación curricular estándar.
/// </summary>
public class TrainingModule
{
    public const int CodeMaxLength = 30;
    public const int NameMaxLength = 150;

    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public short ModuleTypeId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navegación
    public ModuleType ModuleType { get; set; } = null!;
    public ICollection<TrainingModuleSession> Sessions { get; set; } = [];
    public ICollection<TrainingModuleLessonPlanTemplate> LessonPlanTemplates { get; set; } = [];

    /// <summary>
    /// Crea un módulo activo del catálogo validando código, nombre y longitudes.
    /// </summary>
    public static TrainingModule Create(
        string code,
        string name,
        string? description,
        short moduleTypeId
    )
    {
        var module = new TrainingModule();
        module.Update(code, name, description, moduleTypeId, isActive: true);
        return module;
    }

    /// <summary>
    /// Reemplaza los datos del módulo. Valida todo antes de modificar el estado.
    /// </summary>
    public void Update(
        string code,
        string name,
        string? description,
        short moduleTypeId,
        bool isActive
    )
    {
        var normalizedCode = code?.Trim() ?? string.Empty;
        var normalizedName = name?.Trim() ?? string.Empty;

        if (normalizedCode.Length == 0)
            throw new DomainException("El código del módulo es obligatorio.");

        if (normalizedCode.Length > CodeMaxLength)
            throw new DomainException(
                $"El código del módulo no puede superar {CodeMaxLength} caracteres."
            );

        if (normalizedName.Length == 0)
            throw new DomainException("El nombre del módulo es obligatorio.");

        if (normalizedName.Length > NameMaxLength)
            throw new DomainException(
                $"El nombre del módulo no puede superar {NameMaxLength} caracteres."
            );

        Code = normalizedCode;
        Name = normalizedName;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ModuleTypeId = moduleTypeId;
        IsActive = isActive;
    }
}
