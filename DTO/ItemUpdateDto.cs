namespace RentalHub.DTO
{
    public class ItemUpdateDto
    {
        public string ItemName { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
