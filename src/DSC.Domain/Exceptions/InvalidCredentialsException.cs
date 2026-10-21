namespace DSC.Domain.Exceptions;

/// <summary>
/// Autenticación fallida. El mensaje debe ser genérico para no revelar
/// si el identificador existe o el estado de la cuenta.
/// </summary>
public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Las credenciales son inválidas.") { }
}
