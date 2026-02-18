using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RentalHub.Model
{
    public class Order
    {
        [Key]
        [Column("orderid")]
        public int OrderId { get; set; }

        [Required]
        [Column("itemid")]
        public int ItemId { get; set; }

        public Item Item { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("clientname")]
        public string ClientName { get; set; }

        [MaxLength(200)]
        [Column("village")]
        public string Village { get; set; }

        [Required]
        [Column("fromdate")]
        public DateTime FromDate { get; set; }

        [Required]
        [Column("todate")]
        public DateTime ToDate { get; set; }

        [Column("rent", TypeName = "numeric(10,2)")]
        public decimal Rent { get; set; }

        [Column("advance_taken_by")]
        public int? AdvanceTakenBy { get; set; }

        [Column("advance", TypeName = "numeric(10,2)")]
        public decimal Advance { get; set; }

        [Column("remaining", TypeName = "numeric(10,2)")]
        public decimal Remaining { get; set; }

        [Column("remaining_taken_by")]
        public int? RemainingTakenBy { get; set; }

        [Column("remaining_amount", TypeName = "numeric(10,2)")]
        public decimal? RemainingAmount { get; set; }

        [Column("remark")]
        public string? Remark { get; set; }
        //[Column("fullpayment")]
        //public bool FullPayment { get; set; } = false;

        [Required]
        [MaxLength(15)]
        [Column("mobilenumber")]
        public string MobileNumber { get; set; } = string.Empty;

        [Column("createddate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
