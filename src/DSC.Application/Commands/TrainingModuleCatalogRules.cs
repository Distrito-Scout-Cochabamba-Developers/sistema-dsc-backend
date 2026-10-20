using DSC.Domain.Entities;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

/// <summary>
/// Reglas de unicidad del catálogo compartidas por Create y Update.
/// </summary>
internal static class TrainingModuleCatalogRules
{
    public static async Task EnsureValidAsync(
        ITrainingModuleRepository moduleRepository,
        TrainingModule module,
        long? excludeId,
        CancellationToken cancellationToken
    )
    {
        if (!await moduleRepository.ModuleTypeExistsAsync(module.ModuleTypeId, cancellationToken))
            throw new DomainException(
                $"No existe el tipo de módulo con identificador '{module.ModuleTypeId}'."
            );

        // `code` es UNIQUE en BD aunque el módulo esté inactivo
        if (await moduleRepository.ExistsByCodeAsync(module.Code, excludeId, cancellationToken))
            throw new InvalidOperationException(
                $"Ya existe un módulo con el código '{module.Code}'."
            );

        if (
            module.IsActive
            && await moduleRepository.ExistsActiveByNameAsync(
                module.Name,
                excludeId,
                cancellationToken
            )
        )
            throw new InvalidOperationException(
                $"Ya existe un módulo activo con el nombre '{module.Name}'."
            );
    }
}
