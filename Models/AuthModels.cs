namespace JwtDemo.Models;

public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Password, string Role = Roles.User);
public record RefreshRequest(string RefreshToken);

public record TokenResponse(string AccessToken, string RefreshToken, string TokenType = "Bearer");
