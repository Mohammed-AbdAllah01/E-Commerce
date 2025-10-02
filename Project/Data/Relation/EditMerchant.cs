using Project.Tables;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Data.Relation
{
    public class EditMerchant
    {
        [ForeignKey("admin")]
        public required string adminId { get; set; }
        public required Admin admin { get; set; }

        [ForeignKey("merchant")]
        public required string merchantId { get; set; }
        public required Merchant merchant { get; set; }

        public DateTime EditDate { get; set; }
    }
}
