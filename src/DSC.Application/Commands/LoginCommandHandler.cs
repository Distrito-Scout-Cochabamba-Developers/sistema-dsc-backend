using DSC.Application.DTOs;
using DSC.Application.Interfaces;
using DSC.Domain.Exceptions;
using DSC.Domain.Repositories;

namespace DSC.Application.Commands;

public class LoginCommandHandler(
    IUserRepository _userRepository,
    IPasswordHasher _passwordHasher,
    IJwtTokenGenerator _jwtTokenGenerator
)
{
    public async Task<AuthResultDto> HandleAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
            throw new DomainException("Las credenciales son inválidas.");

        var user = await _userRepository.GetByEmailWithRolesAsync(request.Email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.Password))
            throw new DomainException("Las credenciales son inválidas.");

        user.EnsureCanAuthenticate();

        var roles = user
            .UserRoles.Where(userRole => userRole.Role.IsActive)
            .Select(userRole => userRole.Role.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var profile = new UserProfileDto(user.Id, user.Email, roles);

        return new AuthResultDto(_jwtTokenGenerator.GenerateToken(user), profile);
    }
}
