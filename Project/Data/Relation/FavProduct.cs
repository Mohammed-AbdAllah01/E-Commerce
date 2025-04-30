using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class FavProduct
    {
        public int Id { get; set; }
        [ForeignKey("product")]
        public int productId { get; set; }
        public Product product { get; set; }

        [ForeignKey("customer")]
        public string customerId { get; set; }
        public Customer customer { get; set; }

    }
}
