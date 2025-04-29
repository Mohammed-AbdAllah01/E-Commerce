using Project.Data.Relation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Tables
{
    public class DeliveryRep : Person
    {
        [StringLength(14)]
        public required long NationalId { get; set; }

        public  DateTime HireDate { get; set; } = DateTime.Now;

        [NotMapped]  // Exclude Age from the database
        public int HireAge
        {
            get
            {
                var today = DateTime.Today;
                int HireAge = today.Year - HireDate.Year;
                if (HireDate.Date > today.AddYears(-HireAge))
                {
                    HireAge--;
                }
                return HireAge;
            }
        }

        public required DateTime BirthDate { get; set; }

        [NotMapped]  // Exclude Age from the database
        public int BirthAge
        {
            get
            {
                var today = DateTime.Today;
                int BirthAge = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-BirthAge))
                {
                    BirthAge--;
                }
                return BirthAge;
            }
        }


        [ForeignKey("admin")] //Many To One Admin
        public required string adminId { get; set; }
        public  Admin? admin { get; set; }

        public ICollection<EditDelivery>? EditDeliveries { get; set; } //Many To Many  Edit DeliveryRep

        public ICollection<Order>? Orders { get; set; } //one To Many  show order


    }
}
