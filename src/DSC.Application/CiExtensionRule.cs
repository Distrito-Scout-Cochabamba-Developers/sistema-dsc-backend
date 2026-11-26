namespace DSC.Application;

/// <summary>Relación entre un Carnet de Identidad solicitado (con complemento opcional) y las personas que ya tienen ese número de Carnet de Identidad.</summary>
public enum CiPresence
{
    /// <summary>Nadie tiene ese número de Carnet de Identidad.</summary>
    Free,

    /// <summary>Ya existe una persona con ese Carnet de Identidad y ese mismo complemento.</summary>
    ExactMatch,

    /// <summary>El Carnet de Identidad existe solo bajo otros complementos: hay que pedir el complemento para desambiguar.</summary>
    OtherComplement,
}

/// <summary>
/// Regla única de complemento de Carnet de Identidad (spec: ante ambigüedad se pide el complemento; nunca se elige ni se crea
/// automáticamente). El complemento vacío y el ausente son equivalentes; la comparación ignora mayúsculas.
/// </summary>
public static class CiExtensionRule
{
    public static CiPresence Classify(IReadOnlyCollection<string?> existingExtensions, string? requestedExtension)
    {
        if (existingExtensions.Count == 0)
            return CiPresence.Free;
        var requested = requestedExtension ?? "";
        return existingExtensions.Any(e => string.Equals(e ?? "", requested, StringComparison.OrdinalIgnoreCase))
            ? CiPresence.ExactMatch
            : CiPresence.OtherComplement;
    }
}
