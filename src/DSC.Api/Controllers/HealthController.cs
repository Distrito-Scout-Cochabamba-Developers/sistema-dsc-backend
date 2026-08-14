using DSC.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DSC.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController(ScoutDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Verifica el estado de salud de la API y la conectividad con la base de datos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CheckHealth(CancellationToken cancellationToken)
    {
        var isDbConnected = false;
        string? dbProvider = null;
        string? errorMessage = null;

        try
        {
            dbProvider = dbContext.Database.ProviderName;
            isDbConnected = await dbContext.Database.CanConnectAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }

        var response = new
        {
            Status = isDbConnected ? "Healthy" : "Degraded",
            Timestamp = DateTime.UtcNow,
            Database = new
            {
                Provider = dbProvider,
                CanConnect = isDbConnected,
                Error = errorMessage
            }
        };

        if (!isDbConnected)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);

        return Ok(response);
    }

    /// <summary>
    /// Diagnóstico rápido de tablas y conteo de registros en el catálogo.
    /// </summary>
    [HttpGet("db-check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DiagnosticCheck(CancellationToken cancellationToken)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Status = "Error",
                    Message = "No se puede conectar a la base de datos."
                });
            }

            var moduleTypesCount = await dbContext.ModuleTypes.CountAsync(cancellationToken);
            var branchesCount = await dbContext.Branches.CountAsync(cancellationToken);
            var rolesCount = await dbContext.Roles.CountAsync(cancellationToken);
            var scoutGroupsCount = await dbContext.ScoutGroups.CountAsync(cancellationToken);
            var personsCount = await dbContext.Persons.CountAsync(cancellationToken);

            return Ok(new
            {
                Status = "Connected",
                Provider = dbContext.Database.ProviderName,
                CatalogSummary = new
                {
                    ModuleTypes = moduleTypesCount,
                    Branches = branchesCount,
                    Roles = rolesCount,
                    ScoutGroups = scoutGroupsCount,
                    Persons = personsCount
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                Status = "Exception",
                Error = ex.Message
            });
        }
    }
}
