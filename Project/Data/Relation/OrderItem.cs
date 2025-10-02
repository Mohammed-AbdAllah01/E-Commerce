using Project.Enums;
using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;


namespace Project.Data.Relation
{
    public class OrderItem
    {
        public int Id { get; set; }
        [ForeignKey("order")]
        public int orderId { get; set; }
        public required Order order { get; set; }

        [ForeignKey("product")]
        public int productId { get; set; }
        public required Product product { get; set; }


        [ForeignKey("color")]
        public int colorId { get; set; }
        public required Color color { get; set; }

        [ForeignKey("size")]
        public int sizeId { get; set; }
        public required Size size { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("merchant")] //Many To One 
        public required string MerchantId { get; set; }
        public Merchant? merchant { get; set; }

        public OrdStatus Status { get; set; } = OrdStatus.Pending;

        public double UnitPrice { get; set; } // Price of the product at the time of order


    }
}
