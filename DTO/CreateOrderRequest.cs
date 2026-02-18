using System.ComponentModel.DataAnnotations;

namespace RentalHub
{
    public class CreateOrderRequest
    {
        public int ItemId { get; set; }
        public string ClientName { get; set; }
        public string Village { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal Rent { get; set; }
        public int? AdvanceTakenBy { get; set; }
        public decimal Advance { get; set; }
        public int? RemainingTakenBy { get; set; }
        public decimal? RemainingAmount { get; set; }
        public string Remark { get; set; }
        //public bool FullPayment { get; set; }
        public string MobileNumber { get; set; }
    }

}
