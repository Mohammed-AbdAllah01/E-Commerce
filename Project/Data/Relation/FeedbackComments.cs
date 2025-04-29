using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class FeedbackComments
    {
        [ForeignKey("product")]
        public int productId { get; set; }
        public required Product product { get; set; }

        [ForeignKey("customer")]
        public required string customerId { get; set; }
        public required Customer customer { get; set; }

        public required string Comment { get; set; }

        public DateTime DateCreate { get; set; } = DateTime.Now;
        public required double Feeling { get; set; }


    }
}
