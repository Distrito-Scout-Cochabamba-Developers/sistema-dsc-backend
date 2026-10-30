using System.Reflection;
using DSC.Api;
using DSC.Api.Controllers;
using DSC.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DSC.UnitTests.Api;

public class AdminCatalogControllerRouteTests
{
    [Fact]
    public void Controller_ShouldRequireCatalogAdminRolesUnderV1Admin()
    {
        var route = typeof(AdminCatalogController).GetCustomAttribute<RouteAttribute>();
        var authorize = typeof(AdminCatalogController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.Equal("v1/admin", route?.Template);
        Assert.Equal(CatalogAdminRoles.PolicyRoles, authorize?.Roles);
        var tags = typeof(AdminCatalogController).GetCustomAttribute<TagsAttribute>();
        Assert.Equal(OpenApiTags.Modules, tags?.Tags.Single());
    }

    [Fact]
    public void Attendance_ShouldUseAttendanceOpenApiTag()
    {
        var tags = typeof(AttendanceController).GetCustomAttribute<TagsAttribute>();
        Assert.Equal(OpenApiTags.Attendance, tags?.Tags.Single());
    }

    [Fact]
    public void Create_ShouldBeHttpPostModules()
    {
        var method = typeof(AdminCatalogController).GetMethod(
            nameof(AdminCatalogController.CreateModule)
        );
        var httpPost = method?.GetCustomAttribute<HttpPostAttribute>();
        Assert.Equal("modules", httpPost?.Template);
    }

    [Fact]
    public void Delete_ShouldBeHttpDeleteModulesByCode()
    {
        var method = typeof(AdminCatalogController).GetMethod(
            nameof(AdminCatalogController.DeleteModule)
        );
        var httpDelete = method?.GetCustomAttribute<HttpDeleteAttribute>();
        Assert.Equal("modules/{code}", httpDelete?.Template);
    }
}
