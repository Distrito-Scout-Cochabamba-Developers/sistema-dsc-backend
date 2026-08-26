namespace DSC.Application.Interfaces;

public interface IPasswordHasher
{
    bool Verify(string providedPassword, string passwordHash);
    string Hash(string password);
}
