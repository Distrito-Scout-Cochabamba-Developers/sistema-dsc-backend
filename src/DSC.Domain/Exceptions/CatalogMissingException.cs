namespace DSC.Domain.Exceptions;

/// <summary>Falta una fila de catálogo de módulos requerida (instalación sin migrar). Es un error del servidor, no del cliente.</summary>
public sealed class CatalogMissingException(string catalog, string code)
    : Exception($"Falta el valor de catálogo de módulos '{code}' en '{catalog}'. Aplique las migraciones pendientes.");
