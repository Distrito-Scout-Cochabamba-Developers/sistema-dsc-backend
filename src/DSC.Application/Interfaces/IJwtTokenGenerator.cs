using DSC.Application.DTOs;
using DSC.Domain.Entities;

namespace DSC.Application.Interfaces;

public interface IJwtTokenGenerator
{
    JwtTokenResult GenerateToken(User user);
}
