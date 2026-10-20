using System.Net;
using DSC.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DSC.Api.Middlewares;

public class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlingMiddleware> logger
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (InvalidCredentialsException ex)
        {
            logger.LogInformation(
                "Intento de autenticación fallido en {Path}.",
                context.Request.Path
            );
            await HandleExceptionAsync(context, ex);
        }
        catch (RequestValidationException ex)
        {
            logger.LogWarning(
                "Solicitud inválida en {Path}: {Message}",
                context.Request.Path,
                ex.Message
            );
            await HandleExceptionAsync(context, ex);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(
                "Error de validación de dominio en {Path}: {Message}",
                context.Request.Path,
                ex.Message
            );
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ha ocurrido una excepción no controlada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, detail) = exception switch
        {
            InvalidCredentialsException => (
                HttpStatusCode.Unauthorized,
                "Credenciales Inválidas",
                "Las credenciales son inválidas."
            ),

            RequestValidationException validationEx => (
                HttpStatusCode.BadRequest,
                "Solicitud Inválida",
                validationEx.Message
            ),

            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                "Error de Validación de Dominio",
                domainEx.Message
            ),

            KeyNotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                "Recurso No Encontrado",
                notFoundEx.Message
            ),

            InvalidOperationException conflictEx => (
                HttpStatusCode.Conflict,
                "Conflicto en la Operación",
                conflictEx.Message
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                "Error Interno del Servidor",
                "Ocurrió un error inesperado al procesar la solicitud."
            ),
        };

        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
        };

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
