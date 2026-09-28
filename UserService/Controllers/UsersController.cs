using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace UserService.Controllers
{
    [ApiController]
    [Route("users")]
    [Authorize] // Bắt buộc phải có JWT Token hợp lệ mới truy cập được
    public class UsersController : ControllerBase
    {
        // Giả lập danh sách User trong DB
        private static readonly List<object> UserList = new()
        {
            new { Id = 1, Username = "admin", Email = "admin@example.com", Role = "Admin" },
            new { Id = 2, Username = "john_doe", Email = "john@example.com", Role = "User" },
            new { Id = 3, Username = "jane_doe", Email = "jane@example.com", Role = "User" }
        };

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            // Lấy thông tin User hiện tại từ JWT Token
            var currentUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst(ClaimTypes.Name)?.Value;

            return Ok(new
            {
                Message = $"Xác thực thành công! Xin chào {currentUsername}",
                Data = UserList
            });
        }

        [HttpGet("me")]
        public IActionResult GetProfile()
        {
            var username = User.Identity?.Name;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Ok(new
            {
                UserId = userId,
                Username = username,
                Claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }
    }
}