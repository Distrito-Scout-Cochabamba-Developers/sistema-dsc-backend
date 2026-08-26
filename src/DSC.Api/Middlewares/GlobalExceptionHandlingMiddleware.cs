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
