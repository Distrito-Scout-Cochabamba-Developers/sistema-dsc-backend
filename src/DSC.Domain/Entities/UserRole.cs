namespace DSC.Domain.Entities;

/// <summary>
/// Asignación temporal de un rol de seguridad a un usuario.
/// </summary>
public class UserRole
{
    public long UserId { get; set; }
    public short RoleId { get; set; }
    public DateOnly ValidFrom { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public DateOnly? ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
