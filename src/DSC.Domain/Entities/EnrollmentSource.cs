namespace DSC.Domain.Entities;

/// <summary>
/// Clasificador del canal u origen de inscripción (QR_CODE, WEB_LINK, AUTHENTICATED_USER, MANUAL_SECRETARY).
/// </summary>
public class EnrollmentSource
{
    public short Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Colecciones de navegación
    public ICollection<TrainingModuleEnrollment> Enrollments { get; set; } = [];
}
