using Project.Data.Relation;

namespace Project.Tables
{
    public class Size
    {
        public int Id {  get; set; }
        public required string Gradient { get; set; }

        public ICollection<OrderItem>? orderItems { get; set; }//Many to Many
        public ICollection<ProductDetail>? ProductDetails { get; set; } //Many To Many
        public ICollection<Cart>? Carts { get; set; } //Many To Many
    }
}
