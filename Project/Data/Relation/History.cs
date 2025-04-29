using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class History
    {
        [ForeignKey("customer")]
        public required string customerId { get; set; }
        public required Customer customer { get; set; }

        [ForeignKey("product")]
        public int productId { get; set; }
        public required Product product { get; set; }

        public bool IsAddedFav { get; set; }
        public bool IsBrowsing { get; set; }

        public bool IsAddedCart { get; set; }
    }
}
