using JwtDemo.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JwtDemo.Services;

public class JwtService(IConfiguration config) : IJwtService
{
    // Tạo access token JWT chứa userId, username, role làm claims
    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:AccessTokenExpiryMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Refresh token là chuỗi random — không chứa dữ liệu, được lưu server-side
    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    // Validate access token (dùng khi cần đọc claims từ token hết hạn, ví dụ trong refresh flow)
    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = config["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            // Tắt validate thời gian hết hạn vì token đã hết hạn khi gọi refresh
            ValidateLifetime = false,
        };

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, validationParams, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
