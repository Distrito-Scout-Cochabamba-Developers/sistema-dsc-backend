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
    public void CreateModuleSession_ShouldBeHttpPostUnderModules()
    {
        var method = typeof(AdminCatalogController).GetMethod(
            nameof(AdminCatalogController.CreateModuleSession)
        );
        var httpPost = method?.GetCustomAttribute<HttpPostAttribute>();
        Assert.Equal("modules/{id:long}/sessions", httpPost?.Template);
    }

    [Fact]
    public void Attendance_ShouldUseAttendanceOpenApiTag()
    {
        var tags = typeof(AttendanceController).GetCustomAttribute<TagsAttribute>();
        Assert.Equal(OpenApiTags.Attendance, tags?.Tags.Single());
    }

    [Fact]
    public void AdminTrainingLevelsController_ShouldRequireTrainingLevelAdminRolesUnderV1AdminTrainingLevels()
    {
        var route = typeof(AdminTrainingLevelsController).GetCustomAttribute<RouteAttribute>();
        var authorize = typeof(AdminTrainingLevelsController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.Equal("v1/admin/training-levels", route?.Template);
        Assert.Equal(TrainingLevelAdminRoles.PolicyRoles, authorize?.Roles);
        var tags = typeof(AdminTrainingLevelsController).GetCustomAttribute<TagsAttribute>();
        Assert.Equal(OpenApiTags.TrainingLevels, tags?.Tags.Single());
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
    public void ListLines_ShouldBeHttpGetTrainingLines()
    {
        var method = typeof(AdminCatalogController).GetMethod(
            nameof(AdminCatalogController.ListTrainingLines)
        );
        Assert.Equal("training-lines", method?.GetCustomAttribute<HttpGetAttribute>()?.Template);
    }

    [Fact]
    public void Delete_ShouldBeHttpDeleteModulesById()
    {
        var method = typeof(AdminCatalogController).GetMethod(
            nameof(AdminCatalogController.DeleteModule)
        );
        var httpDelete = method?.GetCustomAttribute<HttpDeleteAttribute>();
        Assert.Equal("modules/{id:long}", httpDelete?.Template);
    }
}
