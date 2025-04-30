using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class Image
    {
        [ForeignKey("product")]
        public int productId { get; set; }
        public  Product product { get; set; }

        [ForeignKey("color")]
        public int colorId { get; set; }
        public  Color color { get; set; }

        public  string ImageData { get; set; }

    }
}
