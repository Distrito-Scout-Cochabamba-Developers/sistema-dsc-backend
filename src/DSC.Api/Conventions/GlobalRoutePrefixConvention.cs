using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace DSC.Api.Conventions;

/// <summary>
/// Aplica un prefijo global (por ejemplo 'api') a todos los controladores de la aplicación.
/// Evita duplicar el prefijo si el controlador ya lo define explícitamente.
/// </summary>
public class GlobalRoutePrefixConvention(IRouteTemplateProvider routeTemplateProvider)
    : IApplicationModelConvention
{
    private readonly AttributeRouteModel _routePrefix = new(routeTemplateProvider);

    public void Apply(ApplicationModel application)
    {
        foreach (var selector in application.Controllers.SelectMany(c => c.Selectors))
        {
            if (selector.AttributeRouteModel != null)
            {
                var template = selector.AttributeRouteModel.Template;
                if (
                    !string.IsNullOrEmpty(template)
                    && (
                        template.StartsWith("api/", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(template, "api", StringComparison.OrdinalIgnoreCase)
                    )
                )
                {
                    continue;
                }

                selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                    _routePrefix,
                    selector.AttributeRouteModel
                );
            }
            else
            {
                selector.AttributeRouteModel = _routePrefix;
            }
        }
    }
}
