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
        if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrEmpty(request.Password))
            throw new RequestValidationException(
                "El identificador y la contraseña son obligatorios."
            );

        var user = await _userRepository.GetByIdentifierWithRolesAsync(
            request.Identifier,
            cancellationToken
        );

        // Mismo error para usuario inexistente, contraseña incorrecta o cuenta no habilitada.
        if (user is null || !_passwordHasher.Verify(request.Password, user.Password))
            throw new InvalidCredentialsException();

        user.EnsureCanAuthenticate();

        var roles = user.GetActiveRoleCodes(DateOnly.FromDateTime(DateTime.UtcNow));
        var profile = new UserProfileDto(user.Id, user.PersonId, user.Username, user.Email, roles);
        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResultDto(token.Token, token.ExpiresAt, profile);
    }
}
