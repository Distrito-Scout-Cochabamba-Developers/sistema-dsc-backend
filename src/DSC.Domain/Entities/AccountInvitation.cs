namespace DSC.Domain.Entities;

/// <summary>
/// Entrega pendiente o realizada de la invitación de activación de una cuenta creada
/// junto con una inscripción asistida. No contiene el secreto del enlace ni datos personales.
/// </summary>
public class AccountInvitation
{
    public const string Pending = "PENDING";
    public const string Sent = "SENT";
    public const string Closed = "CLOSED";

    public long Id { get; set; }
    public long UserId { get; set; }
    public long EnrollmentId { get; set; }
    /// <summary>PENDING (por enviar o enviar de nuevo), SENT o CLOSED (la cuenta ya no admite invitación).</summary>
    public string Status { get; set; } = Pending;
    public int Attempts { get; set; }
    /// <summary>Tipo del último fallo (sin mensaje, que podría contener datos personales).</summary>
    public string? LastFailureCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }

    public User User { get; set; } = null!;
    public TrainingModuleEnrollment Enrollment { get; set; } = null!;
}
