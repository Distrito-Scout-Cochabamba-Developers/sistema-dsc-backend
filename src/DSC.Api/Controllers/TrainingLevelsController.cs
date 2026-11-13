using DSC.Application.Authorization;
using DSC.Application.Commands;
using DSC.Application.DTOs;
using DSC.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSC.Api.Controllers;

[ApiController]
[Authorize]
[Tags(OpenApiTags.TrainingLevels)]
[Route("v1/training-levels")]
public class TrainingLevelsController(
    ListTrainingLevelsQueryHandler listHandler,
    GetTrainingLevelRequirementsQueryHandler getHandler
) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Lista niveles de formación")]
    [EndpointDescription(
        "Requiere JWT. Incluye código, nombre, línea y estado del nivel, ordenados por nombre."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingLevelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TrainingLevelDto>>> List(
        CancellationToken cancellationToken
    ) => Ok(await listHandler.HandleAsync(cancellationToken));

    [HttpGet("{code}/requirements")]
    [EndpointSummary("Obtiene los requisitos estándar de un nivel")]
    [EndpointDescription(
        "Requiere JWT. Sin fechas de vigencia. Devuelve 200 con modules: null si el nivel existe sin configuración; 404 si el nivel no existe. No evalúa el historial del dirigente."
    )]
    [ProducesResponseType(typeof(TrainingLevelRequirementsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TrainingLevelRequirementsDto>> GetRequirements(
        [FromRoute] string code,
        CancellationToken cancellationToken
    ) => Ok(await getHandler.HandleAsync(code, cancellationToken));
}

[ApiController]
[Authorize(Roles = CatalogAdminRoles.PolicyRoles)]
[Tags(OpenApiTags.TrainingLevels)]
[Route("v1/admin/training-levels")]
public class AdminTrainingLevelsController(
    ConfigureTrainingLevelRequirementsCommandHandler configureHandler
) : ControllerBase
{
    [HttpPut("{code}/requirements")]
    [EndpointSummary("Configura el criterio de módulos de un nivel")]
    [EndpointDescription(
        "Requiere ADMIN o DISTRICT_SECRETARY. Total, mínimo obligatorio y mandatoryModuleCodes son obligatorios. Reemplaza solo MODULES. Cantidades no negativas; mínimo no supera total ni selección. Códigos existentes, únicos tras recortar espacios. El catálogo no dispone de estado activo de módulo. 400 para selección inválida; 404 para nivel inexistente; 409 para conflicto de persistencia."
    )]
    [ProducesResponseType(typeof(TrainingLevelRequirementsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TrainingLevelRequirementsDto>> ConfigureRequirements(
        [FromRoute] string code,
        [FromBody] WriteTrainingLevelRequirementsDto body,
        CancellationToken cancellationToken
    ) => Ok(await configureHandler.HandleAsync(code, body, cancellationToken));
}
