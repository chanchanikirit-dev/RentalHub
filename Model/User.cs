using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RentalHub.Model
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("userid")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("username")]
        public string Username { get; set; }

        [Required]
        [Column("passwordhash")]
        public string PasswordHash { get; set; }

        [MaxLength(20)]
        [Column("role")]
        public string Role { get; set; } = "Admin";

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("createddate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
