using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class Cart
    {

        public int Id { get; set; }

        [ForeignKey("product")]
        public int productId { get; set; }
        public Product product { get; set; }

        [ForeignKey("color")]
        public int colorId { get; set; }
        public Color? color { get; set; }

        [ForeignKey("size")]
        public int sizeId { get; set; }
        public Size size { get; set; }

        [ForeignKey("customer")]
        public required string customerId { get; set; }
        public Customer customer { get; set; }

        public int Quantity { get; set; }

    }
}
