using DSC.Domain.Entities;

namespace DSC.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
