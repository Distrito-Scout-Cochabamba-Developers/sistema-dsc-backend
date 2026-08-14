using DSC.Domain.Entities;

namespace DSC.Domain.Repositories;

/// <summary>
/// Contrato de persistencia y consulta para personas y dirigentes scouts.
/// </summary>
public interface IScoutLeaderRepository
{
    Task<Person?> GetByCiNumberAsync(string ciNumber, CancellationToken cancellationToken);
    Task<Person?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task AddAsync(Person person, CancellationToken cancellationToken);
    Task UpdateAsync(Person person, CancellationToken cancellationToken);
}
