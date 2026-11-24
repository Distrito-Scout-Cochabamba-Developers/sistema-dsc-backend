using System.Text.Json;
using DSC.Api.Middlewares;
using DSC.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace DSC.UnitTests.Api;

/// <summary>El conflicto de CI de la asistencia digital (endpoint anónimo) responde 409 sin datos personales.</summary>
public class PersonCiConflictMiddlewareTests
{
    [Fact]
    public async Task PersonCiConflictIsMappedTo409WithoutPersonalData()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/attendance/register";
        context.Response.Body = new MemoryStream();
        var middleware = new GlobalExceptionHandlingMiddleware(
            _ => throw new PersonCiConflictException(),
            NullLogger<GlobalExceptionHandlingMiddleware>.Instance
        );

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal("urn:dsc:person:ci-conflict", body.RootElement.GetProperty("type").GetString());
        Assert.DoesNotContain(body.RootElement.GetProperty("detail").GetString()!, char.IsDigit);
    }
}
