using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class FavMerchant
    {
        public int Id { get; set; }
        [ForeignKey("merchant")]
        public required string merchantId { get; set; }
        public Merchant merchant { get; set; }

        [ForeignKey("customer")]
        public required string customerId { get; set; }
        public Customer customer { get; set; }

    }
}
