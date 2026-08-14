using System.Text.RegularExpressions;
using DSC.Domain.Exceptions;

namespace DSC.Domain.ValueObjects;

public record class CiNumber
{
    private static readonly HashSet<string> ValidExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        "CB", "LP", "SC", "OR", "PT", "TJ", "CH", "BE", "PD"
    };

    public string Number { get; }
    public string Extension { get; }
    public string FullValue => string.IsNullOrWhiteSpace(Extension) ? Number : $"{Number}-{Extension.ToUpperInvariant()}";

    public CiNumber(string number, string extension = "")
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("El Carnet de Identidad (CI) no puede estar vacío.");

        var trimmedNumber = number.Trim();

        // Enforce numeric validation exclusively (reject alphabetic characters or phone-length strings > 10 digits)
        if (!Regex.IsMatch(trimmedNumber, @"^\d{4,10}$"))
            throw new DomainException($"El CI '{number}' no es un número de carnet válido. Debe ser exclusivamente numérico entre 4 y 10 dígitos.");

        var trimmedExt = extension?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!string.IsNullOrEmpty(trimmedExt) && !ValidExtensions.Contains(trimmedExt))
            throw new DomainException($"La extensión '{extension}' no es una extensión de departamento válida en Bolivia.");

        Number = trimmedNumber;
        Extension = trimmedExt;
    }

    public override string ToString() => FullValue;
}
