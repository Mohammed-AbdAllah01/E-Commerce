using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class EditDelivery
    {
        [ForeignKey("admin")]
        public required string adminId { get; set; }
        public required Admin admin { get; set; }

        [ForeignKey("delivery")]
        public required string deliveryId { get; set; }
        public required DeliveryRep delivery { get; set; }

        public DateTime EditDate { get; set; }
    }
}
