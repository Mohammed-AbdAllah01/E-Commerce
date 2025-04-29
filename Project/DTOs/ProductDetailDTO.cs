using Project.Enums;
using Project.Tables;

namespace Project.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string title { get; set; }
        public ProStatus status { get; set; }
        public double Discount { get; set; }
        public double Unite { get; set; }
        public double SellPrice { get; set; }
        public string CategoryName { get; set; }
        public string Image { get; set; }
        public int Quantity { get; set; }
    }

        public class ProductDetailDTO
    {


        public int Id { get; set; }
        public string ProductName { get; set; }
            public string Color { get; set; }
            public string  Size { get; set; }
            public string Image { get; set; }
            public int Quantity { get; set; }

    }
}
