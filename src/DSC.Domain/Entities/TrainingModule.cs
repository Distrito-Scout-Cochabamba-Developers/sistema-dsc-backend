using DSC.Domain.Exceptions;

namespace DSC.Domain.Entities;

public class TrainingModule
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string SessionToken { get; private set; } = string.Empty;
    public DateTime EventDate { get; private set; }
    public bool IsActive { get; private set; }

    private TrainingModule() { }

    public static TrainingModule Create(string title, string sessionToken, DateTime eventDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("El título del módulo no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(sessionToken))
            throw new DomainException("El token de sesión QR no puede estar vacío.");

        return new TrainingModule
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            SessionToken = sessionToken.Trim(),
            EventDate = eventDate,
            IsActive = true
        };
    }
}
