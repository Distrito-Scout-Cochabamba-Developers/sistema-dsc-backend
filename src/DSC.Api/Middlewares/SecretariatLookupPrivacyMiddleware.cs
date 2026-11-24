namespace DSC.Api.Middlewares;

/// <summary>Prevents caching lookup responses, even when authorization or binding short-circuits.</summary>
public class SecretariatLookupPrivacyMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        if (
            string.Equals(
                context.Request.Path.Value?.TrimEnd('/'),
                "/api/v1/secretariat/participants/lookup",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.CacheControl = "no-store";
                return Task.CompletedTask;
            });
        }

        return next(context);
    }
}
