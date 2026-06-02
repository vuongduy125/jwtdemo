using JwtDemo.Models;

namespace JwtDemo.Services;

// In-memory store — thay bằng DB thật (EF Core) trong production
public class UserService : IUserService
{
    private readonly List<User> _users = [];
    private readonly Dictionary<string, (int UserId, DateTime Expiry)> _refreshTokens = [];
    private int _nextId = 1;

    public UserService()
    {
        // Seed dữ liệu mẫu
        Register(new RegisterRequest("admin", "Admin@123", Roles.Admin), out _);
        Register(new RegisterRequest("alice", "Alice@123", Roles.User), out _);
    }

    public User? FindByUsername(string username) =>
        _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

    public User? FindById(int id) =>
        _users.FirstOrDefault(u => u.Id == id);

    public bool Register(RegisterRequest request, out string error)
    {
        if (FindByUsername(request.Username) is not null)
        {
            error = "Username already exists.";
            return false;
        }

        var role = request.Role is Roles.Admin or Roles.User ? request.Role : Roles.User;

        _users.Add(new User
        {
            Id = _nextId++,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
        });

        error = string.Empty;
        return true;
    }

    public void SaveRefreshToken(int userId, string refreshToken, DateTime expiry)
    {
        // Xóa token cũ của user (single-session per user)
        var oldToken = _refreshTokens.FirstOrDefault(kv => kv.Value.UserId == userId).Key;
        if (oldToken is not null)
            _refreshTokens.Remove(oldToken);

        _refreshTokens[refreshToken] = (userId, expiry);
    }

    public (int userId, bool isValid) ValidateRefreshToken(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var entry))
            return (0, false);

        if (entry.Expiry < DateTime.UtcNow)
        {
            _refreshTokens.Remove(refreshToken);
            return (0, false);
        }

        return (entry.UserId, true);
    }

    public void RevokeRefreshToken(string refreshToken) =>
        _refreshTokens.Remove(refreshToken);
}
