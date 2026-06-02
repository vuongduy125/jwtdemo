using JwtDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JwtDemo.Controllers;

[ApiController]
[Route("resource")]
public class ResourceController : ControllerBase
{
    // Không cần token
    [HttpGet("public")]
    public IActionResult GetPublic() =>
        Ok(new { message = "Đây là endpoint công khai, ai cũng truy cập được." });

    // Cần JWT hợp lệ (bất kỳ role)
    [Authorize]
    [HttpGet("user")]
    public IActionResult GetUser()
    {
        var username = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);
        return Ok(new { message = $"Xin chào {username}! Bạn có role: {role}" });
    }

    // Chỉ Admin mới truy cập được
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin")]
    public IActionResult GetAdmin()
    {
        var username = User.FindFirstValue(ClaimTypes.Name);
        return Ok(new { message = $"Admin panel — chào {username}. Chỉ Admin mới thấy được trang này." });
    }

    // Xem tất cả claims trong token (hữu ích để debug)
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }
}
