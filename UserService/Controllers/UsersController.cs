using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Data;
using UserService.DTOs;

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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.LtmUsers
                .Select(u => new { u.Username, u.Email, u.Sdt, u.Role })
                .ToListAsync();

            return Ok(new
            {
                Message = $"Xác thực Admin thành công! Xin chào {User.Identity?.Name}",
                TotalUsers = users.Count,
                Data = users
            });
        }

        [HttpGet("{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var user = await _context.LtmUsers
                .Where(u => u.Username == username)
                .Select(u => new { u.Username, u.Email, u.Sdt, u.Role })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = $"Không tìm thấy người dùng '{username}'." });
            }

            return Ok(user);
        }

        // 3. PUT: /users/{username} - Cập nhật thông tin (Email, SĐT, Password)
        [HttpPut("{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string username, [FromBody] UpdateUserDto request)
        {
            var user = await _context.LtmUsers.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound(new { message = $"Không tìm thấy người dùng '{username}' để cập nhật." });
            }

            // Cập nhật các thông tin cơ bản
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.Password = request.Password;
            }
            if (request.Email != null)
            {
                user.Email = request.Email;
            }
            if (request.Sdt != null)
            {
                user.Sdt = request.Sdt;
            }

            // --- LOGIC BẢO VỆ QUYỀN ADMIN ---
            if (!string.IsNullOrEmpty(request.Role))
            {
                // Nếu đối tượng được chỉnh sửa là tài khoản 'admin' gốc hệ thống
                // HOẶC tài khoản đó vốn đang có Role là 'Admin'
                // Khung logic này ngăn chặn việc hạ quyền bất kỳ tài khoản Admin nào thành khác 'Admin'
                if (user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
                   !request.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Tài khoản có quyền Admin không thể thay đổi thành quyền khác!" });
                }

                user.Role = request.Role;
            }

            _context.LtmUsers.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Cập nhật thông tin người dùng '{username}' thành công!" });
        }

        // 4. DELETE: /users/{username} - Xóa người dùng
        [HttpDelete("{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var user = await _context.LtmUsers.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound(new { message = $"Không tìm thấy người dùng '{username}' để xóa." });
            }

            // Bảo vệ: Không cho phép xóa tài khoản 'admin' hệ thống
            if (username.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Không thể xóa tài khoản Admin hệ thống!" });
            }

            _context.LtmUsers.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Đã xóa người dùng '{username}' khỏi cơ sở dữ liệu." });
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