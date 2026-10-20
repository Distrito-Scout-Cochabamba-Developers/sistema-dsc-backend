using DSC.Application.Interfaces;

namespace DSC.Infrastructure;

/// <summary>
/// Hash de contraseñas con BCrypt (salt aleatorio incluido en el hash).
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    // Coste de trabajo de BCrypt. 12 es un valor razonable para producción.
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("La contraseña no puede estar vacía.", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string providedPassword, string passwordHash)
    {
        if (string.IsNullOrEmpty(providedPassword) || string.IsNullOrEmpty(passwordHash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash malformado o contraseña heredada en texto plano: se rechaza.
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
