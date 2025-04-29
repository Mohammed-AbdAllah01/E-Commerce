using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class Notification
    {
        [ForeignKey("product")]
        public int productId { get; set; }
        public required Product product { get; set; }

        [ForeignKey("customer")]
        public required string customerId { get; set; }
        public required Customer customer { get; set; }

    }
}
