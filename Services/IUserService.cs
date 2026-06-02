using JwtDemo.Models;

namespace JwtDemo.Services;

public interface IUserService
{
    User? FindByUsername(string username);
    User? FindById(int id);
    bool Register(RegisterRequest request, out string error);
    void SaveRefreshToken(int userId, string refreshToken, DateTime expiry);
    (int userId, bool isValid) ValidateRefreshToken(string refreshToken);
    void RevokeRefreshToken(string refreshToken);
}
