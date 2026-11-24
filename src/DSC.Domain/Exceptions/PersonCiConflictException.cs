namespace DSC.Domain.Exceptions;

/// <summary>Otra transacción creó una persona con el mismo CI y complemento.</summary>
public sealed class PersonCiConflictException()
    : InvalidOperationException("Ya existe una persona con el mismo CI. Vuelve a buscar.");
