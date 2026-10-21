namespace DSC.Domain.Entities;

using DSC.Domain.Exceptions;

/// <summary>
/// Representa una cuenta de usuario con credenciales de acceso (relación 1:1 estricta con Person).
/// </summary>
public class User
{
    public long Id { get; set; }
    public long PersonId { get; set; }
    public string Username { get; set; } = string.Empty;
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

    /// <summary>
    /// Códigos de los roles activos cuya asignación está vigente en la fecha indicada.
    /// </summary>
    public IReadOnlyCollection<string> GetActiveRoleCodes(DateOnly asOf)
    {
        return UserRoles
            .Where(userRole =>
                userRole.Role is not null
                && userRole.Role.IsActive
                && userRole.ValidFrom <= asOf
                && (userRole.ValidUntil is null || userRole.ValidUntil >= asOf)
            )
            .Select(userRole => userRole.Role.Code)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public void EnsureCanAuthenticate()
    {
        if (!string.Equals(UserStatus?.Code, "ACTIVE", StringComparison.OrdinalIgnoreCase))
            throw new InvalidCredentialsException();
    }
}
