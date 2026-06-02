using JwtDemo.Models;
using System.Security.Claims;

namespace JwtDemo.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateAccessToken(string token);
}
