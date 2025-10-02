using Project.Enums;
using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Eventing.Reader;

namespace Project.Data.Relation
{
    public class History
    {
        [ForeignKey("customer")]
        public required string customerId { get; set; }
        public  Customer customer { get; set; }

        [ForeignKey("product")]
        public int productId { get; set; }
        public  Product product { get; set; }

        public EventStatus event_type { get; set; }
        

    }
}
