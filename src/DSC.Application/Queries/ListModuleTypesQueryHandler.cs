using DSC.Application.DTOs;
using DSC.Domain.Repositories;

namespace DSC.Application.Queries;

public class ListModuleTypesQueryHandler(IModuleTypeRepository moduleTypeRepository)
{
    public async Task<IReadOnlyList<ModuleTypeDto>> HandleAsync(
        CancellationToken cancellationToken = default
    )
    {
        var types = await moduleTypeRepository.ListAsync(cancellationToken);
        return types
            .Select(type => new ModuleTypeDto(type.Id, type.Code, type.Name, type.Description))
            .ToArray();
    }
}
