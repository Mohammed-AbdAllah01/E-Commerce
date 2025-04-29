using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class Image
    {
        [ForeignKey("product")]
        public int productId { get; set; }
        public required Product product { get; set; }

        [ForeignKey("color")]
        public int colorId { get; set; }
        public required Color color { get; set; }

        public required string ImageData { get; set; }

    }
}
