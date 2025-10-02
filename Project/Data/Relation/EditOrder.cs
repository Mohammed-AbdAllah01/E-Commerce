using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class EditOrder
    {
        [ForeignKey("admin")]
        public required string adminId { get; set; }
        public required Admin admin { get; set; }

        [ForeignKey("order")]
        public int orderId { get; set; }
        public required Order order { get; set; }

        public DateTime EditDate { get; set; }

        public bool IsCanceled { get; set; }
        public bool PhoneChange { get; set; }
        public bool LocationChange { get; set; }
    }
}
