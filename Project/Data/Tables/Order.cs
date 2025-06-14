using Project.Data.Relation;
using Project.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Project.Tables
{
    public class Order
    {
        public int Id { get; set; }

        public required string address { get; set; } // address of the order
        public required string phone { get; set; } // phone number of the order

        [NotMapped]
        public OrdStatus Status
        {
            get
            {

                
                return  (OrdStatus)orderItems?.Average(item => (int)item.Status); }           
        }
        public required DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime DeliveryDate { get; set; }

        public ICollection<EditOrder>? EditOrders { get; set; } //Many To Many  Edit Order

        [ForeignKey("deliveryrep")]//Many To One 
        public required string DeliveryId { get; set; }
        public DeliveryRep? deliveryrep { get; set; }


        [ForeignKey("customer")] //Many To One
        public required string CustomerId { get; set; }
        public Customer? customer { get; set; }

        public ICollection <OrderItem>? orderItems { get; set; }//Many to Many

        
        public double TotalPrice { get; set; } // Total price of the order



    }
}
