using Project.Enums;
using Project.Tables;
using System.ComponentModel.DataAnnotations;

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

   
        public class AddFullProductDTO
        {
            [Required]
            public string Title { get; set; }

            [Required]
            public string Description { get; set; }

            [Required]
            public int CategoryId { get; set; }

            [Required]
            [Range(0, double.MaxValue)]
            public double UnitPrice { get; set; }

            [Range(0, 1)]
            public double Discount { get; set; }

            [Required]
            public List<AddProductDetailDTO> ProductDetails { get; set; }


            [Required]
            public List<ColorImagesDTO> ColorImages { get; set; }

        }

        public class ColorImagesDTO
        {
            public int ColorId { get; set; }

            [Required]
            public List<string> ImageUrls { get; set; }
        }

        public class AddProductDetailDTO
        {
            public int ColorId { get; set; }
            public int SizeId { get; set; }
            public int Quantity { get; set; }
    }




        public class AddProductCommentDTO
        {
            public int ProductId { get; set; }

            [Required]
            [Range(1, 5)]
            public int Feedback { get; set; }

            [MaxLength(500)]
            public string Comment { get; set; }

            //  public double? Feeling { get; set; } 
        }





}
