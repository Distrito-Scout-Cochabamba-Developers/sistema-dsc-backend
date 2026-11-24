using System.Reflection;
using DSC.Api;
using DSC.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DSC.UnitTests.Api;

public class CalendarControllerRouteTests
{
    [Fact]
    public void Controller_ShouldUseCalendarTagUnderV1Calendar()
    {
        var route = typeof(CalendarController).GetCustomAttribute<RouteAttribute>();
        var tags = typeof(CalendarController).GetCustomAttribute<TagsAttribute>();

        Assert.Equal("v1/calendar", route?.Template);
        Assert.Equal(OpenApiTags.Calendar, tags?.Tags.Single());
        Assert.Null(typeof(CalendarController).GetCustomAttribute<AuthorizeAttribute>());
    }

    [Fact]
    public void ListSessions_ShouldBeAnonymousGet()
    {
        var method = typeof(CalendarController).GetMethod(nameof(CalendarController.ListSessions));
        Assert.Equal("sessions", method?.GetCustomAttribute<HttpGetAttribute>()?.Template);
        Assert.NotNull(method?.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Null(method?.GetCustomAttribute<AuthorizeAttribute>());
    }

    [Fact]
    public void Enroll_ShouldRequireBearer()
    {
        var method = typeof(CalendarController).GetMethod(nameof(CalendarController.Enroll));
        Assert.Equal(
            "sessions/{id:long}/enrollments",
            method?.GetCustomAttribute<HttpPostAttribute>()?.Template
        );
        Assert.NotNull(method?.GetCustomAttribute<AuthorizeAttribute>());
    }
}
