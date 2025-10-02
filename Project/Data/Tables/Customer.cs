using Project.Data.Relation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Tables
{
    public class Customer : Person
    {



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


        public ICollection<EditCustomer>? editCustomers { get; set; } //Many To Many  Edit Customer

        public ICollection<FavMerchant>? favMerchants { get; set; } //Many To Many  favourite Merchant


        public ICollection<Order>? Orders { get; set; } //Many To Many  

        public ICollection<Cart>? Carts { get; set; }   //Many To Many

        public ICollection<FavProduct>? favProducts { get; set; }  //Many To Many 

        public ICollection<Notification>? notifications { get; set; }  //Many To Many 
        public ICollection<Feedback>? feedbacks { get; set; }  //Many To Many
        public ICollection<FeedbackComments>? feedbackcmments { get; set; }

        public ICollection<History>? histories { get; set; }  //Many To Many


    }
}
