namespace DSC.Domain.Entities;

/// <summary>
/// Representa una cuenta de usuario con credenciales de acceso (relación 1:1 estricta con Person).
/// </summary>
public class User
{
    public long Id { get; set; }
    public long PersonId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public short UserStatusId { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public Person Person { get; set; } = null!;
    public UserStatus UserStatus { get; set; } = null!;
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<FileAsset> UploadedFiles { get; set; } = [];
    public ICollection<TrainingModuleEnrollment> RegisteredEnrollments { get; set; } = [];
    public ICollection<TrainingModuleSessionStatusHistory> StatusChanges { get; set; } = [];
}
