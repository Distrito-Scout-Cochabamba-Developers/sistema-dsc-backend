namespace DSC.Domain.Exceptions;

/// <summary>
/// Conflicto al inscribir a una persona. La API lo presenta como 409 sin
/// distinguir el canal que lo originó.
/// </summary>
public class EnrollmentConflictException : InvalidOperationException
{
    public EnrollmentConflictException(string message)
        : base(message) { }
}

/// <summary>La persona ya tiene una inscripción en la convocatoria; no consume otra plaza.</summary>
public sealed class AlreadyEnrolledException : EnrollmentConflictException
{
    public AlreadyEnrolledException()
        : base("Ya estás inscrito en esta sesión.") { }
}

/// <summary>El cupo propio de la convocatoria se agotó.</summary>
public sealed class SessionFullException : EnrollmentConflictException
{
    public SessionFullException()
        : base("El cupo de esta sesión se agotó.") { }
}

/// <summary>La convocatoria no admite inscripciones en este momento.</summary>
public sealed class EnrollmentClosedException : EnrollmentConflictException
{
    public EnrollmentClosedException()
        : base("La inscripción de esta convocatoria está cerrada.") { }
}

/// <summary>La convocatoria fue cancelada.</summary>
public sealed class SessionCancelledException : EnrollmentConflictException
{
    public SessionCancelledException()
        : base("Esta convocatoria está cancelada.") { }
}

/// <summary>La convocatoria ya finalizó.</summary>
public sealed class SessionFinishedException : EnrollmentConflictException
{
    public SessionFinishedException()
        : base("Esta convocatoria ya finalizó.") { }
}
