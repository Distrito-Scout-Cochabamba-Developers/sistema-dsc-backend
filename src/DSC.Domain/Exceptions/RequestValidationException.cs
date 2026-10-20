namespace DSC.Domain.Exceptions;

/// <summary>
/// Datos de entrada incompletos o con formato inválido.
/// </summary>
public class RequestValidationException : Exception
{
    public RequestValidationException(string message)
        : base(message) { }
}
