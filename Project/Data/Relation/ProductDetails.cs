using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class ProductDetail
    {
        public int Id { get; set; }
        [ForeignKey("product")]
        public int productId { get; set; }
        public  Product product { get; set; }

        [ForeignKey("color")]
        public int colorId { get; set; }
        public  Color color { get; set; }

        [ForeignKey("size")]
        public int sizeId { get; set; }
        public  Size size { get; set; }

        public int Quantity { get; set; }



    }
}
