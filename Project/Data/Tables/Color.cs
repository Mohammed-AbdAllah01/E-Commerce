using Project.Data.Relation;
using System.ComponentModel.DataAnnotations;

namespace Project.Tables
{
    public class Color
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public required string Name { get; set; }

        public ICollection<OrderItem>? OrderItems { get; set; } //Many to Many 

        public ICollection<Image>? images { get; set; }//Many to Many

        public ICollection<ProductDetail>? ProductDetails { get; set; }  //Many To Many  to Many
        public ICollection<Cart>? Carts { get; set; }  //Many To Many
    }
}
