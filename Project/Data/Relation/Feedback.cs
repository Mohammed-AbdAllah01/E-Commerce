using Project.Tables;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class Feedback
    { 
        public int Id { get; set; }
       
        [ForeignKey("product")]
        public int productId { get; set; }
        public required Product product { get; set; }

        [ForeignKey("customer")]
        public required  string customerId { get; set; }
        public required Customer customer { get; set; }

        [Range(1, 5, ErrorMessage = "Star rating must be between 1 and 5.")]
        public int Star { get; set; }

    }
}
