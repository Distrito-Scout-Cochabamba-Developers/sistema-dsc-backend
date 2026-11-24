using DSC.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DSC.Api.Controllers;

/// <summary>Única traducción de conflictos de inscripción a respuestas 409 para los canales de Secretaría.</summary>
public static class EnrollmentConflictProblems
{
    /// <summary>409 con tipo estable <c>urn:dsc:enrollment:*</c>; el mensaje del dominio no contiene datos personales.</summary>
    public static IActionResult EnrollmentConflictProblem(this ControllerBase controller, EnrollmentConflictException exception)
    {
        var type = exception switch
        {
            AlreadyEnrolledException => "already-enrolled",
            SessionFullException => "session-full",
            _ => "enrollment-closed",
        };
        return controller.Problem(statusCode: 409, type: $"urn:dsc:enrollment:{type}", detail: exception.Message);
    }

    /// <summary>409 que pide el complemento del CI para desambiguar; no revela nombres ni identificadores.</summary>
    public static IActionResult ExtensionRequiredProblem(this ControllerBase controller) =>
        controller.Problem(statusCode: 409, type: "urn:dsc:secretariat:extension-required", detail: "El CI tiene coincidencias con otros complementos. Indica el complemento y vuelve a buscar.");
}
