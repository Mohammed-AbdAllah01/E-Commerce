using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class EditCustomer
    {
        [ForeignKey("admin")]
        public required string adminId { get; set; }
        public required Admin admin { get; set; }

        [ForeignKey("customer")]
        public required string customerId { get; set; }
        public required Customer customer { get; set; }

        public DateTime EditDate { get; set; }
    }
}
