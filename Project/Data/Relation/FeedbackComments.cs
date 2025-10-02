using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class FeedbackComments
    {
        [ForeignKey("product")]
        public int productId { get; set; }
        public Product product { get; set; }

        [ForeignKey("customer")]
        public required string customerId { get; set; }
        public Customer customer { get; set; }

        public required string OriginalComment { get; set; }
        public required string TranslateComment { get; set; }

        public DateTime DateCreate { get; set; } = DateTime.Now;
        public required double CommentRate { get; set; }


    }
}
