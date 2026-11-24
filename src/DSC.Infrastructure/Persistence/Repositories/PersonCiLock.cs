namespace DSC.Infrastructure.Persistence.Repositories;

/// <summary>
/// Clave del advisory lock transaccional que serializa el alta de personas por CI en todos los canales
/// (asistencia digital, inscripción asistida y alta de Rover). Usa la forma de dos enteros
/// <c>pg_advisory_xact_lock(int, int)</c>: el primero es el espacio de nombres reservado de la aplicación
/// y el segundo <c>hashtext(clave)</c>. Así no choca con otros usos de la forma de un bigint y no depende
/// del largo del CI. Una colisión de hash solo serializa de más dos CI distintos; la unicidad la garantiza
/// el índice <c>ux_person_ci_extension</c>.
/// </summary>
internal static class PersonCiLock
{
    /// <summary>Espacio de nombres (primer entero) reservado para locks por CI de persona. No reutilizar.</summary>
    public const int Namespace = 158_001;

    public static string KeyFor(string ciNumber) => "person-ci:" + ciNumber.Trim();
}
