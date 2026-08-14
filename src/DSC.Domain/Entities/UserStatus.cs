namespace DSC.Domain.Entities;

/// <summary>
/// Clasificador del estado de una cuenta de usuario (ACTIVE, BLOCKED, PENDING_VERIFICATION).
/// </summary>
public class UserStatus
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Colecciones de navegación
    public ICollection<User> Users { get; set; } = [];
}
