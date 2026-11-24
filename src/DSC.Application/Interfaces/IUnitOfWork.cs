namespace DSC.Application.Interfaces;

/// <summary>
/// Frontera transaccional reutilizable. Permite que una operación compuesta
/// (por ejemplo, alta de persona + inscripción de #158/#160) y la inscripción
/// compartida se confirmen o reviertan juntas. Si ya hay una transacción
/// ambiente, la operación se une a ella.
/// </summary>
public interface IUnitOfWork
{
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default
    );
}
