using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

public interface IModuleTypeRepository
{
    Task<IReadOnlyList<ModuleType>> ListAsync(CancellationToken cancellationToken);

    Task<ModuleType?> GetByCodeAsync(string code, CancellationToken cancellationToken);
}
