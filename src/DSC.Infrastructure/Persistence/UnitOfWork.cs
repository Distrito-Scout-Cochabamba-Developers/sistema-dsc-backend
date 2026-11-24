using System.Data;
using DSC.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DSC.Infrastructure.Persistence;

/// <summary>
/// Transacción única sobre el <see cref="ScoutDbContext"/> actual. Usa la
/// estrategia de reintentos configurada por Npgsql y se une a una transacción
/// ambiente cuando ya existe.
/// </summary>
public class UnitOfWork(ScoutDbContext dbContext) : IUnitOfWork
{
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !dbContext.Database.IsRelational()
            || dbContext.Database.CurrentTransaction is not null
        )
            return await operation(cancellationToken);

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken
            );
            try
            {
                var result = await operation(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                dbContext.ChangeTracker.Clear();
                throw;
            }
        });
    }
}
