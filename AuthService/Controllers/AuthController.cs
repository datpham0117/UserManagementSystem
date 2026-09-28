using AuthService.DTOs;
using AuthService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        // Giả lập Database trong bộ nhớ (In-Memory Data)
        private static readonly List<User> Users = new();
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto request)
        {
            if (Users.Any(u => u.Username == request.Username))
            {
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });
            }

            // Mã hóa mật khẩu (sử dụng BCrypt hoặc PasswordHasher trong thực tế)
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Id = Users.Count + 1,
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = "User"
            };

            Users.Add(newUser);

            return Ok(new { message = "Đăng ký tài khoản thành công!" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto request)
        {
            var user = Users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác." });
            }

            var token = GenerateJwtToken(user);
            return Ok(new TokenResponseDto { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryInMinutes"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}