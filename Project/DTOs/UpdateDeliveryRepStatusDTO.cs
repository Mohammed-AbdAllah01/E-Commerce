namespace Project.DTOs
{
    public class UpdateDeliveryRepStatusDTO
    {
        public string DeliveryRepId { get; set; }

        public string NewStatus { get; set; } // "Active", "Banned", "Inactive"
    }

}
