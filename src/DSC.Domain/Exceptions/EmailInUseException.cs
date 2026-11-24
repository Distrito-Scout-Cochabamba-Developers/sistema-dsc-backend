namespace DSC.Domain.Exceptions;

/// <summary>El correo ya pertenece a otra cuenta; la operación asistida no se confirma.</summary>
public sealed class EmailInUseException()
    : InvalidOperationException("El correo ya pertenece a otra cuenta.");
