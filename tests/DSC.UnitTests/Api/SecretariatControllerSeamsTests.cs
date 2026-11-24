using DSC.Api.Controllers;
using DSC.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DSC.UnitTests.Api;

/// <summary>
/// Seams: mapeo único de conflictos de inscripción a respuestas HTTP y dependencias de los controladores
/// de Secretaría/Auth (solo abstracciones de Application, nunca clases concretas de Infrastructure).
/// </summary>
public class SecretariatControllerSeamsTests
{
    private sealed class Probe : ControllerBase { }

    [Theory]
    [InlineData(typeof(AlreadyEnrolledException), "urn:dsc:enrollment:already-enrolled")]
    [InlineData(typeof(SessionFullException), "urn:dsc:enrollment:session-full")]
    [InlineData(typeof(EnrollmentClosedException), "urn:dsc:enrollment:enrollment-closed")]
    [InlineData(typeof(SessionCancelledException), "urn:dsc:enrollment:enrollment-closed")]
    public void EnrollmentConflictMapsToConflictProblemWithStableType(Type exceptionType, string expectedType)
    {
        var exception = (EnrollmentConflictException)Activator.CreateInstance(exceptionType)!;

        var services = new ServiceCollection().AddLogging().AddControllers().Services.BuildServiceProvider();
        var probe = new Probe { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services } } };

        var result = probe.EnrollmentConflictProblem(exception);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, problem.StatusCode);
        var details = Assert.IsAssignableFrom<ProblemDetails>(problem.Value);
        Assert.Equal(expectedType, details.Type);
        Assert.Equal(exception.Message, details.Detail);
    }

    [Theory]
    [InlineData(typeof(SecretariatParticipantsController))]
    [InlineData(typeof(SecretariatInvitationsController))]
    [InlineData(typeof(SecretariatRoverEnrollmentsController))]
    [InlineData(typeof(AuthController))]
    public void ControllersDoNotDependOnConcreteInfrastructureTypes(Type controller)
    {
        var parameterTypes = controller.GetConstructors().SelectMany(c => c.GetParameters())
            .Concat(controller.GetMethods().SelectMany(m => m.GetParameters()))
            .Select(p => p.ParameterType);

        Assert.DoesNotContain(parameterTypes, t => t.Namespace?.StartsWith("DSC.Infrastructure", StringComparison.Ordinal) == true);
    }
}
