namespace Project.DTOs
{
    public class OrderItemDTO
    {
        public int Id { get; set; }              
        public string ProductName { get; set; } 
        public string Color { get; set; } 
        public string Size { get; set; } 
        public int Quantity { get; set; }
        public string MerchantName { get; set; } 
        public string Status { get; set; }

        public double Price { get; set; }
        public string ImageUrl { get; set; }
    }
}
