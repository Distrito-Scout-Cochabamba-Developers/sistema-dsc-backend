using DSC.Application.Interfaces;

namespace DSC.Infrastructure;

public class NormalPasswordValidation : IPasswordHasher
{
    public string Hash(string password) => password;

    public bool Verify(string providedPassword, string passwordHash) =>
        providedPassword == passwordHash;
}
