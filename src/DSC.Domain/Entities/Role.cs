namespace DSC.Domain.Entities;

/// <summary>
/// Clasificador de roles de seguridad y autorización en el sistema (RBAC).
/// </summary>
public class Role
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Colecciones de navegación
    public ICollection<UserRole> UserRoles { get; set; } = [];
}
