using System.Security.Claims;
using DSC.Application.Interfaces;

namespace DSC.Api;

public class CurrentUser(IHttpContextAccessor _httpContextAccessor) : ICurrentUser
{
    private const string SubClaim = "sub";
    private const string PersonIdClaim = "personId";
    private const string UsernameClaim = "username";
    private const string EmailClaim = "email";
    private const string RolesClaim = "roles";

    public long? Id => ParseLong(SubClaim);

    public long? PersonId => ParseLong(PersonIdClaim);

    public string? Username =>
        _httpContextAccessor?.HttpContext?.User?.FindFirstValue(UsernameClaim);

    public string? Email => _httpContextAccessor?.HttpContext?.User?.FindFirstValue(EmailClaim);

    public IEnumerable<string> Roles =>
        _httpContextAccessor?.HttpContext?.User?.FindAll(RolesClaim).Select(claim => claim.Value)
        ?? Enumerable.Empty<string>();

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;

    private long? ParseLong(string claimType) =>
        long.TryParse(
            _httpContextAccessor?.HttpContext?.User?.FindFirstValue(claimType),
            out var value
        )
            ? value
            : null;
}
