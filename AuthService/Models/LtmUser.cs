using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    [Table("ltm")]
    public class LtmUser
    {
        [Key]
        [Column("username")]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Column("password")]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [Column("email")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Column("sdt")]
        [StringLength(20)]
        public string? Sdt { get; set; }

        [Column("role")]
        [StringLength(20)]
        public string Role { get; set; } = "User";
    }
}
