using JwtDemo.Models;
using JwtDemo.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtDemo.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IUserService userService, IJwtService jwtService, IConfiguration config) : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register(RegisterRequest request)
    {
        if (!userService.Register(request, out var error))
            return BadRequest(new { error });

        return Ok(new { message = "Registered successfully." });
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var user = userService.FindByUsername(request.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { error = "Invalid username or password." });

        return Ok(IssueTokens(user));
    }

    [HttpPost("refresh")]
    public IActionResult Refresh(RefreshRequest request)
    {
        var (userId, isValid) = userService.ValidateRefreshToken(request.RefreshToken);
        if (!isValid)
            return Unauthorized(new { error = "Invalid or expired refresh token." });

        var user = userService.FindById(userId);
        if (user is null)
            return Unauthorized(new { error = "User not found." });

        // Xóa token cũ trước khi cấp mới (token rotation)
        userService.RevokeRefreshToken(request.RefreshToken);

        return Ok(IssueTokens(user));
    }

    [HttpPost("logout")]
    public IActionResult Logout(LogoutRequest request)
    {
        userService.RevokeRefreshToken(request.RefreshToken);
        return Ok(new { message = "Logged out successfully." });
    }

    private TokenResponse IssueTokens(User user)
    {
        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();
        var expiryDays = double.Parse(config["Jwt:RefreshTokenExpiryDays"]!);
        userService.SaveRefreshToken(user.Id, refreshToken, DateTime.UtcNow.AddDays(expiryDays));
        return new TokenResponse(accessToken, refreshToken);
    }
}
