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
    }
}
