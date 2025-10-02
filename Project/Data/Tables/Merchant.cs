using Project.Data.Relation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Tables
{
    public class Merchant : Person
    {
        
        public required long NationalId { get; set; }

        public DateTime CreatDate { get; set; } = DateTime.Now;

        [NotMapped]  // Exclude Age from the database
        public int HireAge
        {
            get
            {
                var today = DateTime.Today;
                int HireAge = today.Year - CreatDate.Year;
                if (CreatDate.Date > today.AddYears(-HireAge))
                {
                    HireAge--;
                }
                return HireAge;
            }
        }

        //deliverd from feedbacks'product
        [NotMapped]
        public double Feedback => products?.SelectMany(p => p.feedbacks)
                                          .DefaultIfEmpty()
                                          .Average(f => f?.Star ?? 0) ?? 0;


        public ICollection<EditMerchant>? editMerchants { get; set; } //Many To Many  Edit Merchant

        public ICollection<FavMerchant>? FavMerchants { get; set; } //Many To Many  Edit Customer

        public ICollection<Product>? products { get; set; } // One to Many Add Product

        public ICollection<OrderItem>? orderItems { get; set; } // One to Many show own order 
    }
}
