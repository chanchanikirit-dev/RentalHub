using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RentalHub.Model
{
    [Table("items")]
    public class Item
    {
        [Key]
        [Column("itemid")]
        public int ItemId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("itemcode")]
        public string ItemCode { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("itemname")]
        public string ItemName { get; set; }

        [Column("photourl")]
        public string? PhotoUrl { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("createddate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
