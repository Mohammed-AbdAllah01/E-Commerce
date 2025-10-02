using Project.Data.Relation;
using Project.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Tables
{
    public class Product
    {
        public int Id { get; set; }

       
        public required string Description { get; set; }

       
        public required string Title { get; set; }

        public ProStatus Status { get; set; } = ProStatus.Pending;

        //deliverd from feedback from productComments
        public double Feedback => feedbacks?.Any() == true ? feedbacks.Average(f => f.Star) : 0;

        public double Discount { get; set; }

        public double UnitPrice { get; set; }

        [Required]
        public double SellPrice { get; set; }

        //--deliverd from ProductDetails
        [NotMapped]
        public int Quantity { 
            get { return ProductDetails?.Sum(pd => pd.Quantity) ?? 0; } }  

   

        [ForeignKey("category")] //Many To One
        public int categoryId { get; set; }
        public Category? category { get; set; }

        
        [ForeignKey("merchant")] //Many To One
        public required string merchantId { get; set; }
        public Merchant? merchant { get; set; }


        public ICollection<OrderItem>? orderItems { get; set; }//Many to Many

         public ICollection<Image>? images { get; set; }//Many to Many

        public ICollection <ProductDetail>? ProductDetails { get; set; } //Many To Many

        public ICollection<Cart>? Carts { get; set; } //Many To Many

        public ICollection<FavProduct>? favProducts { get; set; } //Many To Many

        public ICollection<Notification>? notifications { get; set; } //Many To Many

        public ICollection<Feedback>? feedbacks { get; set; } //Many To Many
        public ICollection<FeedbackComments>? feedbackcmments { get; set; }
        public void CalculateSellPrice()
        {
            SellPrice = UnitPrice * (1 - Discount / 100);
        }


        
    }
}
