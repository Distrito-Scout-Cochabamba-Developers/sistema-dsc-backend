using DSC.Domain.Exceptions;
using DSC.Domain.ValueObjects;

namespace DSC.Domain.Entities;

public class ScoutAdult
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public CiNumber Ci { get; private set; } = null!;
    public string Phone { get; private set; } = string.Empty;
    public bool IsPartialRegistration { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private ScoutAdult() { }

    public static ScoutAdult Create(
        string firstName,
        string lastName,
        CiNumber ci,
        string phone = "",
        bool isPartialRegistration = false)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("El nombre del dirigente no puede estar vacío.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("El apellido del dirigente no puede estar vacío.");

        return new ScoutAdult
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Ci = ci ?? throw new DomainException("El CI no puede ser nulo."),
            Phone = phone?.Trim() ?? string.Empty,
            IsPartialRegistration = isPartialRegistration,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateProfile(string firstName, string lastName, string phone)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            FirstName = firstName.Trim();

        if (!string.IsNullOrWhiteSpace(lastName))
            LastName = lastName.Trim();

        if (phone != null)
            Phone = phone.Trim();

        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteRegistration()
    {
        IsPartialRegistration = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
