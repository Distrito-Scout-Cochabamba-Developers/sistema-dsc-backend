using System.Reflection;
using DSC.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace DSC.UnitTests.Api;

public class AuthControllerRouteTests
{
    [Fact]
    public void Login_ShouldBeExposedUnderV1AuthRoute()
    {
        var controllerRoute = typeof(AuthController).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(controllerRoute);
        Assert.Equal("v1/auth", controllerRoute.Template);

        var loginMethod = typeof(AuthController).GetMethod(nameof(AuthController.Login));

        Assert.NotNull(loginMethod);
        var httpPost = loginMethod.GetCustomAttribute<HttpPostAttribute>();
        Assert.NotNull(httpPost);
        Assert.Equal("login", httpPost.Template);
    }
}
