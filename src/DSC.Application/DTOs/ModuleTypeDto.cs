namespace DSC.Application.DTOs;

/// <summary>
/// Tipo de módulo (`module_type`) para el select de administración.
/// </summary>
public record class ModuleTypeDto(short Id, string Code, string Name, string? Description);
