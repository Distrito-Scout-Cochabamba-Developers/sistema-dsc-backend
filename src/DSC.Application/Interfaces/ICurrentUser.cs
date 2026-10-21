namespace DSC.Application.Interfaces;

public interface ICurrentUser
{
    long? Id { get; }
    long? PersonId { get; }
    string? Username { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
