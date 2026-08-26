using System.Security.Claims;
using DSC.Application.Interfaces;

namespace DSC.Api;

public class CurrentUser(IHttpContextAccessor _httpContextAccessor) : ICurrentUser
{
    //! Modificar a UUID
    public long? Id =>
        long.TryParse(
            _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier),
            out var id
        )
            ? id
            : null;

    public string? Username =>
        _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public string? Email =>
        _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public IEnumerable<string> Roles =>
        _httpContextAccessor
            ?.HttpContext?.User?.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
        ?? Enumerable.Empty<string>();

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
}
