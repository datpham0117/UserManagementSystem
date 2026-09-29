using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Data;

namespace UserService.Controllers
{
    [ApiController]
    [Route("users")]
    [Authorize] // Bắt buộc phải có JWT Token hợp lệ mới truy cập được
    public class UsersController : ControllerBase
    {
        private readonly UserDbContext _context;

        public UsersController(UserDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.LtmUsers
                .Select(u => new { u.Username })
                .ToListAsync();

            return Ok(new
            {
                Message = $"Xác thực JWT thành công! Xin chào {User.Identity?.Name}",
                TotalUsers = users.Count,
                Data = users
            });
        }

        [HttpGet("GetProfile")]
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