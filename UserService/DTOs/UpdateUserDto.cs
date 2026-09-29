namespace UserService.DTOs
{
    public class UpdateUserDto
    {
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Sdt { get; set; }
        public string? Role { get; set; } // Quyền mới (chỉ Admin mới được chỉnh)
    }
}
