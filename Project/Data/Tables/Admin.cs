using Microsoft.EntityFrameworkCore;
using Project.Data.Relation;
using System.ComponentModel.DataAnnotations.Schema;


namespace Project.Tables
{
    [Index("NationalId",IsUnique = true)]
    public class Admin : Person
    {
        

        public required long NationalId { get; set; }

        public required DateTime BirthDate { get; set; }

        [NotMapped]  // Exclude Age from the database
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                int age = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-age))
                {
                    age--;
                }
                return age;
            }
        }


        public ICollection<DeliveryRep>? AddDeliveries { get; set; } //One To Many Add DeliveryRep 

        public ICollection<EditDelivery>? EditDeliveries { get; set; } //Many To Many  Edit DeliveryRep

        public ICollection<EditCustomer>? EditCustomers { get; set; } //Many To Many  Edit Customer

        public ICollection<EditMerchant>? EditMerchants { get; set; } //Many To Many  Edit Merchant

        public ICollection<EditOrder>? EditOrders { get; set; } //Many To Many  Edit Order


    }
}
